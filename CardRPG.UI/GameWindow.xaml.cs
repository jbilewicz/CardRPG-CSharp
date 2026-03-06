using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CardRPG.Core.Data;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class GameWindow : Window
{
    private readonly User _currentUser;
    private Player _player;
    private bool _settingsOpen;

    public GameWindow(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadPlayer();
        UpdateUI();
        SyncSettingsUI();
        AudioManager.PlayMenuMusic();
        PlayEntranceAnimation();
    }

    private void PlayEntranceAnimation()
    {
        this.Opacity = 0;
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        this.BeginAnimation(OpacityProperty, fadeIn);
    }

    private void LoadPlayer()
    {
        if (!string.IsNullOrEmpty(_currentUser.SavedGameData))
        {
            try { _player = JsonSerializer.Deserialize<Player>(_currentUser.SavedGameData)!; }
            catch { _player = CreateNewPlayer(); }
        }
        else { _player = CreateNewPlayer(); }
    }

    private Player CreateNewPlayer()
    {
        var p = new Player(_currentUser.Username);
        p.MasterDeck = CardLibrary.GetStarterDeck();
        return p;
    }

    private void SavePlayer()
    {
        using var db = new GameDBContext();
        var userInDb = db.Users.FirstOrDefault(u => u.Id == _currentUser.Id);
        if (userInDb != null)
        {
            userInDb.SavedGameData = JsonSerializer.Serialize(_player);
            db.SaveChanges();
        }
    }

    private void UpdateUI()
    {
        NameTxt.Text = _player.Name;
        LevelTxt.Text = _player.Level.ToString();
        HpTxt.Text = $"{_player.CurrentHp}/{_player.GetEffectiveMaxHp()}";
        ManaTxt.Text = $"{_player.CurrentMana}/{_player.GetEffectiveMaxMana()}";
        GoldTxt.Text = _player.Gold.ToString();
    }

    private void NameTxt_Click(object sender, MouseButtonEventArgs e)
    {
        var statsWin = new PlayerStatsWindow(_player) { Owner = this };
        statsWin.ShowDialog();
    }

    private void JourneyButton_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        try
        {
            var journeyWin = new JourneyWindow(_player) { Owner = this };
            bool? picked = journeyWin.ShowDialog();

            if (picked != true || journeyWin.SelectedRealmId == null)
            {
                UpdateUI();
                return;
            }

            int realmId = journeyWin.SelectedRealmId.Value;
            int difficulty = journeyWin.SelectedDifficulty;
            var realm = Realm.GetAllRealms().FirstOrDefault(r => r.Id == realmId);
            if (realm == null) return;

            double hpMult = difficulty switch { 0 => 0.7, 2 => 1.5, _ => 1.0 };
            double dmgMult = difficulty switch { 0 => 0.7, 2 => 1.4, _ => 1.0 };
            double rewardMult = difficulty switch { 0 => 0.8, 2 => 1.5, _ => 1.0 };

            AudioManager.StopMenuMusic();

            var rng = new Random();
            int totalStages = realm.Stages.Count;

            int cumulativeGold = 0;
            int cumulativeXp = 0;
            int cumulativeKills = 0;
            var cumulativeCards = new List<string>();

            for (int i = 0; i < totalStages; i++)
            {
                var stage = realm.Stages[i];
                int stageNumber = i + 1;

                int scaledHp = (int)(stage.EnemyHp * hpMult);
                int scaledDmg = (int)(stage.EnemyDmg * dmgMult);
                var enemy = new Enemy(stage.EnemyName, scaledHp, scaledDmg, stage.IsBoss);
                var combat = new CombatWindow(_player, enemy, stageNumber, totalStages, realm.Name) { Owner = this };
                bool? result = combat.ShowDialog();

                if (result != true)
                {
                    AudioManager.PlayMenuMusic();
                    UpdateUI();
                    SavePlayer();
                    return;
                }

                // Track kill stats
                _player.TotalKills++;
                cumulativeKills++;
                if (stage.IsBoss) _player.TotalBossKills++;
                Quest.TrackKill(_player, stage.EnemyName, stage.IsBoss);

                int heal = (int)(_player.GetEffectiveMaxHp() * 0.25);
                _player.Heal(heal);

                if (stage.IsBoss)
                {
                    int goldReward = (int)(realm.GoldReward * rewardMult);
                    int xpReward = (int)(realm.XpReward * rewardMult);
                    _player.Gold += goldReward;
                    _player.TotalGoldEarned += goldReward;
                    bool leveledUp = _player.GainXp(xpReward);
                    cumulativeGold += goldReward;
                    cumulativeXp += xpReward;

                    string diffLabel = difficulty switch { 0 => " [Easy]", 2 => " [Hard]", _ => "" };

                    string? cardRewardName = null;
                    if (realm.CardReward != null)
                    {
                        _player.MasterDeck.Add(realm.CardReward.Clone());
                        _player.TotalCardsCollected++;
                        cardRewardName = realm.CardReward.Name;
                        cumulativeCards.Add($"{realm.CardReward.Name} [{realm.CardReward.Rarity}]");
                    }

                    var bossCard = CardLibrary.GetBossDropCard(rng, realm.Id);
                    _player.MasterDeck.Add(bossCard);
                    _player.TotalCardsCollected++;
                    cumulativeCards.Add($"{bossCard.Name} [{bossCard.Rarity}]");

                    string? newRealmName = null;
                    if (realm.Id >= _player.MaxRealmUnlocked && realm.Id < 25)
                    {
                        _player.MaxRealmUnlocked = realm.Id + 1;
                        _player.RealmsCompleted++;
                        var nextRealm = Realm.GetAllRealms().FirstOrDefault(r => r.Id == realm.Id + 1);
                        if (nextRealm != null)
                            newRealmName = nextRealm.Name;
                    }

                    VictoryWindow.ShowRealmVictory(this, realm.Name, realm.Id,
                        diffLabel, goldReward, xpReward, heal,
                        cardRewardName, bossCard.Name, bossCard.Rarity.ToString(),
                        leveledUp, _player.Level, newRealmName,
                        cumulativeGold, cumulativeXp, cumulativeKills, cumulativeCards);
                }
                else
                {
                    int gold = (int)(rng.Next(8, 18) * rewardMult);
                    int xp = (int)((realm.XpReward / totalStages) * rewardMult);
                    _player.Gold += gold;
                    _player.TotalGoldEarned += gold;
                    bool leveledUp = _player.GainXp(xp);
                    cumulativeGold += gold;
                    cumulativeXp += xp;

                    VictoryWindow.ShowStageVictory(this, stageNumber, totalStages,
                        gold, xp, heal, leveledUp, _player.Level);
                }

                UpdateUI();
            }

            SavePlayer();
            AudioManager.PlayMenuMusic();
            UpdateUI();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error");
            AudioManager.PlayMenuMusic();
            SavePlayer();
            UpdateUI();
        }
    }

    private void ShopButton_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        var shop = new ShopWindow(_player) { Owner = this };
        shop.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void TavernButton_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        var tavern = new TavernWindow(_player) { Owner = this };
        tavern.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void InventoryButton_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        var inv = new InventoryWindow(_player) { Owner = this };
        inv.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void ArenaButton_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        var arena = new ArenaWindow(_player) { Owner = this };
        arena.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void TalentsButton_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        var win = new TalentWindow(_player) { Owner = this };
        win.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void CraftButton_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        var win = new CraftWindow(_player) { Owner = this };
        win.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void EnchantButton_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        var win = new EnchantWindow(_player) { Owner = this };
        win.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void AchievementsButton_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        var win = new AchievementWindow(_player) { Owner = this };
        win.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    // --- Settings / Audio ---

    private void SettingsBtn_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        _settingsOpen = !_settingsOpen;
        SettingsPanel.Visibility = _settingsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (_settingsOpen) SyncSettingsUI();
    }

    private void CloseSettings_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.PlayButtonClick();
        _settingsOpen = false;
        SettingsPanel.Visibility = Visibility.Collapsed;
    }

    private void SyncSettingsUI()
    {
        MasterSlider.Value = AudioManager.MasterVolume * 100;
        MenuSlider.Value = AudioManager.MenuVolume * 100;
        BattleSlider.Value = AudioManager.BattleVolume * 100;
        MenuMuteBtn.Content = AudioManager.MenuMuted ? "🔇" : "🔊";
        BattleMuteBtnSettings.Content = AudioManager.BattleMuted ? "🔇" : "🔊";
    }

    private void MasterSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        AudioManager.MasterVolume = e.NewValue / 100.0;
    }

    private void MenuSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        AudioManager.MenuVolume = e.NewValue / 100.0;
    }

    private void BattleSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        AudioManager.BattleVolume = e.NewValue / 100.0;
    }

    private void MenuMuteBtn_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.MenuMuted = !AudioManager.MenuMuted;
        MenuMuteBtn.Content = AudioManager.MenuMuted ? "🔇" : "🔊";
    }

    private void BattleMuteBtnSettings_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.BattleMuted = !AudioManager.BattleMuted;
        BattleMuteBtnSettings.Content = AudioManager.BattleMuted ? "🔇" : "🔊";
    }
}
