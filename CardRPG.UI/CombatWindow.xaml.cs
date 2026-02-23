using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CardRPG.Core.Models;
using CardRPG.Services;

namespace CardRPG.UI;

public partial class CombatWindow : Window
{
    private readonly Player _player;
    private readonly Enemy _enemy;
    private readonly CombatEngine _engine;
    private List<Card> _hand = new();

    public CombatWindow(Player player, Enemy enemy)
    {
        InitializeComponent();
        _player = player;
        _enemy = enemy;
        _engine = new CombatEngine(player, enemy);
        StartTurn();
    }

    private void StartTurn()
    {
        _player.Armor = 0;
        _hand = _engine.DrawHand();
        RefreshUI();
        RenderHand();
        AddLog($"--- New Turn | Enemy intent: {_enemy.CurrentIntent} ({_enemy.IntentValue}) ---");
    }

    private void RenderHand()
    {
        HandPanel.Children.Clear();
        foreach (var card in _hand)
        {
            var btn = new Button
            {
                Content = $"{card.Name}\n{card.Description}\nCost: {card.Cost}",
                Tag = card,
                Width = 110,
                Height = 90,
                Margin = new Thickness(5),
                IsEnabled = card.Cost <= _player.CurrentMana
            };
            btn.Click += CardButton_Click;
            HandPanel.Children.Add(btn);
        }
    }

    private async void CardButton_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        var card = (Card)btn.Tag;

        if (card.Cost > _player.CurrentMana)
        {
            AddLog("Not enough Mana!");
            return;
        }

        _player.CurrentMana -= card.Cost;

        int enemyHpBefore = _enemy.CurrentHp;
        int playerArmorBefore = _player.Armor;

        _engine.PlayCard(card);

        if (card.Type == CardType.Attack)
            AddLog($"You played {card.Name} → dealt {enemyHpBefore - _enemy.CurrentHp} damage to {_enemy.Name}.");
        else
            AddLog($"You played {card.Name} → gained {_player.Armor - playerArmorBefore} armor.");

        RefreshUI();

        if (_enemy.IsDead)
        {
            HandleVictory();
            return;
        }

        SetHandEnabled(false);
        await Task.Delay(1000);

        int playerHpBefore = _player.CurrentHp;
        _engine.EnemyTurn();
        int damageTaken = playerHpBefore - _player.CurrentHp;

        if (damageTaken > 0)
            AddLog($"{_enemy.Name} attacked you for {damageTaken} damage.");
        else if (_enemy.CurrentIntent == EnemyIntent.Defend)
            AddLog($"{_enemy.Name} healed itself.");
        else
            AddLog("You dodged the attack!");

        RefreshUI();

        if (_player.IsDead)
        {
            HandleDefeat();
            return;
        }

        SetHandEnabled(true);
        StartTurn();
    }

    private void HandleVictory()
    {
        int gold = 10 + new Random().Next(5, 15);
        _player.Gold += gold;
        AddLog($"Victory! You earned {gold} gold.");
        RefreshUI();
        MessageBox.Show($"Victory! You earned {gold} Gold.", "Victory!");
        DialogResult = true;
        Close();
    }

    private void HandleDefeat()
    {
        _player.CurrentHp = 1;
        AddLog("Defeat...");
        RefreshUI();
        MessageBox.Show("You were defeated...", "Defeat");
        DialogResult = false;
        Close();
    }

    private void RefreshUI()
    {
        EnemyNameTxt.Text = _enemy.Name;
        EnemyIntentTxt.Text = $"Intent: {_enemy.CurrentIntent} ({_enemy.IntentValue})";
        EnemyHpBar.Maximum = _enemy.MaxHp;
        EnemyHpBar.Value = _enemy.CurrentHp;
        EnemyHpTxt.Text = $"{_enemy.CurrentHp}/{_enemy.MaxHp}";

        PlayerHpBar.Maximum = _player.MaxHp;
        PlayerHpBar.Value = _player.CurrentHp;
        PlayerHpTxt.Text = $"HP: {_player.CurrentHp}/{_player.MaxHp}";
        PlayerManaTxt.Text = $"Mana: {_player.CurrentMana}/{_player.MaxMana}";
        PlayerArmorTxt.Text = $"Armor: {_player.Armor}";
    }

    private void SetHandEnabled(bool enabled)
    {
        foreach (UIElement child in HandPanel.Children)
            child.IsEnabled = enabled;
    }

    private void AddLog(string message)
    {
        CombatLog.Items.Insert(0, message);
    }
}
