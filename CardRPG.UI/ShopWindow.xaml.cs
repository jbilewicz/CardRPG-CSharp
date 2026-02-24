using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        GoldTxt.Text = _player.Gold.ToString();
        StrValueTxt.Text   = _player.Strength.ToString();
        AgiValueTxt.Text   = _player.Agility.ToString();
        IntValueTxt.Text   = _player.Intelligence.ToString();
        HpValueTxt.Text    = _player.MaxHp.ToString();
        ArmorValueTxt.Text = _player.Armor.ToString();
    }

    private bool TryBuy(int cost)
    {
        if (_player.Gold < cost)
        {
            ShowStatus($"Need {cost - _player.Gold} more gold!", error: true);
            return false;
        }
        _player.Gold -= cost;
        return true;
    }

    private async void BuyStrength_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuy(50)) return;
        _player.Strength++;
        RefreshUI();
        ShowStatus("Strength +1!");
        await AnimateStatChange(StrValueTxt, StrBonusTxt, "+1");
    }

    private async void BuyAgility_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuy(50)) return;
        _player.Agility++;
        RefreshUI();
        ShowStatus("Agility +1!");
        await AnimateStatChange(AgiValueTxt, AgiBonusTxt, "+1");
    }

    private async void BuyIntelligence_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuy(50)) return;
        _player.Intelligence++;
        RefreshUI();
        ShowStatus("Intelligence +1!");
        await AnimateStatChange(IntValueTxt, IntBonusTxt, "+1");
    }

    private async void BuyMaxHp_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuy(75)) return;
        _player.MaxHp += 10;
        _player.Heal(10);
        RefreshUI();
        ShowStatus("Max HP +10!");
        await AnimateStatChange(HpValueTxt, HpBonusTxt, "+10");
    }

    private async void BuyArmor_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuy(30)) return;
        _player.Armor += 5;
        RefreshUI();
        ShowStatus("Armor +5!");
        await AnimateStatChange(ArmorValueTxt, ArmorBonusTxt, "+5");
    }

    private void BuyItem_Click(object sender, RoutedEventArgs e)
    {
        if (ShopList.SelectedItem is not Item item)
        {
            ShowStatus("Select an item first.", error: true);
            return;
        }

        if (!TryBuy(item.Price)) return;

        if (item is Weapon w)
        {
            _player.EquippedWeapon = w;
            ShowStatus($"Equipped {w.Name}!");
        }
        else
        {
            _player.Inventory.Add(item);
            ShowStatus($"Bought {item.Name}!");
        }
        RefreshUI();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowStatus(string msg, bool error = false)
    {
        StatusTxt.Foreground = error ? Brushes.OrangeRed : Brushes.LimeGreen;
        StatusTxt.Text = msg;
    }

    // ── Stat change animation ──
    private async Task AnimateStatChange(TextBlock valueTxt, TextBlock bonusTxt, string bonus)
    {
        // Show bonus indicator with glow
        bonusTxt.Text = bonus;
        bonusTxt.Opacity = 1;

        // Flash the value text gold and enlarge
        var originalBrush = valueTxt.Foreground;
        var originalSize = valueTxt.FontSize;
        valueTxt.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
        valueTxt.FontSize = 22;

        await Task.Delay(400);

        valueTxt.FontSize = originalSize;
        valueTxt.Foreground = originalBrush;

        // Fade out bonus indicator
        for (int i = 10; i >= 0; i--)
        {
            bonusTxt.Opacity = i * 0.1;
            await Task.Delay(30);
        }
    }
}
