using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace CardRPG.UI;

public partial class VictoryWindow : Window
{
    public VictoryWindow()
    {
        InitializeComponent();
    }

    public static void ShowStageVictory(Window owner, int stage, int totalStages,
        int gold, int xp, int heal, bool leveledUp, int newLevel)
    {
        var win = new VictoryWindow { Owner = owner };
        win.TitleTxt.Text = "STAGE CLEARED";
        win.SubtitleTxt.Text = $"Stage {stage} / {totalStages}";

        win.AddRewardLine("💰", $"+{gold} Gold", "#FFD700");
        win.AddRewardLine("⭐", $"+{xp} XP", "#88CCFF");
        win.AddRewardLine("❤️", $"+{heal} HP restored", "#FF6666");

        if (leveledUp)
            win.AddExtraLine($"🎉 LEVEL UP! You are now Level {newLevel}!", "#44FF44");

        win.ShowDialog();
    }

    public static void ShowRealmVictory(Window owner, string realmName, int realmId,
        string diffLabel, int bossGold, int bossXp, int bossHeal,
        string? cardRewardName, string bossDropName, string bossDropRarity,
        bool leveledUp, int newLevel, string? newRealmName,
        int totalGold, int totalXp, int totalKills, List<string> allCards)
    {
        var win = new VictoryWindow { Owner = owner };
        win.TitleTxt.Text = $"{realmName.ToUpper()}";
        win.SubtitleTxt.Text = $"REALM CONQUERED!{diffLabel}";

        win.AddRewardLine("💰", $"+{bossGold} Gold", "#FFD700");
        win.AddRewardLine("⭐", $"+{bossXp} XP", "#88CCFF");
        win.AddRewardLine("❤️", $"+{bossHeal} HP restored", "#FF6666");

        if (cardRewardName != null)
            win.AddRewardLine("🃏", $"New Card: {cardRewardName}", "#FFCC44");

        win.AddRewardLine("👹", $"Boss Drop: {bossDropName} [{bossDropRarity}]", "#FF8844");

        if (leveledUp)
            win.AddExtraLine($"🎉 LEVEL UP! You are now Level {newLevel}!", "#44FF44");

        if (newRealmName != null)
            win.AddExtraLine($"🔓 New realm unlocked: {newRealmName}!", "#44CCFF");

        win.SummaryBorder.Visibility = Visibility.Visible;
        win.AddSummaryHeader("REALM SUMMARY");
        win.AddSummaryLine("Total Gold earned", $"{totalGold}g");
        win.AddSummaryLine("Total XP earned", $"{totalXp}");
        win.AddSummaryLine("Enemies defeated", $"{totalKills}");
        if (allCards.Count > 0)
        {
            win.AddSummaryLine("Cards obtained", $"{allCards.Count}");
            foreach (var card in allCards)
                win.AddSummaryCardLine(card);
        }

        win.ShowDialog();
    }

    private void AddRewardLine(string icon, string text, string colorHex)
    {
        var color = (Color)ColorConverter.ConvertFromString(colorHex);
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 3, 0, 3)
        };

        panel.Children.Add(new TextBlock
        {
            Text = icon + "  ",
            FontSize = 18,
            VerticalAlignment = VerticalAlignment.Center
        });

        panel.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 17,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.3
            }
        });

        RewardsPanel.Children.Add(panel);
    }

    private void AddExtraLine(string text, string colorHex)
    {
        var color = (Color)ColorConverter.ConvertFromString(colorHex);
        ExtraPanel.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 2),
            Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.5
            }
        });
    }

    private void AddSummaryHeader(string text)
    {
        SummaryPanel.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700")),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        });
    }

    private void AddSummaryLine(string label, string value)
    {
        var grid = new Grid { Margin = new Thickness(0, 1, 0, 1) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var labelTxt = new TextBlock
        {
            Text = label,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x88))
        };
        Grid.SetColumn(labelTxt, 0);
        grid.Children.Add(labelTxt);

        var valueTxt = new TextBlock
        {
            Text = value,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White
        };
        Grid.SetColumn(valueTxt, 1);
        grid.Children.Add(valueTxt);

        SummaryPanel.Children.Add(grid);
    }

    private void AddSummaryCardLine(string cardName)
    {
        SummaryPanel.Children.Add(new TextBlock
        {
            Text = $"  🃏 {cardName}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0x44)),
            Margin = new Thickness(8, 1, 0, 1)
        });
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
