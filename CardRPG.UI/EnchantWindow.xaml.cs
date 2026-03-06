using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class EnchantWindow : Window
{
    private readonly Player _player;
    private const int EnchantCost = 50;

    public EnchantWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshUI();
    }

    private void RefreshUI()
    {
        GoldTxt.Text = _player.Gold.ToString();
        if (_player.EquippedWeapon != null)
        {
            WeaponTxt.Text = $"{_player.EquippedWeapon.Name} (+{_player.EquippedWeapon.DamageBonus} DMG)";
            EnchantTxt.Text = _player.EquippedWeapon.Enchantment != CardAbility.None
                ? _player.EquippedWeapon.Enchantment.ToString()
                : "None";
        }
        else
        {
            WeaponTxt.Text = "No weapon equipped!";
            EnchantTxt.Text = "---";
        }
    }

    private void EnchantBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_player.EquippedWeapon == null)
        {
            ShowStatus("Equip a weapon first!", true);
            return;
        }
        if (_player.Gold < EnchantCost)
        {
            ShowStatus($"Need {EnchantCost} gold!", true);
            return;
        }

        var btn = (Button)sender;
        var enchant = Enum.Parse<CardAbility>((string)btn.Tag);

        _player.Gold -= EnchantCost;
        _player.EquippedWeapon.Enchantment = enchant;
        RefreshUI();
        ShowStatus($"Weapon enchanted with {enchant}!");
    }

    private void RemoveEnchant_Click(object sender, RoutedEventArgs e)
    {
        if (_player.EquippedWeapon == null)
        {
            ShowStatus("No weapon equipped.", true);
            return;
        }
        if (_player.EquippedWeapon.Enchantment == CardAbility.None)
        {
            ShowStatus("No enchantment to remove.", true);
            return;
        }
        _player.EquippedWeapon.Enchantment = CardAbility.None;
        RefreshUI();
        ShowStatus("Enchantment removed.");
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    private void ShowStatus(string msg, bool isError = false)
    {
        StatusTxt.Foreground = isError ? Brushes.OrangeRed : Brushes.LightGreen;
        StatusTxt.Text = msg;
    }
}
