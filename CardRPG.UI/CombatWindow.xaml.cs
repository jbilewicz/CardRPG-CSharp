using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using CardRPG.Core.Models;
using CardRPG.Core.Services;

namespace CardRPG.UI;

public partial class CombatWindow : Window
{
    private readonly Player _player;
    private readonly Enemy _enemy;
    private readonly CombatEngine _engine;
    private List<Card> _hand = new();
    private readonly int _stage;
    private readonly int _totalStages;
    private readonly string _realmName;
    private bool _turnInProgress = false;

    public CombatWindow(Player player, Enemy enemy, int stage = 1, int totalStages = 3, string realmName = "")
    {
        InitializeComponent();
        _player = player;
        _enemy = enemy;
        _stage = stage;
        _totalStages = totalStages;
        _realmName = realmName;
        _engine = new CombatEngine(player, enemy);

        EnemySprite.RenderTransform = new TranslateTransform();
        PlayerSprite.RenderTransform = new TranslateTransform();

        SetEnemySprite();
        StartTurn();
    }

    private void SetEnemySprite()
    {
        string spritePath = _enemy.Name.ToLower() switch
        {
            var n when n.Contains("slime") => "Assets/slime.png",
            var n when n.Contains("goblin") => "Assets/goblin.png",
            var n when n.Contains("wolf") || n.Contains("bear") => "Assets/slime.png",
            var n when n.Contains("spider") || n.Contains("treant") => "Assets/goblin.png",
            var n when n.Contains("dragon") || n.Contains("drake") || n.Contains("wyrm") => "Assets/arcwarlord.png",
            var n when n.Contains("demon") || n.Contains("void") || n.Contains("abyss") => "Assets/arcwarlord.png",
            var n when n.Contains("orc") || n.Contains("warlord") || _enemy.IsBoss => "Assets/arcwarlord.png",
            _ => "Assets/slime.png"
        };

        try { EnemySprite.Source = new BitmapImage(new Uri(spritePath, UriKind.Relative)); }
        catch { }
    }

    private void StartTurn()
    {
        _player.CurrentMana = _player.MaxMana;
        _player.Armor = 0;
        _hand = _engine.DrawHand();
        _turnInProgress = false;
        EndTurnBtn.IsEnabled = true;
        RefreshUI();
        RenderHand();
        string realmLabel = string.IsNullOrEmpty(_realmName) ? "" : $" [{_realmName}]";
        AddLog($"── Turn Start{realmLabel} | Enemy: {_enemy.CurrentIntent} ({_enemy.IntentValue}) ──", "#777777");
    }

