using System.Collections.Generic;
using System.Windows;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class ShopWindow : Window
{
    private readonly Player _player;

    private readonly List<Item> _stock = new()
    {
        new Weapon("Iron Sword",    3,  100),
        new Weapon("Steel Blade",   8,  160),
        new Weapon("Shadow Dagger", 14, 220),
        new Consumable("Small Potion", 30, 15),
        new Consumable("Big Potion",   50, 40),
        new Consumable("Elixir",      100, 80),
    };

    public ShopWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        ShopList.ItemsSource = _stock;
        RefreshUI();
    }

    private void RefreshUI()
    {
        GoldTxt.Text = $"Gold: {_player.Gold}";
        StatsTxt.Text = $"STR {_player.Strength}  AGI {_player.Agility}  INT {_player.Intelligence}  MaxHP {_player.MaxHp}";
    }

    private void TryBuy(int cost, System.Action onSuccess)
    {
        if (_player.Gold < cost)
        {
            Show($"Need {cost - _player.Gold} more gold.", error: true);
            return;
        }
        _player.Gold -= cost;
        onSuccess();
        RefreshUI();
    }

    private void BuyStrength_Click(object sender, RoutedEventArgs e)
        => TryBuy(50, () => { _player.Strength++; Show("Strength +1!"); });

    private void BuyAgility_Click(object sender, RoutedEventArgs e)
        => TryBuy(50, () => { _player.Agility++; Show("Agility +1!"); });

    private void BuyIntelligence_Click(object sender, RoutedEventArgs e)
        => TryBuy(50, () => { _player.Intelligence++; Show("Intelligence +1!"); });

    private void BuyMaxHp_Click(object sender, RoutedEventArgs e)
        => TryBuy(75, () => { _player.MaxHp += 10; _player.Heal(10); Show("Max HP +10!"); });

    private void BuyArmor_Click(object sender, RoutedEventArgs e)
        => TryBuy(30, () => { _player.Armor += 5; Show("Armor +5!"); });

    private void BuyItem_Click(object sender, RoutedEventArgs e)
    {
        if (ShopList.SelectedItem is not Item item)
        {
            Show("Select an item first.", error: true);
            return;
        }

        TryBuy(item.Price, () =>
        {
            if (item is Weapon w)
            {
                _player.EquippedWeapon = w;
                Show($"Equipped {w.Name}!");
            }
            else
            {
                _player.Inventory.Add(item);
                Show($"Bought {item.Name}!");
            }
        });
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void Show(string msg, bool error = false)
    {
        StatusTxt.Foreground = error
            ? System.Windows.Media.Brushes.OrangeRed
            : System.Windows.Media.Brushes.LightGreen;
        StatusTxt.Text = msg;
    }
}
