using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class InventoryWindow : Window
{
    private readonly Player _player;
    private Card? _selectedCard;
    private Border? _selectedCardBorder;
    private CardType? _filterType = null;
    private string _sortBy = "none";

    public InventoryWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshUI();
    }

    private void RefreshUI()
    {
        var wpn = _player.EquippedWeapon;
        WeaponTxt.Text = wpn != null
            ? $"{wpn.GetDisplayName()} (+{wpn.DamageBonus} DMG)"
            : "None";
        HpTxt.Text = $"{_player.CurrentHp}/{_player.GetEffectiveMaxHp()}";
        DeckCountTxt.Text = $"{_player.MasterDeck.Count} cards";
        _selectedCard = null;
        _selectedCardBorder = null;
        SellInfoTxt.Text = "Click a card to select it";

        RenderItems();
        RenderCards();
    }

    private void RenderItems()
    {
        ItemList.Items.Clear();
        foreach (var item in _player.Inventory)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };

            Color typeColor = item.Type switch
            {
                ItemType.Weapon => Color.FromRgb(0xFF, 0x88, 0x44),
                ItemType.Consumable => Color.FromRgb(0x44, 0xFF, 0x44),
                _ => Color.FromRgb(0xAA, 0xAA, 0xAA),
            };

            string prefix = item.Type switch
            {
                ItemType.Weapon => "[W] ",
                ItemType.Consumable => "[C] ",
                _ => "",
            };

            panel.Children.Add(new TextBlock
            {
                Text = prefix,
                Foreground = new SolidColorBrush(typeColor),
                FontWeight = FontWeights.Bold,
                FontSize = 13
            });

            panel.Children.Add(new TextBlock
            {
                Text = item.Name,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 13
            });

            panel.Children.Add(new TextBlock
            {
                Text = $"  {item.Description}",
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            });

            var listItem = new ListBoxItem
            {
                Content = panel,
                Tag = item,
                Foreground = Brushes.White
            };
            ItemList.Items.Add(listItem);
        }

        if (_player.Inventory.Count == 0)
        {
            ItemList.Items.Add(new ListBoxItem
            {
                Content = new TextBlock
                {
                    Text = "No items yet.",
                    Foreground = Brushes.Gray,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(4)
                },
                IsEnabled = false
            });
        }
    }

    private void RenderCards()
    {
        CardPanel.Children.Clear();

        var cards = GetFilteredSortedCards().ToList();

        if (cards.Count == 0)
        {
            string msg = _filterType.HasValue
                ? $"No {_filterType.Value} cards in your deck."
                : "No cards in your deck.";
            CardPanel.Children.Add(new TextBlock
            {
                Text = msg,
                Foreground = Brushes.Gray,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(10)
            });
            return;
        }

        foreach (var card in cards)
        {
            bool isAttack = card.Type == CardType.Attack;
            bool isPower = card.Type == CardType.Power;

            Color borderColor = card.Rarity switch
            {
                CardRarity.Legendary => Color.FromRgb(0xFF, 0xAA, 0x00),
                CardRarity.Rare => Color.FromRgb(0x44, 0xAA, 0xFF),
                CardRarity.Uncommon => Color.FromRgb(0x44, 0xFF, 0x44),
                _ => isAttack ? Color.FromRgb(0xFF, 0x44, 0x44)
                   : isPower ? Color.FromRgb(0x44, 0xFF, 0x44)
                   : Color.FromRgb(0x44, 0x88, 0xFF),
            };

            Color bgStart = isAttack ? Color.FromRgb(0x3A, 0x10, 0x10)
                          : isPower ? Color.FromRgb(0x10, 0x3A, 0x10)
                          : Color.FromRgb(0x10, 0x18, 0x3A);
            Color bgEnd = isAttack ? Color.FromRgb(0x1C, 0x08, 0x08)
                        : isPower ? Color.FromRgb(0x08, 0x1C, 0x08)
                        : Color.FromRgb(0x08, 0x0C, 0x1C);

            var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

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

            if (card.IsPassive)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = "PASSIVE",
                    FontSize = 8,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0x88, 0xFF)),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }

            stack.Children.Add(new TextBlock
            {
                Text = card.Name,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            });

            if (card.IsPassive)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = card.Description ?? $"+{card.PassiveValue} {card.PassiveEffect}",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0x88, 0xFF)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center
                });
            }
            else
            {
                string typeLabel = isAttack ? "ATK" : isPower ? "PWR" : "DEF";
                stack.Children.Add(new TextBlock
                {
                    Text = $"{typeLabel} {card.Value}",
                    FontSize = 10,
                    Foreground = isAttack ? Brushes.OrangeRed : isPower ? Brushes.LimeGreen : Brushes.CornflowerBlue,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold
                });
            }

            if (card.Ability != CardAbility.None && !card.IsPassive)
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

            if (!card.IsPassive)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = $"Cost: {card.Cost}",
                    FontSize = 9,
                    Foreground = Brushes.MediumPurple,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 0)
                });
            }

            // Sell price label
            stack.Children.Add(new TextBlock
            {
                Text = $"Sell: {card.GetSellPrice()}g",
                FontSize = 8,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x66)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 0)
            });

            if (card.UpgradeLevel > 0 && !card.IsPassive)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = $"\u2B50 Lv.{card.UpgradeLevel}",
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 1, 0, 0)
                });
            }

            var border = new Border
            {
                Width = 130,
                Height = 170,
                Margin = new Thickness(5),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(borderColor),
                Background = new LinearGradientBrush(bgStart, bgEnd, 90),
                Padding = new Thickness(4),
                Child = stack,
                Tag = card,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            if (card.Rarity >= CardRarity.Rare)
            {
                border.Effect = new DropShadowEffect
                {
                    Color = borderColor,
                    BlurRadius = 8,
                    ShadowDepth = 0,
                    Opacity = 0.4
                };
            }

            border.MouseLeftButtonDown += CardBorder_Click;
            border.MouseEnter += CardBorder_MouseEnter;
            border.MouseLeave += CardBorder_MouseLeave;
            CardPanel.Children.Add(border);
        }
    }

    private IEnumerable<Card> GetFilteredSortedCards()
    {
        IEnumerable<Card> cards = _player.MasterDeck;

        if (_filterType.HasValue)
            cards = cards.Where(c => c.Type == _filterType.Value);

        cards = _sortBy switch
        {
            "type" => cards.OrderBy(c => c.Type).ThenByDescending(c => c.Rarity),
            "rarity" => cards.OrderByDescending(c => c.Rarity).ThenBy(c => c.Type),
            "cost" => cards.OrderByDescending(c => c.Cost).ThenByDescending(c => c.Rarity),
            _ => cards
        };

        return cards;
    }

    private void CardBorder_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            var transform = new ScaleTransform(1, 1);
            border.RenderTransform = transform;
            border.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleXAnim = new DoubleAnimation(1.0, 1.08, TimeSpan.FromMilliseconds(150))
            { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
            var scaleYAnim = new DoubleAnimation(1.0, 1.08, TimeSpan.FromMilliseconds(150))
            { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);

            border.Effect = new DropShadowEffect
            {
                Color = ((SolidColorBrush)border.BorderBrush).Color,
                BlurRadius = 14,
                ShadowDepth = 0,
                Opacity = 0.6
            };
            Panel.SetZIndex(border, 10);
        }
    }

    private void CardBorder_MouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Border border)
        {
            var transform = border.RenderTransform as ScaleTransform ?? new ScaleTransform(1.08, 1.08);
            border.RenderTransform = transform;
            border.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleXAnim = new DoubleAnimation(1.08, 1.0, TimeSpan.FromMilliseconds(150))
            { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };
            var scaleYAnim = new DoubleAnimation(1.08, 1.0, TimeSpan.FromMilliseconds(150))
            { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn } };

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);

            if (border.Tag is Card card && card.Rarity >= CardRarity.Rare)
            {
                border.Effect = new DropShadowEffect
                {
                    Color = ((SolidColorBrush)border.BorderBrush).Color,
                    BlurRadius = 8,
                    ShadowDepth = 0,
                    Opacity = 0.4
                };
            }
            else
            {
                border.Effect = null;
            }
            Panel.SetZIndex(border, 0);
        }
    }

    private void SortByType_Click(object sender, RoutedEventArgs e)
    {
        _sortBy = "type";
        FlashStatus("Sorted by Type", "#88CCFF");
        RefreshUI();
    }

    private void SortByRarity_Click(object sender, RoutedEventArgs e)
    {
        _sortBy = "rarity";
        FlashStatus("Sorted by Rarity", "#FFCC44");
        RefreshUI();
    }

    private void SortByCost_Click(object sender, RoutedEventArgs e)
    {
        _sortBy = "cost";
        FlashStatus("Sorted by Cost", "#CC88FF");
        RefreshUI();
    }

    private void FilterAll_Click(object sender, RoutedEventArgs e)
    {
        _filterType = null;
        FlashStatus("Showing all cards", "#AAAAAA");
        RefreshUI();
    }

    private void FilterAttack_Click(object sender, RoutedEventArgs e)
    {
        _filterType = CardType.Attack;
        FlashStatus("Showing Attack cards", "#FF8888");
        RefreshUI();
    }

    private void FilterDefense_Click(object sender, RoutedEventArgs e)
    {
        _filterType = CardType.Defense;
        FlashStatus("Showing Defense cards", "#8888FF");
        RefreshUI();
    }

    private void FilterPower_Click(object sender, RoutedEventArgs e)
    {
        _filterType = CardType.Power;
        FlashStatus("Showing Power cards", "#88FF88");
        RefreshUI();
    }

    private void FlashStatus(string message, string color)
    {
        StatusTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        StatusTxt.Text = message;
    }

    private void CardBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is Card card)
        {
            // Deselect previous
            if (_selectedCardBorder != null)
                _selectedCardBorder.BorderThickness = new Thickness(2);

            _selectedCard = card;
            _selectedCardBorder = border;
            border.BorderThickness = new Thickness(3);

            string upgradeInfo = card.CanUpgrade && !card.IsPassive
                ? $" | Upgrade: {card.GetUpgradeCost()}g (Lv.{card.UpgradeLevel}/{card.MaxUpgradeLevel})"
                : card.IsPassive ? "" : " | MAX LEVEL";
            SellInfoTxt.Text = $"Selected: {card.Name} (Sell: {card.GetSellPrice()}g{upgradeInfo})";
            SellInfoTxt.Foreground = Brushes.Gold;
        }
    }

    private void SellCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCard == null)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Select a card to sell first.";
            return;
        }

        // Minimum deck size check
        int nonPassiveCount = _player.MasterDeck.Count(c => !c.IsPassive);
        if (!_selectedCard.IsPassive && nonPassiveCount <= 4)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Cannot sell! Need at least 4 playable cards.";
            return;
        }

        int price = _selectedCard.GetSellPrice();
        string name = _selectedCard.Name;
        _player.MasterDeck.Remove(_selectedCard);
        _player.Gold += price;
        _player.TotalGoldEarned += price;

        StatusTxt.Foreground = Brushes.LightGreen;
        StatusTxt.Text = $"Sold {name} for {price}g!";
        AnimateStatusFlash();
        RefreshUI();
    }

    private void UseButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemList.SelectedItem is not ListBoxItem listItem || listItem.Tag is not Item item)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Select an item first.";
            return;
        }

        if (item is Consumable potion)
        {
            _player.Heal(potion.HealAmount);
            _player.Inventory.Remove(potion);
            StatusTxt.Foreground = Brushes.LightGreen;
            StatusTxt.Text = $"Used {potion.Name}! (+{potion.HealAmount} HP)";
            AnimateStatusFlash();
        }
        else if (item is Weapon weapon)
        {
            if (_player.EquippedWeapon != null)
                _player.Inventory.Add(_player.EquippedWeapon);
            _player.EquippedWeapon = weapon;
            _player.Inventory.Remove(weapon);
            StatusTxt.Foreground = Brushes.LightBlue;
            StatusTxt.Text = $"Equipped {weapon.Name}!";
            AnimateStatusFlash();
        }
        else
        {
            StatusTxt.Foreground = Brushes.Gray;
            StatusTxt.Text = "Cannot use this item.";
        }

        RefreshUI();
    }

    private void DropButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemList.SelectedItem is not ListBoxItem listItem || listItem.Tag is not Item item)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Select an item to drop.";
            return;
        }

        _player.Inventory.Remove(item);
        StatusTxt.Foreground = Brushes.Gray;
        StatusTxt.Text = $"Dropped {item.Name}.";
        RefreshUI();
    }

    private void ItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void UpgradeCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCard == null)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Select a card to upgrade first.";
            return;
        }

        if (_selectedCard.IsPassive)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Passive cards cannot be upgraded.";
            return;
        }

        if (!_selectedCard.CanUpgrade)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = $"{_selectedCard.Name} is already at max upgrade level!";
            return;
        }

        int cost = _selectedCard.GetUpgradeCost();
        if (_player.Gold < cost)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = $"Not enough gold! Need {cost}g to upgrade.";
            return;
        }

        _player.Gold -= cost;
        string oldName = _selectedCard.Name;
        _selectedCard.Upgrade();

        StatusTxt.Foreground = Brushes.LimeGreen;
        StatusTxt.Text = $"Upgraded {oldName} \u2192 {_selectedCard.Name} (Value: {_selectedCard.Value})!";
        AnimateStatusFlash();
        RefreshUI();
    }

    private void AnimateStatusFlash()
    {
        var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        StatusTxt.BeginAnimation(OpacityProperty, anim);
    }
}
