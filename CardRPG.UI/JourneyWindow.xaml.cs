using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class JourneyWindow : Window
{
    private readonly Player _player;
    public int? SelectedRealmId { get; private set; }

    public JourneyWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RenderRealms();
        UpdateHeader();
    }

    private void UpdateHeader()
    {
        LevelTxt.Text = _player.Level.ToString();
        XpTxt.Text = $"{_player.XP}/{_player.XpForNextLevel}";
        UnlockedTxt.Text = $"{_player.MaxRealmUnlocked}/10";
    }

    private void RenderRealms()
    {
        RealmPanel.Children.Clear();
        var allRealms = Realm.GetAllRealms();

        foreach (var realm in allRealms)
        {
            bool unlocked = realm.Id <= _player.MaxRealmUnlocked;
            bool levelOk = _player.Level >= realm.RequiredLevel;
            bool canEnter = unlocked && levelOk;

            var card = new Border
            {
                Width = 260,
                Margin = new Thickness(8),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14),
                BorderThickness = new Thickness(2),
                Background = canEnter
                    ? new LinearGradientBrush(
                        Color.FromArgb(0xDD, 0x1A, 0x1A, 0x2E),
                        Color.FromArgb(0xDD, 0x0D, 0x0D, 0x14), 90)
                    : new SolidColorBrush(Color.FromArgb(0xAA, 0x0A, 0x0A, 0x0A)),
                BorderBrush = canEnter
                    ? new LinearGradientBrush(
                        Color.FromRgb(0xB8, 0x86, 0x0B),
                        Color.FromRgb(0xFF, 0xD7, 0x00), 45)
                    : new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)),
                Opacity = canEnter ? 1.0 : 0.55,
            };

            if (canEnter)
            {
                card.Effect = new DropShadowEffect
                {
                    Color = Colors.Gold,
                    BlurRadius = 8,
                    ShadowDepth = 0,
                    Opacity = 0.15
                };
            }

            var stack = new StackPanel();

            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };
            headerPanel.Children.Add(new TextBlock
            {
                Text = unlocked ? realm.Icon : "[?]",
                FontSize = 28,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            });

            var titleStack = new StackPanel();
            titleStack.Children.Add(new TextBlock
            {
                Text = unlocked ? realm.Name : "???",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = canEnter
                    ? new LinearGradientBrush(
                        Color.FromRgb(0xFF, 0xD7, 0x00),
                        Color.FromRgb(0xFF, 0xA5, 0x00), 0)
                    : Brushes.Gray
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = $"Realm {realm.Id}  •  Lv.{realm.RequiredLevel}+",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x7A))
            });
            headerPanel.Children.Add(titleStack);
            stack.Children.Add(headerPanel);

            stack.Children.Add(new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xD7, 0x00)),
                Margin = new Thickness(0, 2, 0, 6)
            });

            if (unlocked)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = realm.Description,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99)),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 6)
                });

                stack.Children.Add(new TextBlock
                {
                    Text = $"{realm.Stages.Count} stages  |  {realm.GoldReward}g  |  {realm.XpReward} XP",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x7A)),
                    Margin = new Thickness(0, 0, 0, 4)
                });

                if (realm.CardReward != null)
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = $"Reward: {realm.CardReward.Name} [{realm.CardReward.Rarity}]",
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0x44)),
                        Margin = new Thickness(0, 0, 0, 8)
                    });
                }
            }
            else
            {
                stack.Children.Add(new TextBlock
                {
                    Text = "Complete the previous realm to unlock.",
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 8)
                });
            }

            if (canEnter)
            {
                var enterBtn = new Button
                {
                    Content = "ENTER REALM",
                    Tag = realm.Id,
                    Height = 38,
                    FontSize = 14,
                    Margin = new Thickness(0, 4, 0, 0),
                };

                if (TryFindResource("RpgRedButton") is Style redStyle)
                    enterBtn.Style = redStyle;

                enterBtn.Click += RealmButton_Click;
                stack.Children.Add(enterBtn);
            }
            else if (unlocked && !levelOk)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = $"Requires Level {realm.RequiredLevel}",
                    FontSize = 12,
                    Foreground = Brushes.OrangeRed,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            card.Child = stack;
            RealmPanel.Children.Add(card);
        }
    }

    private void RealmButton_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        SelectedRealmId = (int)btn.Tag;
        DialogResult = true;
        Close();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
