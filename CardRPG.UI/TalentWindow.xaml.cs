using System.Windows;
using System.Windows.Media;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class TalentWindow : Window
{
    private readonly Player _player;

    public TalentWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshUI();
    }

    private void RefreshUI()
    {
        PointsTxt.Text = _player.TalentPoints.ToString();
        StrTxt.Text = _player.Strength.ToString();
        AgiTxt.Text = _player.Agility.ToString();
        IntTxt.Text = _player.Intelligence.ToString();
    }

    private void AddStr_Click(object sender, RoutedEventArgs e)
    {
        if (_player.TalentPoints <= 0) { ShowStatus("No talent points!", true); return; }
        _player.TalentPoints--;
        _player.Strength++;
        RefreshUI();
        ShowStatus($"Strength increased to {_player.Strength}!");
    }

    private void AddAgi_Click(object sender, RoutedEventArgs e)
    {
        if (_player.TalentPoints <= 0) { ShowStatus("No talent points!", true); return; }
        _player.TalentPoints--;
        _player.Agility++;
        RefreshUI();
        ShowStatus($"Agility increased to {_player.Agility}!");
    }

    private void AddInt_Click(object sender, RoutedEventArgs e)
    {
        if (_player.TalentPoints <= 0) { ShowStatus("No talent points!", true); return; }
        _player.TalentPoints--;
        _player.Intelligence++;
        RefreshUI();
        ShowStatus($"Intelligence increased to {_player.Intelligence}!");
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowStatus(string msg, bool isError = false)
    {
        StatusTxt.Foreground = isError ? Brushes.OrangeRed : Brushes.LightGreen;
        StatusTxt.Text = msg;
    }
}
