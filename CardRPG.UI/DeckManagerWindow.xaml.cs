using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class DeckManagerWindow : Window
{
    private readonly Player _player;
    private Card? _selectedCard;

    public DeckManagerWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshAll();
    }

    private void RefreshAll()
    {
        DeckCountTxt.Text = $"{_player.MasterDeck.Count} cards";
        GoldTxt.Text = _player.Gold.ToString();
        RenderCards();
        UpdateSelectedInfo();
    }

    private void RenderCards()
    {
        CardPanel.Children.Clear();

        foreach (var card in _player.MasterDeck)
        {
            bool isAttack = card.Type == CardType.Attack;
            bool isPower = card.Type == CardType.Power;

            var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

            Color borderColor = card.Rarity switch
            {
                CardRarity.Legendary => Color.FromRgb(0xFF, 0xAA, 0x00),
                CardRarity.Rare => Color.FromRgb(0x44, 0xAA, 0xFF),
                CardRarity.Uncommon => Color.FromRgb(0x44, 0xFF, 0x44),
                _ => isAttack ? Color.FromRgb(0xFF, 0x44, 0x44)
                   : isPower ? Color.FromRgb(0x44, 0xFF, 0x44)
                   : Color.FromRgb(0x44, 0x88, 0xFF),
            };

            if (card.Rarity != CardRarity.Common)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = card.Rarity.ToString().ToUpper(),
                    FontSize = 8,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(borderColor),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }

            stack.Children.Add(new TextBlock
            {
                Text = card.DisplayName,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            });

            string typeLabel = isAttack ? "ATK" : isPower ? "PWR" : "DEF";
            stack.Children.Add(new TextBlock
            {
                Text = $"{typeLabel} {card.Value}",
                FontSize = 10,
                Foreground = isAttack ? Brushes.OrangeRed : isPower ? Brushes.LimeGreen : Brushes.CornflowerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold
            });

            if (card.Ability != CardAbility.None)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = card.Ability.ToString(),
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0x44)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold
                });
            }

            stack.Children.Add(new TextBlock
            {
                Text = $"Cost: {card.Cost}",
                FontSize = 9,
                Foreground = Brushes.MediumPurple,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            });

            Color bgStart = isAttack ? Color.FromRgb(0x3A, 0x10, 0x10)
                          : isPower ? Color.FromRgb(0x10, 0x3A, 0x10)
                          : Color.FromRgb(0x10, 0x18, 0x3A);
            Color bgEnd = isAttack ? Color.FromRgb(0x1C, 0x08, 0x08)
                        : isPower ? Color.FromRgb(0x08, 0x1C, 0x08)
                        : Color.FromRgb(0x08, 0x0C, 0x1C);

            bool isSelected = _selectedCard != null && card.Id == _selectedCard.Id;

            var btn = new Button
            {
                Content = stack,
                Tag = card,
                Width = 105,
                Height = 130,
                Margin = new Thickness(4),
                Background = new LinearGradientBrush(bgStart, bgEnd, 90),
                BorderBrush = new SolidColorBrush(isSelected ? Colors.Gold : borderColor),
                BorderThickness = new Thickness(isSelected ? 3 : 2),
                Foreground = Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            if (TryFindResource("CardButton") is Style cardStyle)
                btn.Style = cardStyle;

            if (isSelected || card.Rarity >= CardRarity.Rare)
            {
                btn.Effect = new DropShadowEffect
                {
                    Color = isSelected ? Colors.Gold : borderColor,
                    BlurRadius = isSelected ? 14 : 8,
                    ShadowDepth = 0,
                    Opacity = isSelected ? 0.7 : 0.4
                };
            }

            btn.Click += CardSelect_Click;
            CardPanel.Children.Add(btn);
        }
    }

    private void CardSelect_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Card card) return;
        _selectedCard = card;
        RefreshAll();
    }

    private void UpdateSelectedInfo()
    {
        if (_selectedCard == null)
        {
            SelNameTxt.Text = "Select a card...";
            SelTypeTxt.Text = "";
            SelDescTxt.Text = "";
            SelRarityTxt.Text = "";
            SelUpgradeTxt.Text = "";
            InfoTxt.Text = "";
            UpgradeBtn.IsEnabled = false;
            RemoveBtn.IsEnabled = false;
            return;
        }

        var c = _selectedCard;
        SelNameTxt.Text = c.DisplayName;
        SelTypeTxt.Text = $"{c.Type} | Mana: {c.Cost}";
        SelTypeTxt.Foreground = c.Type == CardType.Attack ? Brushes.OrangeRed
                              : c.Type == CardType.Power ? Brushes.LimeGreen
                              : Brushes.CornflowerBlue;
        SelDescTxt.Text = c.Description;

        Color rc = c.Rarity switch
        {
            CardRarity.Legendary => Color.FromRgb(0xFF, 0xAA, 0x00),
            CardRarity.Rare => Color.FromRgb(0x44, 0xAA, 0xFF),
            CardRarity.Uncommon => Color.FromRgb(0x44, 0xFF, 0x44),
            _ => Color.FromRgb(0xAA, 0xAA, 0xAA)
        };
        SelRarityTxt.Text = c.Rarity.ToString();
        SelRarityTxt.Foreground = new SolidColorBrush(rc);

        SelUpgradeTxt.Text = $"Level {c.UpgradeLevel} | Upgrade: {c.UpgradeCost}g | Remove: {c.RemoveCost}g";

        UpgradeBtn.Content = $"UPGRADE ({c.UpgradeCost}g)";
        UpgradeBtn.IsEnabled = _player.Gold >= c.UpgradeCost;

        // Evolution
        if (c.CanEvolve)
        {
            var evo = CardEvolution.GetEvolution(c.BaseName.Length > 0 ? c.BaseName : c.Name);
            EvolveBtn.Visibility = Visibility.Visible;
            EvolveBtn.Content = $"\u2728 EVOLVE → {evo?.EvolvedName} ({c.EvolveCost}g)";
            EvolveBtn.IsEnabled = _player.Gold >= c.EvolveCost;
        }
        else if (c.IsEvolved)
        {
            EvolveBtn.Visibility = Visibility.Visible;
            EvolveBtn.Content = "\u2728 EVOLVED";
            EvolveBtn.IsEnabled = false;
        }
        else
        {
            EvolveBtn.Visibility = Visibility.Collapsed;
        }

        RemoveBtn.Content = $"REMOVE ({c.RemoveCost}g)";
        RemoveBtn.IsEnabled = _player.Gold >= c.RemoveCost && _player.MasterDeck.Count > 5;

        InfoTxt.Text = _player.MasterDeck.Count <= 5
            ? "Minimum deck size: 5 cards"
            : "";
    }

    private void UpgradeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCard == null) return;

        if (_player.Gold < _selectedCard.UpgradeCost)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Not enough gold!";
            return;
        }

        _player.Gold -= _selectedCard.UpgradeCost;
        _selectedCard.Upgrade();

        StatusTxt.Foreground = Brushes.LimeGreen;
        StatusTxt.Text = $"Upgraded {_selectedCard.DisplayName}!";

        // Notify if evolution is now available
        if (_selectedCard.CanEvolve)
        {
            var evo = CardEvolution.GetEvolution(
                _selectedCard.BaseName.Length > 0 ? _selectedCard.BaseName : _selectedCard.Name);
            StatusTxt.Text += $" \u2728 EVOLUTION AVAILABLE → {evo?.EvolvedName}!";
        }

        RefreshAll();
    }

    private void EvolveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCard == null || !_selectedCard.CanEvolve) return;

        int cost = _selectedCard.EvolveCost;
        if (_player.Gold < cost)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Not enough gold!";
            return;
        }

        string oldName = _selectedCard.DisplayName;
        var evo = CardEvolution.GetEvolution(
            _selectedCard.BaseName.Length > 0 ? _selectedCard.BaseName : _selectedCard.Name);

        var result = MessageBox.Show(
            $"Evolve {oldName} into {evo?.EvolvedName}?\n\nCost: {cost} Gold\n" +
            $"Effect: {evo?.EvolvedDescription.Replace("{value}", (_selectedCard.Value + (evo?.BonusValue ?? 0)).ToString())}" +
            (evo?.HitCount > 1 ? $"\nHits: {evo.HitCount}x" : "") +
            (evo?.GrantAbility != null ? $"\nGains: {evo.GrantAbility}" : ""),
            "Card Evolution",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        _player.Gold -= cost;
        _selectedCard.Evolve();

        StatusTxt.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
        StatusTxt.Text = $"\u2728 {oldName} evolved into {_selectedCard.DisplayName}!";
        RefreshAll();
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCard == null) return;

        if (_player.MasterDeck.Count <= 5)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Minimum deck size is 5!";
            return;
        }

        if (_player.Gold < _selectedCard.RemoveCost)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Not enough gold!";
            return;
        }

        string name = _selectedCard.DisplayName;
        _player.Gold -= _selectedCard.RemoveCost;
        _player.MasterDeck.Remove(_selectedCard);
        _selectedCard = null;

        StatusTxt.Foreground = Brushes.Gray;
        StatusTxt.Text = $"Removed {name} from deck.";
        RefreshAll();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
