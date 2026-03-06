using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class CardRewardWindow : Window
{
    private readonly List<Card> _choices;
    public Card? SelectedCard { get; private set; }

    public CardRewardWindow(List<Card> choices)
    {
        InitializeComponent();
        _choices = choices;
        RenderCards();
    }

    private void RenderCards()
    {
        CardPanel.Children.Clear();

        foreach (var card in _choices)
        {
            bool isAttack = card.Type == CardType.Attack;
            bool isPower = card.Type == CardType.Power;

            var stack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Color rarityColor = card.Rarity switch
            {
                CardRarity.Uncommon => Color.FromRgb(0x44, 0xFF, 0x44),
                CardRarity.Rare => Color.FromRgb(0x44, 0xAA, 0xFF),
                CardRarity.Legendary => Color.FromRgb(0xFF, 0xAA, 0x00),
                _ => Color.FromRgb(0xAA, 0xAA, 0xAA),
            };

            // Rarity label
            stack.Children.Add(new TextBlock
            {
                Text = card.Rarity.ToString().ToUpper(),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(rarityColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            });

            // Type
            string typeLabel = isAttack ? "ATK" : isPower ? "PWR" : "DEF";
            stack.Children.Add(new TextBlock
            {
                Text = typeLabel,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = isAttack ? Brushes.OrangeRed : isPower ? Brushes.LimeGreen : Brushes.CornflowerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            });

            // Name
            stack.Children.Add(new TextBlock
            {
                Text = card.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });

            // Value
            string valueLabel = isAttack ? $"{card.Value} DMG"
                              : isPower ? card.Description
                              : $"{card.Value} ARM";
            stack.Children.Add(new TextBlock
            {
                Text = valueLabel,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 6, 0, 0)
            });

            // Ability
            if (card.Ability != CardAbility.None)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = card.Ability.ToString(),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0x44)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                });
            }

            // Mana cost
            stack.Children.Add(new TextBlock
            {
                Text = $"Mana: {card.Cost}",
                FontSize = 13,
                Foreground = Brushes.MediumPurple,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            });

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
                _ => Color.FromRgb(0xAA, 0xAA, 0xAA)
            };

            var btn = new Button
            {
                Content = stack,
                Tag = card,
                Width = 180,
                Height = 280,
                Margin = new Thickness(12),
                Background = new LinearGradientBrush(bgStart, bgEnd, 90),
                BorderBrush = new SolidColorBrush(borderColor),
                Foreground = Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            if (card.Rarity >= CardRarity.Rare)
            {
                btn.Effect = new DropShadowEffect
                {
                    Color = borderColor,
                    BlurRadius = 14,
                    ShadowDepth = 0,
                    Opacity = 0.5
                };
            }

            if (TryFindResource("CardButton") is Style cardStyle)
                btn.Style = cardStyle;

            btn.Click += CardChoice_Click;
            CardPanel.Children.Add(btn);
        }
    }

    private void CardChoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Card card) return;
        SelectedCard = card;
        DialogResult = true;
    }

    private void SkipButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedCard = null;
        DialogResult = false;
    }
}
