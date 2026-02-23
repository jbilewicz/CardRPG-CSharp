using System.Windows;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class InventoryWindow : Window
{
    private readonly Player _player;

    public InventoryWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshUI();
    }

    private void RefreshUI()
    {
        WeaponTxt.Text = _player.EquippedWeapon?.Name ?? "None";
        HpTxt.Text = $"{_player.CurrentHp}/{_player.MaxHp}";
        InvList.ItemsSource = null;
        InvList.ItemsSource = _player.Inventory;
    }

    private void UseButton_Click(object sender, RoutedEventArgs e)
    {
        if (InvList.SelectedItem is not Item item)
        {
            StatusTxt.Foreground = System.Windows.Media.Brushes.Red;
            StatusTxt.Text = "Select an item first.";
            return;
        }

        if (item is Consumable potion)
        {
            _player.Heal(potion.HealAmount);
            _player.Inventory.Remove(potion);
            StatusTxt.Foreground = System.Windows.Media.Brushes.LightGreen;
            StatusTxt.Text = $"Used {potion.Name}! (+{potion.HealAmount} HP)";
        }
        else if (item is Weapon weapon)
        {
            _player.EquippedWeapon = weapon;
            StatusTxt.Foreground = System.Windows.Media.Brushes.LightBlue;
            StatusTxt.Text = $"Equipped {weapon.Name}!";
        }
        else
        {
            StatusTxt.Foreground = System.Windows.Media.Brushes.Gray;
            StatusTxt.Text = "Cannot use this item.";
        }

        RefreshUI();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
