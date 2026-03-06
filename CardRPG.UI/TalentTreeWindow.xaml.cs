using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class TalentTreeWindow : Window
{
    private readonly Player _player;

    public TalentTreeWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RenderAll();
    }

    private void RenderAll()
    {
        PointsTxt.Text = _player.TalentPoints.ToString();
        RenderTree(WarriorPanel, TalentTree.Warrior);
        RenderTree(RoguePanel, TalentTree.Rogue);
        RenderTree(MagePanel, TalentTree.Mage);
    }

    private void RenderTree(StackPanel panel, TalentTree treeName)
    {
        panel.Children.Clear();

        var nodes = _player.Talents
            .Where(t => t.Tree == treeName)
            .OrderBy(t => t.Tier)
            .ToList();

        Color treeColor = treeName switch
        {
            TalentTree.Warrior => Color.FromRgb(0xFF, 0x66, 0x44),
            TalentTree.Rogue => Color.FromRgb(0x44, 0xFF, 0x44),
            TalentTree.Mage => Color.FromRgb(0x66, 0x99, 0xFF),
            _ => Colors.White
        };

        foreach (var node in nodes)
        {
            bool isMaxed = node.CurrentRanks >= node.MaxRanks;
            bool canUnlock = !isMaxed && _player.TalentPoints > 0 && CanUnlockTier(treeName, node.Tier);

            var nodePanel = new StackPanel
            {
                Margin = new Thickness(0, 4, 0, 4),
                Orientation = Orientation.Horizontal
            };

            // Tier indicator
            var tierTxt = new TextBlock
            {
                Text = $"T{node.Tier}",
                Foreground = new SolidColorBrush(Color.FromArgb(0x88, treeColor.R, treeColor.G, treeColor.B)),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 28
            };
            nodePanel.Children.Add(tierTxt);

            // Talent button
            var btn = new Button
            {
                Width = 220,
                Height = 60,
                Margin = new Thickness(0, 0, 6, 0),
                Tag = node,
                IsEnabled = canUnlock,
                Cursor = canUnlock ? System.Windows.Input.Cursors.Hand : System.Windows.Input.Cursors.Arrow
            };

            // Content stack
            var contentStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(6, 4, 6, 4) };

            var nameTxt = new TextBlock
            {
                Text = node.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Foreground = isMaxed ? new SolidColorBrush(treeColor) : Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };
            contentStack.Children.Add(nameTxt);

            var descTxt = new TextBlock
            {
                Text = node.Description,
                FontSize = 10,
                Foreground = Brushes.Gray,
                TextWrapping = TextWrapping.Wrap
            };
            contentStack.Children.Add(descTxt);

            var rankTxt = new TextBlock
            {
                Text = $"{node.CurrentRanks}/{node.MaxRanks}",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = isMaxed ? Brushes.Gold : Brushes.White,
                Margin = new Thickness(0, 2, 0, 0)
            };
            contentStack.Children.Add(rankTxt);

            Color bgStart, bgEnd, borderCol;
            if (isMaxed)
            {
                bgStart = Color.FromArgb(0x44, treeColor.R, treeColor.G, treeColor.B);
                bgEnd = Color.FromArgb(0x22, treeColor.R, treeColor.G, treeColor.B);
                borderCol = treeColor;
            }
            else if (canUnlock)
            {
                bgStart = Color.FromRgb(0x2A, 0x2A, 0x1A);
                bgEnd = Color.FromRgb(0x18, 0x18, 0x0E);
                borderCol = Color.FromRgb(0xB8, 0x86, 0x0B);
            }
            else
            {
                bgStart = Color.FromRgb(0x1A, 0x1A, 0x1A);
                bgEnd = Color.FromRgb(0x10, 0x10, 0x10);
                borderCol = Color.FromRgb(0x44, 0x44, 0x44);
            }

            btn.Content = contentStack;
            btn.Background = new LinearGradientBrush(bgStart, bgEnd, 90);
            btn.BorderBrush = new SolidColorBrush(borderCol);

            if (TryFindResource("CardButton") is Style cardStyle)
                btn.Style = cardStyle;

            if (isMaxed)
            {
                btn.Effect = new DropShadowEffect
                {
                    Color = treeColor,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.5
                };
            }

            btn.Click += TalentButton_Click;
            nodePanel.Children.Add(btn);

            panel.Children.Add(nodePanel);
        }
    }

    private bool CanUnlockTier(TalentTree tree, int tier)
    {
        if (tier == 1) return true;
        // Must have at least 1 rank in previous tier
        return _player.Talents.Any(t => t.Tree == tree && t.Tier == tier - 1 && t.CurrentRanks > 0);
    }

    private void TalentButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not TalentNode node) return;

        if (_player.TalentPoints <= 0)
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "No talent points available!";
            return;
        }

        if (node.CurrentRanks >= node.MaxRanks)
        {
            StatusTxt.Foreground = Brushes.Gray;
            StatusTxt.Text = "Already maxed!";
            return;
        }

        if (!CanUnlockTier(node.Tree, node.Tier))
        {
            StatusTxt.Foreground = Brushes.OrangeRed;
            StatusTxt.Text = "Unlock previous tier talent first!";
            return;
        }

        node.CurrentRanks++;
        _player.TalentPoints--;

        StatusTxt.Foreground = Brushes.LimeGreen;
        StatusTxt.Text = $"Unlocked: {node.Name}! ({node.CurrentRanks}/{node.MaxRanks})";
        RenderAll();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