    private void RenderHand()
    {
        HandPanel.Children.Clear();
        foreach (var card in _hand)
        {
            bool canAfford = card.Cost <= _player.CurrentMana;
            bool isAttack = card.Type == CardType.Attack;
            bool isPower = card.Type == CardType.Power;

            var cardStack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            string icon = isAttack ? "⚔️" : isPower ? "✨" : "🛡️";
            cardStack.Children.Add(new TextBlock
            {
                Text = icon,
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 4)
            });

            cardStack.Children.Add(new TextBlock
            {
                Text = card.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            string valueLabel;
            Color valueColor;
            if (isAttack)
            {
                int displayValue = card.Value + _player.GetTotalDamage();
                valueLabel = $"{displayValue} DMG";
                valueColor = Color.FromRgb(0xFF, 0xAA, 0xAA);
            }
            else if (isPower)
            {
                valueLabel = $"{card.Value} HP";
                valueColor = Color.FromRgb(0xAA, 0xFF, 0xAA);
            }
            else
            {
                int displayValue = card.Value + (_player.Agility / 2);
                valueLabel = $"{displayValue} ARM";
                valueColor = Color.FromRgb(0xAA, 0xCC, 0xFF);
            }

            cardStack.Children.Add(new TextBlock
            {
                Text = valueLabel,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(valueColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            });

            cardStack.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromArgb(0x44, 0xFF, 0xFF, 0xFF)),
                Margin = new Thickness(6, 8, 6, 6)
            });

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

            Color bgStart = isAttack ? Color.FromRgb(0x50, 0x14, 0x14)
                          : isPower ? Color.FromRgb(0x14, 0x50, 0x14)
                          : Color.FromRgb(0x14, 0x20, 0x50);
            Color bgEnd = isAttack ? Color.FromRgb(0x28, 0x08, 0x08)
                        : isPower ? Color.FromRgb(0x08, 0x28, 0x08)
                        : Color.FromRgb(0x08, 0x10, 0x28);
            Color borderColor = isAttack ? Color.FromRgb(0xFF, 0x44, 0x44)
                              : isPower ? Color.FromRgb(0x44, 0xFF, 0x44)
                              : Color.FromRgb(0x44, 0x88, 0xFF);

            var btn = new Button
            {
                Content = cardStack,
                Tag = card,
                Width = 130,
                Height = 170,
                Margin = new Thickness(6),
                IsEnabled = canAfford && !_turnInProgress,
                Foreground = Brushes.White,
                Background = new LinearGradientBrush(bgStart, bgEnd, 90),
                BorderBrush = new SolidColorBrush(borderColor),
            };

            if (TryFindResource("CardButton") is Style cardStyle)
                btn.Style = cardStyle;

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
            AddLog("Not enough Mana!", "#FF4444");
            return;
        }

        var result = _engine.PlayCard(card);
        _hand.Remove(card);

        if (card.Type == CardType.Attack)
        {
            string color = result.IsCrit ? "#FFD700" : "#FF8888";
            AddLog($"⚔️ {result.Message}", color);
            _ = ShakeElement(EnemySprite);
            _ = ShowFloatingDamage(EnemyDmgPopup, result.DamageDealt);
        }
        else if (card.Type == CardType.Power)
        {
            AddLog($"✨ {result.Message}", "#88FF88");
        }
        else
        {
            AddLog($"🛡️ {result.Message}", "#8888FF");
        }

        RefreshUI();
        RenderHand();

        if (_enemy.IsDead)
        {
            AddLog($"💀 {_enemy.Name} defeated!", "#FFD700");
            EndTurnBtn.IsEnabled = false;
            await Task.Delay(800);
            HandleVictory();
            return;
        }

        if (_player.CurrentMana <= 0 || _hand.Count == 0)
        {
            await ExecuteEnemyTurn();
        }
    }

    private async void EndTurnButton_Click(object sender, RoutedEventArgs e)
    {
        if (_turnInProgress) return;
        await ExecuteEnemyTurn();
    }

    private async Task ExecuteEnemyTurn()
    {
        _turnInProgress = true;
        EndTurnBtn.IsEnabled = false;
        SetHandEnabled(false);

        AddLog("...", "#555555");
        await Task.Delay(800);

        var result = _engine.EnemyTurn();

        if (result.IsDodge)
        {
            AddLog($"💨 {result.Message}", "#88FF88");
        }
        else if (_enemy.CurrentIntent == EnemyIntent.Defend || result.DamageDealt > 0 && result.Message.Contains("heal"))
        {
            AddLog($"⚕️ {result.Message}", "#88FF88");
        }
        else
        {
            AddLog($"💢 {result.Message}", "#FF4444");
            _ = FlashHitOverlay();
            _ = ShakeElement(PlayerSprite);
            _ = ShowFloatingDamage(PlayerDmgPopup, result.DamageDealt);
        }

        RefreshUI();

        if (_player.IsDead)
        {
            HandleDefeat();
            return;
        }

        await Task.Delay(400);
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
        string realmLabel = string.IsNullOrEmpty(_realmName) ? "" : $" — {_realmName}";
        StageTxt.Text = $"Stage {_stage} / {_totalStages}{realmLabel}";

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
