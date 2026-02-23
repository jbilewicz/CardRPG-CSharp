using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using CardRPG.Core.Data;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class GameWindow : Window
{
    private readonly User _currentUser;
    private Player _player;

    private static readonly List<(string Name, int Hp, int Dmg, bool IsBoss)> Stages = new()
    {
        ("Green Slime",   30,  4, false),
        ("Goblin Scout",  50,  8, false),
        ("Orc Warlord",  120, 12, true),
    };

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
            try   { _player = JsonSerializer.Deserialize<Player>(_currentUser.SavedGameData)!; }
            catch { _player = CreateNewPlayer(); }
        }
        else { _player = CreateNewPlayer(); }
    }

    private Player CreateNewPlayer()
    {
        var p = new Player(_currentUser.Username);
        p.MasterDeck = new List<Card>
        {
            new Card("Strike", 1, CardType.Attack,  6),
            new Card("Strike", 1, CardType.Attack,  6),
            new Card("Defend", 1, CardType.Defense, 5),
            new Card("Defend", 1, CardType.Defense, 5),
        };
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
        HpTxt.Text   = $"{_player.CurrentHp}/{_player.MaxHp}";
        ManaTxt.Text = $"{_player.CurrentMana}/{_player.MaxMana}";
        GoldTxt.Text = _player.Gold.ToString();
    }

    private void JourneyButton_Click(object sender, RoutedEventArgs e)
    {
        int totalStages = Stages.Count;

        for (int i = 0; i < totalStages; i++)
        {
            var (name, hp, dmg, isBoss) = Stages[i];
            int stageNumber = i + 1;

            var enemy = new Enemy(name, hp, dmg, isBoss);
            var combat = new CombatWindow(_player, enemy, stageNumber, totalStages) { Owner = this };
            bool? result = combat.ShowDialog();

            if (result != true)
            {
                UpdateUI();
                SavePlayer();
                return;
            }

            // Stage rewards
            int heal = (int)(_player.MaxHp * 0.2);
            _player.Heal(heal);

            if (isBoss)
            {
                _player.Gold += 100;
                var reward = new Card("Execute", 2, CardType.Attack, 25);
                _player.MasterDeck.Add(reward);
                MessageBox.Show(
                    $"⚔️ BOSS DEFEATED!\n+100 Gold\n+NEW CARD: Execute (25 DMG)\n+{heal} HP restored.",
                    $"Stage {stageNumber}/{totalStages} Cleared!");
            }
            else
            {
                int gold = new Random().Next(10, 21);
                _player.Gold += gold;
                MessageBox.Show(
                    $"Stage {stageNumber}/{totalStages} cleared!\n+{gold} Gold\n+{heal} HP restored.",
                    "Victory!");
            }

            UpdateUI();
        }

        MessageBox.Show("JOURNEY COMPLETE! You return to the city as a hero. 🏆", "Journey Complete");
        SavePlayer();
        UpdateUI();
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
}