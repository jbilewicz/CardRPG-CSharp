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

    private static readonly List<(string Name, int Hp, int Dmg)> EnemyPool = new()
    {
        ("Goblin",       40,  8),
        ("Orc Warrior",  70, 12),
        ("Dark Mage",    55, 15),
        ("Skeleton",     45, 10),
        ("Troll",       100, 18),
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
            try
            {
                _player = JsonSerializer.Deserialize<Player>(_currentUser.SavedGameData)!;
            }
            catch
            {
                _player = CreateNewPlayer();
            }
        }
        else
        {
            _player = CreateNewPlayer();
        }
    }

    private Player CreateNewPlayer()
    {
        var p = new Player(_currentUser.Username);
        p.MasterDeck = new List<Card>
        {
            new Card("Strike",  1, CardType.Attack,  6),
            new Card("Strike",  1, CardType.Attack,  6),
            new Card("Defend",  1, CardType.Defense, 5),
            new Card("Defend",  1, CardType.Defense, 5),
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
        var rng   = new Random();
        var pick  = EnemyPool[rng.Next(EnemyPool.Count)];
        var enemy = new Enemy(pick.Name, pick.Hp, pick.Dmg);

        var combat = new CombatWindow(_player, enemy) { Owner = this };
        combat.ShowDialog();

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
        const int cost = 10;
        const int heal = 30;

        if (_player.Gold < cost)
        {
            MessageBox.Show($"You need {cost} gold to rest here.", "Tavern");
            return;
        }

        _player.Gold    -= cost;
        _player.Heal(heal);
        SavePlayer();
        UpdateUI();
        MessageBox.Show($"You rested at the tavern. (+{heal} HP)", "Tavern");
    }

    private void InventoryButton_Click(object sender, RoutedEventArgs e)
    {
        var inv = new InventoryWindow(_player) { Owner = this };
        inv.ShowDialog();
        SavePlayer();
        UpdateUI();
    }
}