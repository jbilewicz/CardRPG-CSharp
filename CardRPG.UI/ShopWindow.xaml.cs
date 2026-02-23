using System.Collections.Generic;
using System.Windows;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class ShopWindow : Window
{
    private readonly Player _player;

    private readonly List<Item> _stock = new()
    {
        new Weapon("Iron Sword",    4,  30),
        new Weapon("Steel Blade",   8,  60),
        new Weapon("Shadow Dagger", 12, 100),
        new Consumable("Small Potion",  20, 10),
        new Consumable("Large Potion",  50, 25),
        new Consumable("Elixir",       100, 50),
    };

    public ShopWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshGold();
        ShopList.ItemsSource = _stock;
    }

    private void RefreshGold()
    {
        GoldTxt.Text = $"Gold: {_player.Gold}";
    }

    private void BuyButton_Click(object sender, RoutedEventArgs e)
    {
        if (ShopList.SelectedItem is not Item item)
        {
            StatusTxt.Foreground = System.Windows.Media.Brushes.Red;
            StatusTxt.Text = "Select an item first.";
            return;
        }

        if (_player.Gold < item.Price)
        {
            StatusTxt.Foreground = System.Windows.Media.Brushes.Red;
            StatusTxt.Text = "Not enough gold!";
            return;
        }

        _player.Gold -= item.Price;

        if (item is Weapon w)
        {
            _player.EquippedWeapon = w;
            StatusTxt.Foreground = System.Windows.Media.Brushes.LightGreen;
            StatusTxt.Text = $"Equipped {w.Name}!";
        }
        else
        {
            _player.Inventory.Add(item);
            StatusTxt.Foreground = System.Windows.Media.Brushes.LightGreen;
            StatusTxt.Text = $"Bought {item.Name}!";
        }

        RefreshGold();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
