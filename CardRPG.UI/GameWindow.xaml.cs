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
        var classWin = new ClassSelectionWindow();
        classWin.ShowDialog();
        var selectedClass = classWin.SelectedClass;

        var p = new Player(_currentUser.Username, selectedClass);
        p.MasterDeck = CardLibrary.GetStarterDeck(selectedClass);
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

                // Random event before non-boss, non-first stages (40% chance)
                if (i > 0 && !stage.IsBoss && rng.Next(100) < 40)
                {
                    var journeyEvent = JourneyEvent.GetRandomEvent(rng);
                    var eventWin = new EventWindow(_player, journeyEvent) { Owner = this };
                    eventWin.ShowDialog();

                    if (_player.IsDead)
                    {
                        _player.CurrentHp = 1;
                        MessageBox.Show("The event left you barely alive. Returning to town.", "Close Call");
                        SavePlayer();
                        UpdateUI();
                        return;
                    }

                    UpdateUI();
                }

                // Mini-Boss encounter (30% chance between stages, never before stage 1 or boss)
                if (i > 0 && !stage.IsBoss && rng.Next(100) < 30)
                {
                    var elite = Enemy.GetRandomElite(rng, realmId);
                    var eliteCombat = new CombatWindow(_player, elite, stageNumber, totalStages, realm.Name + " [ELITE]") { Owner = this };
                    bool? eliteResult = eliteCombat.ShowDialog();

                    if (eliteResult != true)
                    {
                        UpdateUI();
                        SavePlayer();
                        return;
                    }

                    // Elite rewards: bonus gold, XP, and card
                    int eliteGold = rng.Next(20, 40) + realmId * 5;
                    int eliteXp = 30 + realmId * 10;
                    _player.Gold += eliteGold;
                    _player.Stats.TotalGoldEarned += eliteGold;
                    _player.Stats.ElitesKilled++;
                    bool eLeveled = _player.GainXp(eliteXp);
                    int eHeal = (int)(_player.MaxHp * 0.15);
                    _player.Heal(eHeal);

                    string eliteMsg = $"ELITE DEFEATED!\n+{eliteGold} Gold\n+{eliteXp} XP\n+{eHeal} HP restored";
                    if (eLeveled) eliteMsg += $"\nLEVEL UP! Level {_player.Level}!";

                    // 60% chance for bonus card from elite
                    if (rng.Next(100) < 60)
                    {
                        var eliteCard = CardLibrary.GetBossDropCard(rng, Math.Max(1, realmId - 1));
                        _player.MasterDeck.Add(eliteCard);
                        eliteMsg += $"\nELITE DROP: {eliteCard.Name} [{eliteCard.Rarity}]!";
                    }

                    MessageBox.Show(eliteMsg, "Elite Victory!");
                    UpdateUI();
                }

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

                // Card reward (pick 1 of 3) after each victory
                var cardChoices = new List<Card>();
                for (int c = 0; c < 3; c++)
                    cardChoices.Add(CardLibrary.GetRandomCard(rng, _player.Level));

                var rewardWin = new CardRewardWindow(cardChoices) { Owner = this };
                if (rewardWin.ShowDialog() == true && rewardWin.SelectedCard != null)
                {
                    _player.MasterDeck.Add(rewardWin.SelectedCard);
                }

                if (stage.IsBoss)
                {
                    _player.Gold += realm.GoldReward;
                    _player.Stats.TotalGoldEarned += realm.GoldReward;
                    _player.Stats.BossesKilled++;
                    _player.Stats.EnemiesKilled++;
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
                        _player.Stats.RealmsCompleted++;
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
                    _player.Stats.TotalGoldEarned += gold;
                    _player.Stats.EnemiesKilled++;
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

    private void TalentsButton_Click(object sender, RoutedEventArgs e)
    {
        var talentWin = new TalentTreeWindow(_player) { Owner = this };
        talentWin.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void DeckButton_Click(object sender, RoutedEventArgs e)
    {
        var deckWin = new DeckManagerWindow(_player) { Owner = this };
        deckWin.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void ForgeButton_Click(object sender, RoutedEventArgs e)
    {
        var forgeWin = new FusionWindow(_player) { Owner = this };
        forgeWin.ShowDialog();
        SavePlayer();
        UpdateUI();
    }

    private void EnchantButton_Click(object sender, RoutedEventArgs e)
    {
        var enchantWin = new EnchantWindow(_player) { Owner = this };
        enchantWin.ShowDialog();
        SavePlayer();
        UpdateUI();
    }
}
