using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class FusionWindow : Window
{
    private readonly Player _player;
    private Card? _slot1;
    private Card? _slot2;
    private readonly Random _rng = new();

    public FusionWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshAll();
    }

    private int GetFusionCost(CardRarity rarity) => rarity switch
    {
        CardRarity.Common => 50,
        CardRarity.Uncommon => 100,
        CardRarity.Rare => 250,
        _ => 0
    };

    private CardRarity? GetNextRarity(CardRarity rarity) => rarity switch
    {
        CardRarity.Common => CardRarity.Uncommon,
        CardRarity.Uncommon => CardRarity.Rare,
        CardRarity.Rare => CardRarity.Legendary,
        _ => null
    };

    private void RefreshAll()
    {
        GoldTxt.Text = _player.Gold.ToString();
        Slot1Txt.Text = _slot1 != null ? _slot1.DisplayName : "(empty)";
        Slot2Txt.Text = _slot2 != null ? _slot2.DisplayName : "(empty)";

        bool canFuse = _slot1 != null && _slot2 != null
            && _slot1.Rarity == _slot2.Rarity
            && _slot1.Rarity != CardRarity.Legendary
            && !_slot1.IsCurse && !_slot2.IsCurse
            && _player.Gold >= GetFusionCost(_slot1.Rarity);

        FuseBtn.IsEnabled = canFuse;

        if (_slot1 != null && _slot1.Rarity != CardRarity.Legendary)
            CostTxt.Text = $"{GetFusionCost(_slot1.Rarity)}g";
        else
            CostTxt.Text = "-";

        RenderCards();
    }

    private void RenderCards()
    {
        CardPanel.Children.Clear();

        foreach (var card in _player.MasterDeck)
        {
            if (card.IsCurse) continue; // Can't fuse curse cards

            bool isAttack = card.Type == CardType.Attack;
            bool isPower = card.Type == CardType.Power;

            bool isSelected = (_slot1 != null && card.Id == _slot1.Id)
                           || (_slot2 != null && card.Id == _slot2.Id);

            // Gray out cards of different rarity if slot1 is filled
            bool dimmed = _slot1 != null && card.Rarity != _slot1.Rarity && !isSelected;
            bool isLegendary = card.Rarity == CardRarity.Legendary;

            var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

            Color borderColor = card.Rarity switch
            {
                CardRarity.Legendary => Color.FromRgb(0xFF, 0xAA, 0x00),
                CardRarity.Rare => Color.FromRgb(0x44, 0xAA, 0xFF),
                CardRarity.Uncommon => Color.FromRgb(0x44, 0xFF, 0x44),
                _ => Color.FromRgb(0xAA, 0xAA, 0xAA),
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
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = dimmed || isLegendary ? Brushes.Gray : Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            });

            string typeLabel = isAttack ? "ATK" : isPower ? "PWR" : "DEF";
            stack.Children.Add(new TextBlock
            {
                Text = $"{typeLabel} {card.Value}",
                FontSize = 9,
                Foreground = isAttack ? Brushes.OrangeRed : isPower ? Brushes.LimeGreen : Brushes.CornflowerBlue,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold
            });

            Color bgStart = isAttack ? Color.FromRgb(0x3A, 0x10, 0x10)
                          : isPower ? Color.FromRgb(0x10, 0x3A, 0x10)
                          : Color.FromRgb(0x10, 0x18, 0x3A);
            Color bgEnd = isAttack ? Color.FromRgb(0x1C, 0x08, 0x08)
                        : isPower ? Color.FromRgb(0x08, 0x1C, 0x08)
                        : Color.FromRgb(0x08, 0x0C, 0x1C);

            var btn = new Button
            {
                Content = stack,
                Tag = card,
                Width = 95,
                Height = 90,
                Margin = new Thickness(3),
                Background = new LinearGradientBrush(bgStart, bgEnd, 90),
                BorderBrush = new SolidColorBrush(isSelected ? Colors.Gold : borderColor),
                BorderThickness = new Thickness(isSelected ? 3 : 2),
                Foreground = Brushes.White,
                IsEnabled = !dimmed && !isLegendary,
                Cursor = System.Windows.Input.Cursors.Hand
            };

            if (TryFindResource("CardButton") is Style cardStyle)
                btn.Style = cardStyle;

            if (isSelected)
            {
                btn.Effect = new DropShadowEffect
                {
                    Color = Colors.Gold,
                    BlurRadius = 14,
                    ShadowDepth = 0,
                    Opacity = 0.7
                };
            }

            btn.Click += CardSelect_Click;
            CardPanel.Children.Add(btn);
        }
    }

    private void CardSelect_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Card card) return;

        // Deselect if already selected
        if (_slot1 != null && card.Id == _slot1.Id) { _slot1 = null; RefreshAll(); return; }
        if (_slot2 != null && card.Id == _slot2.Id) { _slot2 = null; RefreshAll(); return; }

        if (_slot1 == null)
            _slot1 = card;
        else if (_slot2 == null && card.Rarity == _slot1.Rarity)
            _slot2 = card;
        else
        {
            // Reset and start new selection
            _slot1 = card;
            _slot2 = null;
        }

        RefreshAll();
    }

    private void FuseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_slot1 == null || _slot2 == null) return;
        if (_slot1.Rarity == CardRarity.Legendary) return;

        var nextRarity = GetNextRarity(_slot1.Rarity);
        if (nextRarity == null) return;

        int cost = GetFusionCost(_slot1.Rarity);
        if (_player.Gold < cost) { StatusTxt.Text = "Not enough gold!"; return; }

        _player.Gold -= cost;
        _player.MasterDeck.Remove(_slot1);
        _player.MasterDeck.Remove(_slot2);

        var newCard = CardLibrary.GetRandomCardOfRarity(_rng, nextRarity.Value);
        _player.MasterDeck.Add(newCard);

        ResultTxt.Text = $"Fused into: {newCard.Name} [{newCard.Rarity}]!";
        StatusTxt.Text = $"Success! {_slot1.Name} + {_slot2.Name} → {newCard.Name}!";
        StatusTxt.Foreground = Brushes.LimeGreen;

        _slot1 = null;
        _slot2 = null;
        RefreshAll();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _slot1 = null;
        _slot2 = null;
        StatusTxt.Text = "";
        ResultTxt.Text = "";
        RefreshAll();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
