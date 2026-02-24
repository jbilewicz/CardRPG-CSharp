using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using CardRPG.Core.Data;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class GameWindow : Window
{
    private readonly User _currentUser;
    private Player _player;

    public GameWindow(User user)
    {
        InitializeComponent();
        _currentUser = user;
        LoadPlayer();
        UpdateUI();
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
        HpTxt.Text = $"{_player.CurrentHp}/{_player.MaxHp}";
        ManaTxt.Text = $"{_player.CurrentMana}/{_player.MaxMana}";
        GoldTxt.Text = _player.Gold.ToString();
    }

    private void NameTxt_Click(object sender, MouseButtonEventArgs e)
    {
        var statsWin = new PlayerStatsWindow(_player) { Owner = this };
        statsWin.ShowDialog();
    }

    private void JourneyButton_Click(object sender, RoutedEventArgs e)
    {
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
            var realm = Realm.GetAllRealms().FirstOrDefault(r => r.Id == realmId);
            if (realm == null) return;

            var rng = new Random();
            int totalStages = realm.Stages.Count;

            for (int i = 0; i < totalStages; i++)
            {
                var stage = realm.Stages[i];
                int stageNumber = i + 1;

                var enemy = new Enemy(stage.EnemyName, stage.EnemyHp, stage.EnemyDmg, stage.IsBoss);
                var combat = new CombatWindow(_player, enemy, stageNumber, totalStages, realm.Name) { Owner = this };
                bool? result = combat.ShowDialog();

                if (result != true)
                {
                    UpdateUI();
                    SavePlayer();
                    return;
                }

                int heal = (int)(_player.MaxHp * 0.25);
                _player.Heal(heal);

                if (stage.IsBoss)
                {
                    _player.Gold += realm.GoldReward;
                    bool leveledUp = _player.GainXp(realm.XpReward);

                    string msg = $"{realm.Name} CONQUERED!\n\n" +
                                 $"+{realm.GoldReward} Gold\n" +
                                 $"+{realm.XpReward} XP\n" +
                                 $"+{heal} HP restored";

                    if (realm.CardReward != null)
                    {
                        _player.MasterDeck.Add(realm.CardReward.Clone());
                        msg += $"\nNEW CARD: {realm.CardReward.Name}!";
                    }

                    var bossCard = CardLibrary.GetBossDropCard(rng, realm.Id);
                    _player.MasterDeck.Add(bossCard);
                    msg += $"\nBOSS DROP: {bossCard.Name} [{bossCard.Rarity}]!";

                    if (leveledUp)
                        msg += $"\n\nLEVEL UP! You are now Level {_player.Level}!";

                    if (realm.Id >= _player.MaxRealmUnlocked && realm.Id < 10)
                    {
                        _player.MaxRealmUnlocked = realm.Id + 1;
                        var nextRealm = Realm.GetAllRealms().FirstOrDefault(r => r.Id == realm.Id + 1);
                        if (nextRealm != null)
                            msg += $"\nNew realm unlocked: {nextRealm.Name}!";
                    }

                    MessageBox.Show(msg, $"Realm {realm.Id} Complete!");
                }
                else
                {
                    int gold = rng.Next(8, 18);
                    int xp = realm.XpReward / totalStages;
                    _player.Gold += gold;
                    bool leveledUp = _player.GainXp(xp);

                    string msg = $"Stage {stageNumber}/{totalStages} cleared!\n+{gold} Gold\n+{xp} XP\n+{heal} HP restored";
                    if (leveledUp)
                        msg += $"\n\nLEVEL UP! You are now Level {_player.Level}!";

                    MessageBox.Show(msg, "Victory!");
                }

                UpdateUI();
            }

            SavePlayer();
            UpdateUI();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}", "Error");
            SavePlayer();
            UpdateUI();
        }
    }

    private void ShopButton_Click(object sender, RoutedEventArgs e)
    {
        var shop = new ShopWindow(_player) { Owner = this };
        shop.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void TavernButton_Click(object sender, RoutedEventArgs e)
    {
        var tavern = new TavernWindow(_player) { Owner = this };
        tavern.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void InventoryButton_Click(object sender, RoutedEventArgs e)
    {
        var inv = new InventoryWindow(_player) { Owner = this };
        inv.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void ArenaButton_Click(object sender, RoutedEventArgs e)
    {
        var arena = new ArenaWindow(_player) { Owner = this };
        arena.ShowDialog();
        SavePlayer();
        UpdateUI();
    }
}
