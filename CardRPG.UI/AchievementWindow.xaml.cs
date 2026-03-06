using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class AchievementWindow : Window
{
    private readonly Player _player;

    public AchievementWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RenderAchievements();
    }

    private void RenderAchievements()
    {
        AchievementList.Children.Clear();
        var all = Achievement.GetAll();
        int completed = 0;
        int claimed = 0;

        foreach (var ach in all)
        {
            bool done = ach.CheckCompletion(_player);
            bool alreadyClaimed = _player.CompletedAchievements.Contains(ach.Id);
            if (done) completed++;
            if (alreadyClaimed) claimed++;

            Color borderColor;
            string statusLabel;
            if (alreadyClaimed)
            {
                borderColor = Color.FromRgb(0x44, 0x88, 0x44);
                statusLabel = "CLAIMED";
            }
            else if (done)
            {
                borderColor = Color.FromRgb(0xFF, 0xD7, 0x00);
                statusLabel = "READY!";
            }
            else
            {
                borderColor = Color.FromRgb(0x33, 0x33, 0x33);
                statusLabel = "";
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftPanel = new StackPanel();
            leftPanel.Children.Add(new TextBlock
            {
                Text = ach.Name,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = alreadyClaimed ? Brushes.Gray
                           : done ? new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00))
                           : Brushes.White
            });
            leftPanel.Children.Add(new TextBlock
            {
                Text = ach.Description,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                Margin = new Thickness(0, 2, 0, 0)
            });
            leftPanel.Children.Add(new TextBlock
            {
                Text = $"Reward: {ach.RewardGold}g + {ach.RewardXp} XP",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(0xBB, 0xAA, 0x66)),
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(leftPanel, 0);
            grid.Children.Add(leftPanel);

            var rightPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            if (done && !alreadyClaimed)
            {
                var claimBtn = new Button
                {
                    Content = "CLAIM",
                    Width = 110,
                    Height = 40,
                    Tag = ach.Id,
                    FontSize = 14
                };
                if (TryFindResource("RpgGreenButton") is Style greenStyle)
                    claimBtn.Style = greenStyle;
                claimBtn.Click += ClaimBtn_Click;
                rightPanel.Children.Add(claimBtn);
            }
            else if (alreadyClaimed)
            {
                rightPanel.Children.Add(new TextBlock
                {
                    Text = statusLabel,
                    Foreground = Brushes.DarkGreen,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                });
            }
            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(rightPanel);

            var border = new Border
            {
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 3, 0, 3),
                Background = new SolidColorBrush(Color.FromArgb(0x44, 0x11, 0x11, 0x11)),
                Child = grid
            };

            if (done && !alreadyClaimed)
            {
                border.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(0xFF, 0xD7, 0x00),
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.3
                };
            }

            AchievementList.Children.Add(border);
        }

        ProgressTxt.Text = $"{claimed} / {all.Count} claimed  ({completed} completed)";
    }

    private void ClaimBtn_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        string id = (string)btn.Tag;
        var ach = Achievement.GetAll().FirstOrDefault(a => a.Id == id);
        if (ach == null) return;

        _player.CompletedAchievements.Add(ach.Id);
        _player.Gold += ach.RewardGold;
        _player.TotalGoldEarned += ach.RewardGold;
        _player.GainXp(ach.RewardXp);

        StatusTxt.Text = $"Claimed {ach.Name}! +{ach.RewardGold}g +{ach.RewardXp} XP";
        RenderAchievements();
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
}
