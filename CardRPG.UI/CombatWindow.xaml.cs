using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
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

        // Setup RenderTransforms for shake animations
        EnemySprite.RenderTransform = new TranslateTransform();
        PlayerSprite.RenderTransform = new TranslateTransform();

        SetEnemySprite();
        StartTurn();
    }

    private void SetEnemySprite()
    {
        string spritePath = _enemy.Name.ToLower() switch
        {
            var n when n.Contains("slime")                                         => "Assets/slime.png",
            var n when n.Contains("goblin")                                        => "Assets/goblin.png",
            var n when n.Contains("orc") || n.Contains("warlord") || _enemy.IsBoss => "Assets/arcwarlord.png",
            _                                                                       => "Assets/slime.png"
        };

        try { EnemySprite.Source = new BitmapImage(new Uri(spritePath, UriKind.Relative)); }
        catch { }
    }

    private void StartTurn()
    {
        _player.CurrentMana = _player.MaxMana;
        _player.Armor = 0;
        _hand = _engine.DrawHand();
        RefreshUI();
        RenderHand();
        AddLog($"── New Turn | Enemy intent: {_enemy.CurrentIntent} ({_enemy.IntentValue}) ──", "#777777");
    }

    // ═══════════════════════════════════════════
    //  CARD RENDERING — Enhanced RPG-style cards
    // ═══════════════════════════════════════════
    private void RenderHand()
    {
        HandPanel.Children.Clear();
        foreach (var card in _hand)
        {
            bool canAfford = card.Cost <= _player.CurrentMana;
            bool isAttack = card.Type == CardType.Attack;

            // Card content layout
            var cardStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Type icon
            cardStack.Children.Add(new TextBlock
            {
                Text = isAttack ? "⚔️" : "🛡️",
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 4)
            });

            // Card name
            cardStack.Children.Add(new TextBlock
            {
                Text = card.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            // Calculated value
            int displayValue = isAttack
                ? card.Value + _player.GetTotalDamage()
                : card.Value + (_player.Agility / 2);
            cardStack.Children.Add(new TextBlock
            {
                Text = isAttack ? $"{displayValue} DMG" : $"{displayValue} ARM",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(isAttack
                    ? Color.FromRgb(0xFF, 0xAA, 0xAA)
                    : Color.FromRgb(0xAA, 0xCC, 0xFF)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            });

            // Separator line
            cardStack.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromArgb(0x44, 0xFF, 0xFF, 0xFF)),
                Margin = new Thickness(6, 8, 6, 6)
            });

            // Mana cost
            var costPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            costPanel.Children.Add(new TextBlock { Text = "💜 ", FontSize = 12 });
            costPanel.Children.Add(new TextBlock
            {
                Text = card.Cost.ToString(),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = canAfford ? Brushes.MediumPurple : Brushes.Gray
            });
            cardStack.Children.Add(costPanel);

            // The card button
            var btn = new Button
            {
                Content = cardStack,
                Tag = card,
                Width = 130,
                Height = 170,
                Margin = new Thickness(6),
                IsEnabled = canAfford,
                Foreground = Brushes.White,
                Background = new LinearGradientBrush(
                    isAttack ? Color.FromRgb(0x50, 0x14, 0x14) : Color.FromRgb(0x14, 0x20, 0x50),
                    isAttack ? Color.FromRgb(0x28, 0x08, 0x08) : Color.FromRgb(0x08, 0x10, 0x28),
                    90),
                BorderBrush = new SolidColorBrush(isAttack
                    ? Color.FromRgb(0xFF, 0x44, 0x44)
                    : Color.FromRgb(0x44, 0x88, 0xFF)),
            };

            // Apply CardButton style for hover/press effects
            if (TryFindResource("CardButton") is Style cardStyle)
                btn.Style = cardStyle;

            btn.Click += CardButton_Click;
            HandPanel.Children.Add(btn);
        }
    }

    // ═══════════════════════════════════════════
    //  CARD PLAY — with animations
    // ═══════════════════════════════════════════
    private async void CardButton_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        var card = (Card)btn.Tag;

        if (card.Cost > _player.CurrentMana)
        {
            AddLog("❌ Not enough Mana!", "#FF4444");
            return;
        }

        _player.CurrentMana -= card.Cost;

        int enemyHpBefore = _enemy.CurrentHp;
        int playerArmorBefore = _player.Armor;

        _engine.PlayCard(card);

        if (card.Type == CardType.Attack)
        {
            int dmgDealt = enemyHpBefore - _enemy.CurrentHp;
            AddLog($"⚔️ {card.Name} → {dmgDealt} damage to {_enemy.Name}!", "#FF8888");
            // Shake enemy + floating damage
            _ = ShakeElement(EnemySprite);
            _ = ShowFloatingDamage(EnemyDmgPopup, dmgDealt);
        }
        else
        {
            int armorGained = _player.Armor - playerArmorBefore;
            AddLog($"🛡️ {card.Name} → +{armorGained} armor.", "#8888FF");
        }

        RefreshUI();
        RenderHand();

        if (_enemy.IsDead)
        {
            AddLog($"💀 {_enemy.Name} defeated!", "#FFD700");
            await Task.Delay(800);
            HandleVictory();
            return;
        }

        // ── Enemy turn ──
        SetHandEnabled(false);
        AddLog("...", "#555555");
        await Task.Delay(1000);

        int playerHpBefore = _player.CurrentHp;
        _engine.EnemyTurn();
        int damageTaken = playerHpBefore - _player.CurrentHp;

        if (damageTaken > 0)
        {
            AddLog($"💢 {_enemy.Name} hit you for {damageTaken} damage!", "#FF4444");
            _ = FlashHitOverlay();
            _ = ShakeElement(PlayerSprite);
            _ = ShowFloatingDamage(PlayerDmgPopup, damageTaken);
        }
        else if (_enemy.CurrentIntent == EnemyIntent.Defend)
        {
            AddLog($"⚕️ {_enemy.Name} healed itself.", "#88FF88");
        }
        else
        {
            AddLog("💨 You dodged the attack!", "#88FF88");
        }

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
        EnemyHpBar.Value = Math.Max(0, _enemy.CurrentHp);
        EnemyHpTxt.Text = $"{_enemy.CurrentHp} / {_enemy.MaxHp} HP";

        PlayerHpBar.Maximum = _player.MaxHp;
        PlayerHpBar.Value = Math.Max(0, _player.CurrentHp);
        PlayerHpTxt.Text = $"{_player.CurrentHp} / {_player.MaxHp} HP";
        PlayerManaTxt.Text = $"Mana: {_player.CurrentMana}/{_player.MaxMana}";
        PlayerArmorTxt.Text = $"Armor: {_player.Armor}";
    }

    private void SetHandEnabled(bool enabled)
    {
        foreach (UIElement child in HandPanel.Children)
            child.IsEnabled = enabled;
    }

    // ═══════════════════════════════════════════
    //  COLORED COMBAT LOG
    // ═══════════════════════════════════════════
    private void AddLog(string message, string color = "#AAAAAA")
    {
        var tb = new TextBlock
        {
            Text = message,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
            FontSize = 12,
            Padding = new Thickness(4, 2, 4, 2)
        };
        CombatLog.Items.Insert(0, tb);
        if (CombatLog.Items.Count > 50)
            CombatLog.Items.RemoveAt(50);
    }

    // ═══════════════════════════════════════════
    //  COMBAT ANIMATIONS
    // ═══════════════════════════════════════════

    /// <summary>Shake an element left-right rapidly.</summary>
    private async Task ShakeElement(FrameworkElement element)
    {
        if (element.RenderTransform is not TranslateTransform transform)
        {
            transform = new TranslateTransform();
            element.RenderTransform = transform;
        }

        int[] offsets = [12, -12, 10, -10, 6, -6, 3, -3, 0];
        foreach (int offset in offsets)
        {
            transform.X = offset;
            await Task.Delay(30);
        }
    }

    /// <summary>Flash the screen red when the player takes a hit.</summary>
    private async Task FlashHitOverlay()
    {
        HitFlash.Opacity = 0.4;
        await Task.Delay(70);
        HitFlash.Opacity = 0.25;
        await Task.Delay(70);
        HitFlash.Opacity = 0.12;
        await Task.Delay(60);
        HitFlash.Opacity = 0;
    }

    /// <summary>Show a floating damage number that rises and fades away.</summary>
    private async Task ShowFloatingDamage(TextBlock popup, int damage)
    {
        popup.Text = $"-{damage}";
        popup.Opacity = 1;
        popup.RenderTransform = new TranslateTransform(0, 0);

        for (int i = 0; i < 20; i++)
        {
            popup.Opacity = Math.Max(0, 1.0 - (i * 0.05));
            ((TranslateTransform)popup.RenderTransform).Y = -(i * 2.5);
            await Task.Delay(28);
        }
        popup.Opacity = 0;
    }
}
