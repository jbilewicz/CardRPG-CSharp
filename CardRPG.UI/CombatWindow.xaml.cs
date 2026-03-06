using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

        AudioManager.PlayBattleMusic();

        SetEnemySprite();
        UpdateStatusOverlays();
        StartIdleAnimations();
        StartNewRound();
    }

    private void StartIdleAnimations()
    {
        // Player idle breathing
        var playerBreath = new DoubleAnimation(1.0, 1.03, TimeSpan.FromMilliseconds(1200))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        var playerTransform = new ScaleTransform(1, 1);
        PlayerSprite.RenderTransform = playerTransform;
        PlayerSprite.RenderTransformOrigin = new Point(0.5, 1);
        playerTransform.BeginAnimation(ScaleTransform.ScaleYProperty, playerBreath);

        // Enemy idle hovering
        var enemyHover = new DoubleAnimation(0, -6, TimeSpan.FromMilliseconds(1500))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
        };
        var enemyTransform = new TranslateTransform(0, 0);
        EnemySprite.RenderTransform = enemyTransform;
        enemyTransform.BeginAnimation(TranslateTransform.YProperty, enemyHover);
    }

    private void SetEnemySprite()
    {
        string spritePath = _enemy.Name.ToLower() switch
        {
            // Realm 1 - Verdant Meadows
            var n when n.Contains("slime") => "Assets/slime.png",

            // Realm 2 - Dark Forest
            var n when n.Contains("wolf") => "Assets/wolf.png",
            var n when n.Contains("spider") => "Assets/spider.png",
            var n when n.Contains("treant") => "Assets/gryf.png",
            var n when n.Contains("bear") => "Assets/wolf.png",

            // Realm 3 - Goblin Caves
            var n when n.Contains("goblin") && n.Contains("shaman") => "Assets/necromant_goblin.png",
            var n when n.Contains("goblin") && n.Contains("chieftain") => "Assets/necromant_goblin.png",
            var n when n.Contains("goblin") => "Assets/goblin.png",

            // Realm 4 - Frozen Tundra
            var n when n.Contains("ice") || n.Contains("frost") => "Assets/golem.png",

            // Realm 5 - Volcanic Depths
            var n when n.Contains("magma") || n.Contains("golem") => "Assets/golem.png",
            var n when n.Contains("fire") || n.Contains("inferno") => "Assets/necromant_goblin.png",

            // Realm 6 - Shadow Marsh
            var n when n.Contains("lich") => "Assets/necromant_goblin.png",
            var n when n.Contains("skeleton") || n.Contains("wraith") => "Assets/skeleton_archer.png",

            // Realm 7 - Crystal Caverns
            var n when n.Contains("crystal") || n.Contains("diamond") => "Assets/golem.png",

            // Realm 8 - Dragon's Peak
            var n when n.Contains("wyvern") || n.Contains("drake") => "Assets/gryf.png",
            var n when n.Contains("dragon") => "Assets/arcwarlord.png",

            // Realm 9 - Demon Wasteland
            var n when n.Contains("imp") || n.Contains("succubus") => "Assets/necromant_goblin.png",
            var n when n.Contains("demon") => "Assets/arcwarlord.png",

            // Realm 10 - Abyss of Eternity
            var n when n.Contains("void") || n.Contains("abyss") => "Assets/arcwarlord.png",

            // Arena & bosses
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
        TurnIndicatorTxt.Text = "YOUR TURN";
        TurnIndicatorTxt.Foreground = Brushes.LimeGreen;
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
                Width = 160,
                Height = 210,
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
            AudioManager.PlayAttackSound();
            await AnimateCardPlay(btn, card);
            await SafeShake(EnemySprite);
            await SafeFloatingDmg(EnemyDmgPopup, result.DamageDealt);
        }
        else if (card.Type == CardType.Power)
        {
            AddLog($"PWR: {result.Message}", "#88FF88");
            await AnimateCardPlay(btn, card);
            if (result.Message != null && result.Message.Contains("HP"))
                await SafeHealPopup(result.DamageDealt);
        }
        else
        {
            AddLog($"DEF: {result.Message}", "#8888FF");
            await AnimateCardPlay(btn, card);
            await SafeArmorFlash();
        }

        await ShowCombo(result);
        UpdateStatusOverlays();

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

        TurnIndicatorTxt.Text = "ENEMY TURN";
        TurnIndicatorTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6644"));

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
                AudioManager.PlayAttackSound();
                await SafeFlash();
                await SafeShake(PlayerSprite);
                await SafeFloatingDmg(PlayerDmgPopup, result.DamageDealt);
            }
        }

        if (!string.IsNullOrEmpty(result.AbilityMessage))
            AddLog($"  >> {result.AbilityMessage}", "#FFCC44");

        UpdateStatusOverlays();
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
        AudioManager.StopBattleMusic();
        RefreshUI();
        DialogResult = true;
    }

    private void HandleDefeat()
    {
        if (_closing) return;
        _closing = true;
        AudioManager.StopBattleMusic();
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
            TranslateTransform transform;
            if (element.RenderTransform is TranslateTransform existing)
            {
                transform = existing;
            }
            else
            {
                transform = new TranslateTransform();
                element.RenderTransform = transform;
            }
            double baseX = transform.X;
            int[] offsets = [10, -10, 8, -8, 4, -4, 0];
            foreach (int offset in offsets)
            {
                if (_closing) { transform.X = baseX; return; }
                transform.X = baseX + offset;
                await Task.Delay(25);
            }
            transform.X = baseX;
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

    private async Task AnimateCardPlay(Button btn, Card card)
    {
        try
        {
            if (_closing) return;
            var transform = new ScaleTransform(1, 1);
            btn.RenderTransform = transform;
            btn.RenderTransformOrigin = new Point(0.5, 0.5);

            // Scale up briefly
            for (int i = 0; i < 5; i++)
            {
                if (_closing) return;
                double scale = 1.0 + (i * 0.04);
                transform.ScaleX = scale;
                transform.ScaleY = scale;
                await Task.Delay(20);
            }

            // Fade out and scale down
            for (int i = 0; i < 8; i++)
            {
                if (_closing) return;
                double scale = 1.2 - (i * 0.15);
                btn.Opacity = Math.Max(0, 1.0 - (i * 0.13));
                transform.ScaleX = Math.Max(0.1, scale);
                transform.ScaleY = Math.Max(0.1, scale);
                await Task.Delay(20);
            }
            btn.Visibility = Visibility.Collapsed;
        }
        catch { }
    }

    private async Task SafeHealPopup(int healAmount)
    {
        try
        {
            if (_closing || healAmount <= 0) return;
            PlayerHealPopup.Text = $"+{healAmount}";
            PlayerHealPopup.Opacity = 1;
            PlayerHealPopup.RenderTransform = new TranslateTransform(0, 0);

            // Green heal flash
            PlayerHealOverlay.Opacity = 0.3;
            await Task.Delay(120);
            if (_closing) { PlayerHealOverlay.Opacity = 0; PlayerHealPopup.Opacity = 0; return; }
            PlayerHealOverlay.Opacity = 0.15;
            await Task.Delay(120);
            PlayerHealOverlay.Opacity = 0;

            for (int i = 0; i < 12; i++)
            {
                if (_closing) { PlayerHealPopup.Opacity = 0; return; }
                PlayerHealPopup.Opacity = Math.Max(0, 1.0 - (i * 0.09));
                ((TranslateTransform)PlayerHealPopup.RenderTransform).Y = -(i * 3);
                await Task.Delay(30);
            }
            PlayerHealPopup.Opacity = 0;
        }
        catch { }
    }

    private async Task SafeArmorFlash()
    {
        try
        {
            if (_closing) return;
            PlayerArmorOverlay.Opacity = 0.3;
            await Task.Delay(100);
            if (_closing) { PlayerArmorOverlay.Opacity = 0; return; }
            PlayerArmorOverlay.Opacity = 0.15;
            await Task.Delay(100);
            PlayerArmorOverlay.Opacity = 0;
        }
        catch { }
    }

    private async Task ShowCombo(CombatResult result)
    {
        try
        {
            if (_closing || result.ComboCount < 2) return;
            AddLog($"  \u2B50 COMBO x{result.ComboCount}! +{result.ComboBonus} bonus!", "#FFD700");

            // Pulse the combo border
            ComboBorder.Opacity = 1;
            var transform = new ScaleTransform(1, 1);
            ComboBorder.RenderTransform = transform;
            ComboBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            for (int i = 0; i < 4; i++)
            {
                if (_closing) return;
                double s = 1.0 + (i < 2 ? i * 0.1 : (4 - i) * 0.1);
                transform.ScaleX = s;
                transform.ScaleY = s;
                await Task.Delay(50);
            }
            transform.ScaleX = 1;
            transform.ScaleY = 1;
        }
        catch { }
    }

    private void UpdateStatusOverlays()
    {
        try
        {
            // Update combo display
            if (_engine.ComboCount > 1)
            {
                ComboTxt.Text = $"COMBO x{_engine.ComboCount}! (+{(_engine.ComboCount - 1) * 2})";
                ComboBorder.Opacity = 1;
            }
            else
            {
                ComboBorder.Opacity = 0;
            }

            bool hasBurn = _engine.StatusEffects.Any(e => e.Name == "Burn" && e.AppliesToEnemy);
            bool hasPoison = _engine.StatusEffects.Any(e => e.Name == "Poison" && e.AppliesToEnemy);
            bool hasBleed = _engine.StatusEffects.Any(e => e.Name == "Bleed" && e.AppliesToEnemy);
            bool hasStun = _engine.EnemyStunned;

            EnemyBurnOverlay.Opacity = hasBurn ? 0.25 : 0;
            EnemyBurnOverlay.Background = new SolidColorBrush(Color.FromArgb(hasBurn ? (byte)0x40 : (byte)0, 0xFF, 0x44, 0x00));

            EnemyPoisonOverlay.Opacity = hasPoison ? 0.25 : 0;
            EnemyPoisonOverlay.Background = new SolidColorBrush(Color.FromArgb(hasPoison ? (byte)0x40 : (byte)0, 0x44, 0xFF, 0x00));

            EnemyBleedOverlay.Opacity = hasBleed ? 0.2 : 0;
            EnemyBleedOverlay.Background = new SolidColorBrush(Color.FromArgb(hasBleed ? (byte)0x40 : (byte)0, 0x88, 0x00, 0x00));

            EnemyStunOverlay.Opacity = hasStun ? 0.2 : 0;
            EnemyStunOverlay.Background = new SolidColorBrush(Color.FromArgb(hasStun ? (byte)0x30 : (byte)0, 0xFF, 0xFF, 0x00));

            // Status icons on enemy
            var icons = new List<string>();
            if (hasBurn) icons.Add("\U0001F525");
            if (hasPoison) icons.Add("\u2620");
            if (hasBleed) icons.Add("\U0001FA78");
            if (hasStun) icons.Add("\u26A1");
            EnemyStatusIcon.Text = string.Join(" ", icons);
            EnemyStatusIcon.Opacity = icons.Count > 0 ? 1 : 0;
        }
        catch { }
    }

    private void BattleMuteBtn_Click(object sender, RoutedEventArgs e)
    {
        AudioManager.BattleMuted = !AudioManager.BattleMuted;
        BattleMuteBtn.Content = AudioManager.BattleMuted ? "🔇" : "🔊";
    }
}
