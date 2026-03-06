using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class EnchantWindow : Window
{
    private readonly Player _player;
    private Card? _selectedCard;

    // Enchant cost per rarity
    private static readonly Dictionary<CardRarity, int> _baseCost = new()
    {
        { CardRarity.Common, 40 },
        { CardRarity.Uncommon, 80 },
        { CardRarity.Rare, 150 },
        { CardRarity.Legendary, 300 }
    };

    // Ability costs (multiplier on base)
    private static readonly Dictionary<string, (CardAbility Ability, double CostMul, string Desc)> _enchants = new()
    {
        { "Lifesteal", (CardAbility.Lifesteal, 2.0, "Heal for 1/3 of damage dealt") },
        { "Poison",    (CardAbility.Poison,    1.5, "Apply poison for 3 turns") },
        { "Stun",      (CardAbility.Stun,      3.0, "Stun enemy for 1 turn") },
        { "Weaken",    (CardAbility.Weaken,     1.75, "Weaken enemy attacks") },
        { "Fortify",   (CardAbility.Fortify,    2.25, "Armor persists next turn") },
        { "Draw",      (CardAbility.Draw,       2.5, "Draw extra card(s)") }
    };

    public EnchantWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        GoldTxt.Text = _player.Gold.ToString();
        EnchantOptions.IsEnabled = false;
        PopulateCards();
    }

    private void PopulateCards()
    {
        CardPanel.Children.Clear();
        var enchantable = _player.MasterDeck
            .Where(c => c.Ability == CardAbility.None && !c.IsCurse)
            .OrderBy(c => c.Type)
            .ThenByDescending(c => c.Rarity)
            .ToList();

        if (enchantable.Count == 0)
        {
            CardPanel.Children.Add(new TextBlock
            {
                Text = "No enchantable cards in your deck.\nAll cards already have abilities or are curses.",
                Foreground = Brushes.Gray,
                FontSize = 14,
                Margin = new Thickness(12)
            });
            return;
        }

        foreach (var card in enchantable)
        {
            var btn = new Button
            {
                Width = 130,
                Height = 90,
                Margin = new Thickness(4),
                Style = (Style)FindResource("CardButton"),
                Tag = card
            };

            var rarityColor = card.Rarity switch
            {
                CardRarity.Legendary => "#FFD700",
                CardRarity.Rare => "#4488FF",
                CardRarity.Uncommon => "#44DD44",
                _ => "#BBBBBB"
            };

            var typeIcon = card.Type switch
            {
                CardType.Attack => "⚔",
                CardType.Defense => "🛡",
                CardType.Power => "✨",
                _ => "?"
            };

            var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            stack.Children.Add(new TextBlock
            {
                Text = $"{typeIcon} {card.DisplayName}",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(rarityColor)),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock
            {
                Text = $"[{card.Rarity}] {card.Type}",
                FontSize = 10,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center
            });
            stack.Children.Add(new TextBlock
            {
                Text = $"Value: {card.Value}  |  Cost: {_baseCost[card.Rarity]}g",
                FontSize = 9,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#888")),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 2, 0, 0)
            });

            btn.Content = stack;
            btn.Click += CardButton_Click;
            CardPanel.Children.Add(btn);
        }
    }

    private void CardButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Card card)
        {
            _selectedCard = card;
            var typeIcon = card.Type switch
            {
                CardType.Attack => "⚔",
                CardType.Defense => "🛡",
                CardType.Power => "✨",
                _ => ""
            };
            SelectedCardTxt.Text = $"{typeIcon} {card.DisplayName} [{card.Rarity}]";
            EnchantOptions.IsEnabled = true;
            StatusTxt.Text = "";
            UpdateEnchantButtons();
        }
    }

    private void UpdateEnchantButtons()
    {
        if (_selectedCard == null) return;

        int baseCost = _baseCost[_selectedCard.Rarity];

        // ATK cards: Lifesteal, Poison only
        // DEF cards: Fortify only
        // PWR cards: all enchants
        LifestealBtn.Visibility = (_selectedCard.Type == CardType.Attack || _selectedCard.Type == CardType.Power)
            ? Visibility.Visible : Visibility.Collapsed;
        PoisonBtn.Visibility = (_selectedCard.Type == CardType.Attack || _selectedCard.Type == CardType.Power)
            ? Visibility.Visible : Visibility.Collapsed;
        StunBtn.Visibility = (_selectedCard.Type == CardType.Power)
            ? Visibility.Visible : Visibility.Collapsed;
        WeakenBtn.Visibility = (_selectedCard.Type == CardType.Power)
            ? Visibility.Visible : Visibility.Collapsed;
        FortifyBtn.Visibility = (_selectedCard.Type == CardType.Defense || _selectedCard.Type == CardType.Power)
            ? Visibility.Visible : Visibility.Collapsed;
        DrawBtn.Visibility = (_selectedCard.Type == CardType.Power)
            ? Visibility.Visible : Visibility.Collapsed;

        // Update button text with actual costs
        foreach (var child in EnchantOptions.Children)
        {
            if (child is Button enchBtn && enchBtn.Tag is string tagName && _enchants.ContainsKey(tagName))
            {
                int cost = GetEnchantCost(tagName);
                string icon = tagName switch
                {
                    "Lifesteal" => "⚔️",
                    "Poison" => "☠️",
                    "Stun" => "⚡",
                    "Weaken" => "⬇️",
                    "Fortify" => "🛡",
                    "Draw" => "🔄",
                    _ => ""
                };
                enchBtn.Content = $"{icon} {tagName} ({cost}g)";
                enchBtn.IsEnabled = _player.Gold >= cost;
            }
        }
    }

    private int GetEnchantCost(string enchantName)
    {
        if (_selectedCard == null) return 999;
        int baseCostVal = _baseCost[_selectedCard.Rarity];
        double mul = _enchants[enchantName].CostMul;
        return (int)(baseCostVal * mul);
    }

    private void EnchantButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCard == null)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Select a card first!";
            return;
        }

        if (sender is Button btn && btn.Tag is string enchantName && _enchants.ContainsKey(enchantName))
        {
            int cost = GetEnchantCost(enchantName);
            if (_player.Gold < cost)
            {
                StatusTxt.Foreground = Brushes.OrangeRed;
                StatusTxt.Text = "Not enough gold!";
                return;
            }

            var ench = _enchants[enchantName];

            // Confirm
            var result = MessageBox.Show(
                $"Enchant {_selectedCard.DisplayName} with {enchantName}?\n\nCost: {cost} Gold\nEffect: {ench.Desc}",
                "Confirm Enchantment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            _player.Gold -= cost;
            _selectedCard.Imbue(ench.Ability);
            _player.Stats.CardsEnchanted++;

            GoldTxt.Text = _player.Gold.ToString();
            StatusTxt.Foreground = Brushes.LimeGreen;
            StatusTxt.Text = $"✨ {_selectedCard.DisplayName} enchanted with {enchantName}!";

            // Reset selection
            _selectedCard = null;
            SelectedCardTxt.Text = "Select a card...";
            EnchantOptions.IsEnabled = false;
            PopulateCards();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
