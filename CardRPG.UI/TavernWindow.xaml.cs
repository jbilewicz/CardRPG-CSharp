using System.Windows;
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
        HpTxt.Text = $"HP: {_player.CurrentHp} / {_player.MaxHp}";
        GoldTxt.Text = $"Gold: {_player.Gold}";
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
        int healed = _player.MaxHp - _player.CurrentHp;
        _player.Gold -= cost;
        _player.Heal(_player.MaxHp);
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
