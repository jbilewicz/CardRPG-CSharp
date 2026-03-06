using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class TavernWindow : Window
{
    private readonly Player _player;

    public TavernWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshUI();
    }

    private void RefreshUI()
    {
        HpTxt.Text = $"HP: {_player.CurrentHp} / {_player.GetEffectiveMaxHp()}";
        GoldTxt.Text = $"Gold: {_player.Gold}";
        RenderQuests();
    }

    private void RenderQuests()
    {
        QuestList.Children.Clear();
        var allQuests = Quest.GetAllQuests();

        foreach (var quest in allQuests)
        {
            bool isActive = _player.ActiveQuestIds.Contains(quest.Id);
            bool isCompleted = _player.CompletedQuestIds.Contains(quest.Id);
            bool tooLow = _player.Level < quest.MinLevel;
            bool questDone = isActive && quest.IsComplete(_player);

            Color borderColor;
            if (isCompleted) borderColor = Color.FromRgb(0x44, 0x66, 0x44);
            else if (questDone) borderColor = Color.FromRgb(0xFF, 0xD7, 0x00);
            else if (isActive) borderColor = Color.FromRgb(0x44, 0xAA, 0xFF);
            else if (tooLow) borderColor = Color.FromRgb(0x44, 0x44, 0x44);
            else borderColor = Color.FromRgb(0x88, 0x77, 0x55);

            var stack = new StackPanel { Margin = new Thickness(0, 0, 0, 2) };

            // Title row
            var titleRow = new DockPanel();
            var nameTxt = new TextBlock
            {
                Text = quest.Name,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = isCompleted ? Brushes.Gray : Brushes.White
            };
            titleRow.Children.Add(nameTxt);

            if (isCompleted)
            {
                var doneTxt = new TextBlock
                {
                    Text = "DONE",
                    FontSize = 10,
                    Foreground = Brushes.Green,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                DockPanel.SetDock(doneTxt, Dock.Right);
                titleRow.Children.Insert(0, doneTxt);
            }
            stack.Children.Add(titleRow);

            // Description
            stack.Children.Add(new TextBlock
            {
                Text = quest.Description,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x88)),
                TextWrapping = TextWrapping.Wrap
            });

            // Rewards
            string rewardTxt = $"Reward: {quest.RewardGold}g + {quest.RewardXp} XP";
            if (quest.RewardCard != null)
                rewardTxt += $" + [{quest.RewardCard.Name}]";
            stack.Children.Add(new TextBlock
            {
                Text = rewardTxt,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xCC, 0x44)),
                Margin = new Thickness(0, 2, 0, 0)
            });

            // Progress bar for active quests
            if (isActive && !isCompleted)
            {
                int progress = quest.GetProgress(_player);
                var progressRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };

                var progressBg = new Border
                {
                    Width = 120,
                    Height = 10,
                    CornerRadius = new CornerRadius(4),
                    Background = new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
                    BorderThickness = new Thickness(1)
                };
                double pct = Math.Min(1.0, (double)progress / quest.TargetCount);
                var progressFill = new Border
                {
                    Width = 118 * pct,
                    Height = 8,
                    CornerRadius = new CornerRadius(3),
                    Background = questDone ? Brushes.Gold : new SolidColorBrush(Color.FromRgb(0x44, 0xAA, 0xFF)),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                progressBg.Child = progressFill;
                progressRow.Children.Add(progressBg);
                progressRow.Children.Add(new TextBlock
                {
                    Text = $"  {progress}/{quest.TargetCount}",
                    Foreground = Brushes.White,
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center
                });

                if (questDone)
                {
                    var claimBtn = new Button
                    {
                        Content = "CLAIM",
                        Width = 90,
                        Height = 34,
                        FontSize = 13,
                        Style = (Style)FindResource("RpgGreenButton"),
                        Margin = new Thickness(8, 0, 0, 0),
                        Tag = quest.Id
                    };
                    claimBtn.Click += ClaimQuest_Click;
                    progressRow.Children.Add(claimBtn);
                }

                stack.Children.Add(progressRow);
            }

            // Accept button for available quests
            if (!isActive && !isCompleted)
            {
                if (tooLow)
                {
                    stack.Children.Add(new TextBlock
                    {
                        Text = $"Requires Level {quest.MinLevel}",
                        FontSize = 10,
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 4, 0, 0)
                    });
                }
                else
                {
                    var acceptBtn = new Button
                    {
                        Content = "ACCEPT",
                        Width = 100,
                        Height = 36,
                        FontSize = 13,
                        Style = (Style)FindResource("RpgTealButton"),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 4, 0, 0),
                        Tag = quest.Id
                    };
                    acceptBtn.Click += AcceptQuest_Click;
                    stack.Children.Add(acceptBtn);
                }
            }

            var border = new Border
            {
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Background = new SolidColorBrush(Color.FromArgb(0x44, 0x11, 0x11, 0x11)),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 0, 0, 6)
            };

            if (questDone)
            {
                border.Effect = new DropShadowEffect
                {
                    Color = Colors.Gold,
                    BlurRadius = 10,
                    ShadowDepth = 0,
                    Opacity = 0.4
                };
            }

            border.Child = stack;
            QuestList.Children.Add(border);
        }
    }

    private void AcceptQuest_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string questId)
        {
            if (!_player.ActiveQuestIds.Contains(questId))
            {
                _player.ActiveQuestIds.Add(questId);
                if (!_player.QuestProgress.ContainsKey(questId))
                    _player.QuestProgress[questId] = 0;
                ShowStatus("Quest accepted!");
                RefreshUI();
            }
        }
    }

    private void ClaimQuest_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string questId)
        {
            var quest = Quest.GetAllQuests().FirstOrDefault(q => q.Id == questId);
            if (quest == null) return;

            _player.Gold += quest.RewardGold;
            _player.TotalGoldEarned += quest.RewardGold;
            bool leveledUp = _player.GainXp(quest.RewardXp);

            string msg = $"Quest Complete: {quest.Name}!\n+{quest.RewardGold} Gold\n+{quest.RewardXp} XP";

            if (quest.RewardCard != null)
            {
                _player.MasterDeck.Add(quest.RewardCard.Clone());
                _player.TotalCardsCollected++;
                msg += $"\nNew card: {quest.RewardCard.Name}!";
            }

            if (leveledUp)
                msg += $"\n\nLEVEL UP! You are now Level {_player.Level}!";

            _player.ActiveQuestIds.Remove(questId);
            _player.CompletedQuestIds.Add(questId);

            ShowStatus(msg.Replace("\n", " | "));
            RefreshUI();
        }
    }

    private void MealButton_Click(object sender, RoutedEventArgs e)
    {
        const int cost = 10;
        const int heal = 30;
        if (_player.Gold < cost)
        {
            ShowStatus($"Need {cost}g.", isError: true);
            return;
        }
        _player.Gold -= cost;
        _player.Heal(heal);
        RefreshUI();
        ShowStatus($"+{heal} HP restored!");
    }

    private void RestButton_Click(object sender, RoutedEventArgs e)
    {
        const int cost = 30;
        if (_player.Gold < cost)
        {
            ShowStatus($"Need {cost}g.", isError: true);
            return;
        }
        int healed = _player.GetEffectiveMaxHp() - _player.CurrentHp;
        _player.Gold -= cost;
        _player.Heal(_player.GetEffectiveMaxHp());
        RefreshUI();
        ShowStatus($"Fully rested! (+{healed} HP)");
    }

    private void LeaveButton_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowStatus(string msg, bool isError = false)
    {
        StatusTxt.Foreground = isError
            ? System.Windows.Media.Brushes.OrangeRed
            : System.Windows.Media.Brushes.LightGreen;
        StatusTxt.Text = msg;
    }
}
