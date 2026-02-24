using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class InventoryWindow : Window
{
    private readonly Player _player;

    public InventoryWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshUI();
    }

    private void RefreshUI()
    {
        WeaponTxt.Text = _player.EquippedWeapon != null
            ? $"{_player.EquippedWeapon.Name} (+{_player.EquippedWeapon.DamageBonus} DMG)"
            : "None";
        HpTxt.Text = $"{_player.CurrentHp}/{_player.MaxHp}";
        DeckCountTxt.Text = $"{_player.MasterDeck.Count} cards";

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

        if (_player.MasterDeck.Count == 0)
        {
            CardPanel.Children.Add(new TextBlock
            {
                Text = "No cards in your deck.",
                Foreground = Brushes.Gray,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(10)
            });
            return;
        }

        foreach (var card in _player.MasterDeck)
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

            stack.Children.Add(new TextBlock
            {
                Text = card.Name,
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

            var border = new Border
            {
                Width = 100,
                Height = 120,
                Margin = new Thickness(4),
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(borderColor),
                Background = new LinearGradientBrush(bgStart, bgEnd, 90),
                Padding = new Thickness(4),
                Child = stack
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

            CardPanel.Children.Add(border);
        }
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
        }
        else if (item is Weapon weapon)
        {
            _player.EquippedWeapon = weapon;
            StatusTxt.Foreground = Brushes.LightBlue;
            StatusTxt.Text = $"Equipped {weapon.Name}!";
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
}
