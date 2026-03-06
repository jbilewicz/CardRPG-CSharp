using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class CraftWindow : Window
{
    private readonly Player _player;
    private string? _selectedCardName;
    private Card? _previewResult;

    public CraftWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshUI();
    }

    private void RefreshUI()
    {
        GoldTxt.Text = _player.Gold.ToString();
        RenderCardList();
    }

    private void RenderCardList()
    {
        CardList.Items.Clear();
        var grouped = _player.MasterDeck
            .Where(c => !c.IsPassive && c.Rarity <= CardRarity.Uncommon)
            .GroupBy(c => c.Name)
            .Where(g => g.Count() >= 2)
            .OrderBy(g => g.Key);

        foreach (var group in grouped)
        {
            var card = group.First();
            Color rarityColor = card.Rarity switch
            {
                CardRarity.Uncommon => Color.FromRgb(0x44, 0xFF, 0x44),
                _ => Color.FromRgb(0xCC, 0xCC, 0xCC),
            };

            string typeTag = card.Type switch
            {
                CardType.Attack => "ATK",
                CardType.Power => "PWR",
                _ => "DEF"
            };

            int cost = CardLibrary.GetCraftCost(card.Rarity);

            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(new TextBlock
            {
                Text = $"[{card.Rarity.ToString().ToUpper()}] ",
                Foreground = new SolidColorBrush(rarityColor),
                FontWeight = FontWeights.Bold, FontSize = 12
            });
            panel.Children.Add(new TextBlock
            {
                Text = $"{card.Name} x{group.Count()}",
                Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 12
            });
            panel.Children.Add(new TextBlock
            {
                Text = $"  {typeTag} {card.Value}  |  {cost}g",
                Foreground = Brushes.Gray, FontSize = 11, VerticalAlignment = VerticalAlignment.Center
            });

            var item = new ListBoxItem { Content = panel, Tag = card.Name };
            CardList.Items.Add(item);
        }

        if (CardList.Items.Count == 0)
        {
            CardList.Items.Add(new ListBoxItem
            {
                Content = new TextBlock { Text = "No craftable pairs. Need 2+ of same card (Common/Uncommon).",
                    Foreground = Brushes.Gray, FontStyle = FontStyles.Italic, TextWrapping = TextWrapping.Wrap },
                IsEnabled = false
            });
        }
    }

    private void CardList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CardList.SelectedItem is ListBoxItem item && item.Tag is string name)
        {
            _selectedCardName = name;
            var cards = _player.MasterDeck.Where(c => c.Name == name).ToList();
            if (cards.Count >= 2)
            {
                _previewResult = CardLibrary.TryCraft(cards[0], cards[1]);
                if (_previewResult != null)
                {
                    int cost = CardLibrary.GetCraftCost(cards[0].Rarity);
                    ResultNameTxt.Text = _previewResult.Name;
                    ResultNameTxt.Foreground = new SolidColorBrush(_previewResult.Rarity switch
                    {
                        CardRarity.Rare => Color.FromRgb(0x44, 0xAA, 0xFF),
                        CardRarity.Uncommon => Color.FromRgb(0x44, 0xFF, 0x44),
                        _ => Color.FromRgb(0xFF, 0xFF, 0xFF),
                    });

                    string typeLabel = _previewResult.Type switch
                    {
                        CardType.Attack => $"ATK {_previewResult.Value} DMG",
                        CardType.Defense => $"DEF {_previewResult.Value} ARM",
                        _ => $"PWR {_previewResult.Value}"
                    };
                    string abilityLabel = _previewResult.Ability != CardAbility.None
                        ? $"\nAbility: {_previewResult.Ability}" : "";
                    ResultDetailTxt.Text = $"{_previewResult.Rarity} | {typeLabel} | Cost: {_previewResult.Cost} mana{abilityLabel}";
                    CostTxt.Text = $"Forge cost: {cost} Gold";
                    CraftBtn.IsEnabled = _player.Gold >= cost;
                    return;
                }
            }
        }

        _selectedCardName = null;
        _previewResult = null;
        ResultNameTxt.Text = "---";
        ResultNameTxt.Foreground = Brushes.White;
        ResultDetailTxt.Text = "";
        CostTxt.Text = "";
        CraftBtn.IsEnabled = false;
    }

    private void CraftBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCardName == null || _previewResult == null) return;

        var cards = _player.MasterDeck.Where(c => c.Name == _selectedCardName).ToList();
        if (cards.Count < 2) return;

        int cost = CardLibrary.GetCraftCost(cards[0].Rarity);
        if (_player.Gold < cost) { ShowStatus("Not enough gold!", true); return; }

        _player.Gold -= cost;
        _player.MasterDeck.Remove(cards[0]);
        _player.MasterDeck.Remove(cards[1]);
        _player.MasterDeck.Add(_previewResult.Clone());
        _player.TotalCardsCollected++;

        ShowStatus($"Forged {_previewResult.Name}!");
        _selectedCardName = null;
        _previewResult = null;
        ResultNameTxt.Text = "---";
        ResultNameTxt.Foreground = Brushes.White;
        ResultDetailTxt.Text = "";
        CostTxt.Text = "";
        CraftBtn.IsEnabled = false;
        RefreshUI();
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowStatus(string msg, bool isError = false)
    {
        StatusTxt.Foreground = isError ? Brushes.OrangeRed : Brushes.LightGreen;
        StatusTxt.Text = msg;
    }
}
