using System.Windows;
using System.Windows.Media;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class EventWindow : Window
{
    private readonly Player _player;
    private readonly JourneyEvent _event;
    private readonly Random _rng = new();

    public Card? RewardCard { get; private set; }
    public bool GavePotion { get; private set; }

    public EventWindow(Player player, JourneyEvent journeyEvent)
    {
        InitializeComponent();
        _player = player;
        _event = journeyEvent;

        TitleTxt.Text = _event.Title;
        DescTxt.Text = _event.Description;
        ChoiceABtn.Content = _event.ChoiceA;
        ChoiceBBtn.Content = _event.ChoiceB;
    }

    private void ChoiceA_Click(object sender, RoutedEventArgs e)
    {
        var (message, gold, hp, card) = _event.ResolveChoiceA(_player, _rng);
        ApplyResult(message, gold, hp, card);
    }

    private void ChoiceB_Click(object sender, RoutedEventArgs e)
    {
        var (message, gold, hp, card) = _event.ResolveChoiceB(_player, _rng);

        // Special case: Fountain flask gives potion
        if (_event.Type == JourneyEventType.Fountain && message.Contains("Potion"))
        {
            _player.Inventory.Add(new Consumable("Small Potion", 30, 0));
            GavePotion = true;
        }

        ApplyResult(message, gold, hp, card);
    }

    private void ApplyResult(string message, int goldChange, int hpChange, Card? card)
    {
        _player.Gold += goldChange;
        if (_player.Gold < 0) _player.Gold = 0;

        if (hpChange > 0)
            _player.Heal(hpChange);
        else if (hpChange < 0)
            _player.TakeDamage(Math.Abs(hpChange));

        if (card != null)
        {
            _player.MasterDeck.Add(card);
            RewardCard = card;
        }

        ChoiceABtn.Visibility = Visibility.Collapsed;
        ChoiceBBtn.Visibility = Visibility.Collapsed;
        ResultPanel.Visibility = Visibility.Visible;

        string extra = "";
        if (goldChange != 0) extra += $" ({(goldChange > 0 ? "+" : "")}{goldChange} gold)";
        if (hpChange != 0) extra += $" ({(hpChange > 0 ? "+" : "")}{hpChange} HP)";
        if (card != null) extra += $" [New card: {card.Name}]";

        ResultTxt.Text = message + extra;
        ResultTxt.Foreground = hpChange < 0
            ? new SolidColorBrush(Color.FromRgb(0xFF, 0x66, 0x66))
            : Brushes.LimeGreen;
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
