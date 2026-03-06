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
        new Weapon("Iron Sword",        3,   100),
        new Weapon("Steel Blade",       8,   160),
        new Weapon("Shadow Dagger",    14,   220),
        new Weapon("Flame Axe",        20,   320),
        new Weapon("Frost Claymore",   28,   450),
        new Weapon("Void Reaper",      40,   700),
        new Weapon("Thunder Mace",     50,   950),
        new Weapon("Obsidian Katana",  60,  1200),
        new Weapon("Dragon Fang",      75,  1600),
        new Weapon("Celestial Blade", 100,  2500),
        new Weapon("Soul Harvester",  130,  3500),
        new Weapon("Godslayer",       170,  5000),

        new Consumable("Small Potion",      30,    15),
        new Consumable("Big Potion",        50,    40),
        new Consumable("Elixir",           100,    80),
        new Consumable("Grand Elixir",     200,   150),
        new Consumable("Phoenix Tear",     500,   350),
        new Consumable("Healing Herb",      15,     8),
        new Consumable("Antidote",          40,    25),
        new Consumable("Dragon Blood",     300,   220),
        new Consumable("Divine Nectar",    750,   500),
        new Consumable("Elixir of Life",  1000,   800),
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
        AudioManager.PlayBuySound();
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
            _player.Inventory.Add(new Weapon(w.Name, w.DamageBonus, 0));
            ShowStatus($"Bought {w.Name}! ({_player.Inventory.Count} items in bag)");
        }
        else
        {
            _player.Inventory.Add(item is Consumable c
                ? new Consumable(c.Name, c.HealAmount, 0)
                : new Item(item.Name, item.Description, 0, item.Type));
            ShowStatus($"Bought {item.Name}! ({_player.Inventory.Count} items in bag)");
        }
        RefreshUI();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowStatus(string msg, bool error = false)
    {
        StatusTxt.Foreground = error ? Brushes.OrangeRed : Brushes.LimeGreen;
        StatusTxt.Text = msg;
    }

    private async Task AnimateStatChange(TextBlock valueTxt, TextBlock bonusTxt, string bonus)
    {
        bonusTxt.Text = bonus;
        bonusTxt.Opacity = 1;

        var originalBrush = valueTxt.Foreground;
        var originalSize = valueTxt.FontSize;
        valueTxt.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
        valueTxt.FontSize = 22;

        await Task.Delay(400);

        valueTxt.FontSize = originalSize;
        valueTxt.Foreground = originalBrush;

        for (int i = 10; i >= 0; i--)
        {
            bonusTxt.Opacity = i * 0.1;
            await Task.Delay(30);
        }
    }
}
