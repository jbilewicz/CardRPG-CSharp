using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CardRPG.Core.Models;
using CardRPG.Services;

namespace CardRPG.UI;

public partial class CombatWindow : Window
{
    private readonly Player _player;
    private readonly Enemy _enemy;
    private readonly CombatEngine _engine;
    private List<Card> _hand = new();
    private readonly int _stage;
    private readonly int _totalStages;

    public CombatWindow(Player player, Enemy enemy, int stage = 1, int totalStages = 3)
    {
        InitializeComponent();
        _player = player;
        _enemy = enemy;
        _stage = stage;
        _totalStages = totalStages;
        _engine = new CombatEngine(player, enemy);
        SetEnemySprite();
        StartTurn();
    }

    private void SetEnemySprite()
    {
        string spritePath = _enemy.Name.ToLower() switch
        {
            var n when n.Contains("slime")                                    => "Assets/slime.png",
            var n when n.Contains("goblin")                                   => "Assets/goblin.png",
            var n when n.Contains("orc") || n.Contains("warlord") || _enemy.IsBoss => "Assets/arcwarlord.png",
            _                                                                  => "Assets/slime.png"
        };

        try
        {
            EnemySprite.Source = new BitmapImage(
                new Uri(spritePath, UriKind.Relative));
        }
        catch { }
    }

    private void StartTurn()
    {
        _player.CurrentMana = _player.MaxMana;
        _player.Armor = 0;
        _hand = _engine.DrawHand();
        RefreshUI();
        RenderHand();
        AddLog($"--- New Turn | Enemy: {_enemy.CurrentIntent} ({_enemy.IntentValue} dmg) ---");
    }

    private void RenderHand()
    {
        HandPanel.Children.Clear();
        foreach (var card in _hand)
        {
            bool canAfford = card.Cost <= _player.CurrentMana;

            var cardContent = new StackPanel { Width = 105 };
            cardContent.Children.Add(new TextBlock
            {
                Text = card.Type == CardType.Attack ? "⚔️" : "🛡️",
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            cardContent.Children.Add(new TextBlock
            {
                Text = card.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            cardContent.Children.Add(new TextBlock
            {
                Text = card.Description,
                FontSize = 10,
                Foreground = Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            cardContent.Children.Add(new TextBlock
            {
                Text = $"Cost: {card.Cost}",
                FontSize = 10,
                Foreground = canAfford ? Brushes.MediumPurple : Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 0)
            });

            var btn = new Button
            {
                Content = cardContent,
                Tag = card,
                Width = 115,
                Height = 100,
                Margin = new Thickness(4),
                IsEnabled = canAfford,
                Background = new SolidColorBrush(
                    card.Type == CardType.Attack
                        ? Color.FromRgb(80, 20, 20)
                        : Color.FromRgb(20, 40, 80)),
                BorderBrush = new SolidColorBrush(
                    card.Type == CardType.Attack ? Colors.OrangeRed : Colors.CornflowerBlue),
                BorderThickness = new Thickness(1),
                Foreground = Brushes.White,
                Padding = new Thickness(4)
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
            AddLog("❌ Not enough Mana!");
            return;
        }

        _player.CurrentMana -= card.Cost;

        int enemyHpBefore = _enemy.CurrentHp;
        int playerArmorBefore = _player.Armor;

        _engine.PlayCard(card);

        if (card.Type == CardType.Attack)
            AddLog($"⚔️ {card.Name} → {enemyHpBefore - _enemy.CurrentHp} damage to {_enemy.Name}.");
        else
            AddLog($"🛡️ {card.Name} → +{_player.Armor - playerArmorBefore} armor.");

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
            AddLog($"💢 {_enemy.Name} hit you for {damageTaken} damage.");
        else if (_enemy.CurrentIntent == EnemyIntent.Defend)
            AddLog($"⚕️ {_enemy.Name} healed itself.");
        else
            AddLog("💨 You dodged the attack!");

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
        RefreshUI();
        DialogResult = true;
        Close();
    }

    private void HandleDefeat()
    {
        _player.CurrentHp = 1;
        _player.Gold = Math.Max(0, _player.Gold - 10);
        RefreshUI();
        MessageBox.Show("You were defeated and dragged back to town.\n-10 Gold.", "Defeat");
        DialogResult = false;
        Close();
    }

    private void RefreshUI()
    {
        StageTxt.Text = $"Stage {_stage} / {_totalStages}";

        EnemyNameTxt.Text = _enemy.Name;
        EnemyNameSmallTxt.Text = _enemy.Name;
        EnemyIntentTxt.Text = $"Intent: {_enemy.CurrentIntent} ({_enemy.IntentValue})";
        EnemyHpBar.Maximum = _enemy.MaxHp;
        EnemyHpBar.Value = _enemy.CurrentHp;
        EnemyHpTxt.Text = $"{_enemy.CurrentHp} / {_enemy.MaxHp} HP";

        PlayerHpBar.Maximum = _player.MaxHp;
        PlayerHpBar.Value = _player.CurrentHp;
        PlayerHpTxt.Text = $"{_player.CurrentHp} / {_player.MaxHp} HP";
        PlayerManaTxt.Text = $"{_player.CurrentMana}/{_player.MaxMana}";
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
