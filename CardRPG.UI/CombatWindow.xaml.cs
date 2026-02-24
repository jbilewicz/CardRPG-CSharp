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
    private bool _actionInProgress = false;
    private bool _closing = false;

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
        StartNewRound();
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
            var n when n.Contains("arena") => "Assets/arcwarlord.png",
            _ => "Assets/slime.png"
        };

        try { EnemySprite.Source = new BitmapImage(new Uri(spritePath, UriKind.Relative)); }
        catch { }
    }

    private async void StartNewRound()
    {
        if (_closing) return;

        var statusMsgs = _engine.ProcessStatusEffects();
        foreach (var msg in statusMsgs)
            AddLog($"  * {msg}", "#FF9944");

        if (_enemy.IsDead)
        {
            AddLog($"{_enemy.Name} defeated by status effects!", "#FFD700");
            RefreshUI();
            await Task.Delay(600);
            if (!_closing) HandleVictory();
            return;
        }

        _player.CurrentMana = _player.MaxMana;
        if (!_engine.FortifyActive)
            _player.Armor = 0;
        _hand = _engine.DrawHand();
        _actionInProgress = false;
        EndTurnBtn.IsEnabled = true;
        RefreshUI();
        RenderHand();
        string realmLabel = string.IsNullOrEmpty(_realmName) ? "" : $" [{_realmName}]";
        AddLog($"-- New Round{realmLabel} | Mana: {_player.CurrentMana}/{_player.MaxMana} | Cards: {_hand.Count} --", "#777777");

        if (_hand.Count == 0)
        {
            AddLog("No cards available! Enemy attacks...", "#FFAA44");
            await Task.Delay(400);
            if (!_closing) await ExecuteEnemyAttack(true);
        }
    }

    private void EnableHand()
    {
        _actionInProgress = false;
        RefreshUI();
        RenderHand();
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

            Color rarityColor = card.Rarity switch
            {
                CardRarity.Uncommon => Color.FromRgb(0x44, 0xFF, 0x44),
                CardRarity.Rare => Color.FromRgb(0x44, 0xAA, 0xFF),
                CardRarity.Legendary => Color.FromRgb(0xFF, 0xAA, 0x00),
                _ => Color.FromRgb(0xAA, 0xAA, 0xAA),
            };

            if (card.Rarity != CardRarity.Common)
            {
                cardStack.Children.Add(new TextBlock
                {
                    Text = card.Rarity.ToString().ToUpper(),
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(rarityColor),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 2)
                });
            }

            string typeLabel = isAttack ? "ATK" : isPower ? "PWR" : "DEF";
            cardStack.Children.Add(new TextBlock
            {
                Text = typeLabel,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = isAttack ? Brushes.OrangeRed : isPower ? Brushes.LimeGreen : Brushes.CornflowerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 2)
            });

            cardStack.Children.Add(new TextBlock
            {
                Text = card.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
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
                if (card.Ability == CardAbility.ManaSurge)
                    valueLabel = $"+{card.Value} Mana";
                else if (card.Ability == CardAbility.Weaken)
                    valueLabel = "Weaken -3";
                else if (card.Ability == CardAbility.Draw)
                    valueLabel = "+1 Draw";
                else
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

            if (card.Ability != CardAbility.None)
            {
                cardStack.Children.Add(new TextBlock
                {
                    Text = card.Ability.ToString(),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0x44)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0)
                });
            }

            cardStack.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromArgb(0x44, 0xFF, 0xFF, 0xFF)),
                Margin = new Thickness(6, 6, 6, 4)
            });

            var costPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            costPanel.Children.Add(new TextBlock
            {
                Text = $"Mana: {card.Cost}",
                FontSize = 12,
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
            Color borderColor = card.Rarity switch
            {
                CardRarity.Legendary => Color.FromRgb(0xFF, 0xAA, 0x00),
                CardRarity.Rare => Color.FromRgb(0x44, 0xAA, 0xFF),
                CardRarity.Uncommon => Color.FromRgb(0x44, 0xFF, 0x44),
                _ => isAttack ? Color.FromRgb(0xFF, 0x44, 0x44)
                   : isPower ? Color.FromRgb(0x44, 0xFF, 0x44)
                   : Color.FromRgb(0x44, 0x88, 0xFF),
            };

            var btn = new Button
            {
                Content = cardStack,
                Tag = card,
                Width = 140,
                Height = 190,
                Margin = new Thickness(6),
                IsEnabled = canAfford && !_actionInProgress,
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
        if (_closing || _actionInProgress) return;

        var btn = (Button)sender;
        var card = (Card)btn.Tag;

        if (card.Cost > _player.CurrentMana)
        {
            AddLog("Not enough Mana!", "#FF4444");
            return;
        }

        _actionInProgress = true;
        SetHandEnabled(false);
        EndTurnBtn.IsEnabled = false;

        var result = _engine.PlayCard(card);
        _hand.Remove(card);

        if (card.Type == CardType.Attack)
        {
            string color = result.IsCrit ? "#FFD700" : "#FF8888";
            AddLog($"ATK: {result.Message}", color);
            await SafeShake(EnemySprite);
            await SafeFloatingDmg(EnemyDmgPopup, result.DamageDealt);
        }
        else if (card.Type == CardType.Power)
        {
            AddLog($"PWR: {result.Message}", "#88FF88");
        }
        else
        {
            AddLog($"DEF: {result.Message}", "#8888FF");
        }

        if (!string.IsNullOrEmpty(result.AbilityMessage))
            AddLog($"  >> {result.AbilityMessage}", "#FFCC44");

        RefreshUI();

        if (_closing) return;

        if (_enemy.IsDead)
        {
            AddLog($"{_enemy.Name} defeated!", "#FFD700");
            await Task.Delay(600);
            HandleVictory();
            return;
        }

        await ExecuteEnemyAttack(false);
    }

    private async void EndTurnButton_Click(object sender, RoutedEventArgs e)
    {
        if (_actionInProgress || _closing) return;
        AddLog("Skipping remaining cards... New hand!", "#AAAAAA");
        _actionInProgress = true;
        EndTurnBtn.IsEnabled = false;
        SetHandEnabled(false);
        await Task.Delay(300);
        if (!_closing) StartNewRound();
    }

    private async Task ExecuteEnemyAttack(bool startNewRoundAfter)
    {
        if (_closing) return;

        _actionInProgress = true;
        EndTurnBtn.IsEnabled = false;
        SetHandEnabled(false);

        AddLog("...", "#555555");
        await Task.Delay(500);
        if (_closing) return;

        var savedIntent = _enemy.CurrentIntent;
        var result = _engine.EnemyTurn();

        if (result.IsDodge)
        {
            AddLog($"DODGE: {result.Message}", "#88FF88");
        }
        else if (savedIntent == EnemyIntent.Defend)
        {
            AddLog($"HEAL: {result.Message}", "#88FF88");
        }
        else if (result.Message != null && result.Message.Contains("stunned"))
        {
            AddLog($"STUN: {result.Message}", "#FFCC44");
        }
        else
        {
            AddLog($"HIT: {result.Message ?? "Enemy attacks!"}", "#FF4444");
            if (!_closing)
            {
                await SafeFlash();
                await SafeShake(PlayerSprite);
                await SafeFloatingDmg(PlayerDmgPopup, result.DamageDealt);
            }
        }

        if (!string.IsNullOrEmpty(result.AbilityMessage))
            AddLog($"  >> {result.AbilityMessage}", "#FFCC44");

        RefreshUI();

        if (_closing) return;

        if (_player.IsDead)
        {
            HandleDefeat();
            return;
        }

        await Task.Delay(250);
        if (_closing) return;

        if (startNewRoundAfter || _hand.Count == 0 || !_engine.HasAffordableCard(_hand))
        {
            StartNewRound();
        }
        else
        {
            EnableHand();
        }
    }

    private void HandleVictory()
    {
        if (_closing) return;
        _closing = true;
        RefreshUI();
        DialogResult = true;
    }

    private void HandleDefeat()
    {
        if (_closing) return;
        _closing = true;
        _player.CurrentHp = 1;
        _player.Gold = Math.Max(0, _player.Gold - 10);
        RefreshUI();
        MessageBox.Show("You were defeated and dragged back to town.\n-10 Gold.", "Defeat");
        DialogResult = false;
    }

    private void RefreshUI()
    {
        try
        {
            string realmLabel = string.IsNullOrEmpty(_realmName) ? "" : $" -- {_realmName}";
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
            PlayerManaTxt.Text = $"{_player.CurrentMana}/{_player.MaxMana}";
            PlayerArmorTxt.Text = $"{_player.Armor}";

            StatusEffectsTxt.Text = _engine.GetStatusSummary();
        }
        catch { }
    }

    private void SetHandEnabled(bool enabled)
    {
        foreach (UIElement child in HandPanel.Children)
            child.IsEnabled = enabled;
    }

    private void AddLog(string message, string color = "#AAAAAA")
    {
        try
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
        catch { }
    }

    private async Task SafeShake(FrameworkElement element)
    {
        try
        {
            if (_closing) return;
            if (element.RenderTransform is not TranslateTransform transform)
            {
                transform = new TranslateTransform();
                element.RenderTransform = transform;
            }
            int[] offsets = [10, -10, 8, -8, 4, -4, 0];
            foreach (int offset in offsets)
            {
                if (_closing) { transform.X = 0; return; }
                transform.X = offset;
                await Task.Delay(25);
            }
        }
        catch { }
    }

    private async Task SafeFlash()
    {
        try
        {
            if (_closing) return;
            HitFlash.Opacity = 0.4;
            await Task.Delay(60);
            if (_closing) { HitFlash.Opacity = 0; return; }
            HitFlash.Opacity = 0.2;
            await Task.Delay(60);
            HitFlash.Opacity = 0;
        }
        catch { }
    }

    private async Task SafeFloatingDmg(TextBlock popup, int damage)
    {
        try
        {
            if (_closing || damage <= 0) return;
            popup.Text = $"-{damage}";
            popup.Opacity = 1;
            popup.RenderTransform = new TranslateTransform(0, 0);

            for (int i = 0; i < 15; i++)
            {
                if (_closing) { popup.Opacity = 0; return; }
                popup.Opacity = Math.Max(0, 1.0 - (i * 0.07));
                ((TranslateTransform)popup.RenderTransform).Y = -(i * 3);
                await Task.Delay(25);
            }
            popup.Opacity = 0;
        }
        catch { }
    }
}
