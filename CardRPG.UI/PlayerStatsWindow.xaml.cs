using System.Windows;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class PlayerStatsWindow : Window
{
    public PlayerStatsWindow(Player player)
    {
        InitializeComponent();

        PlayerNameTxt.Text = player.Name;
        LevelTxt.Text = $"Level {player.Level}";
        XpTxt.Text = $"XP: {player.XP}/{player.XpForNextLevel}";

        HpVal.Text = $"{player.CurrentHp}/{player.MaxHp}";
        ManaVal.Text = $"{player.CurrentMana}/{player.MaxMana}";
        StrVal.Text = player.Strength.ToString();
        AgiVal.Text = player.Agility.ToString();
        IntVal.Text = player.Intelligence.ToString();
        ArmorVal.Text = player.Armor.ToString();
        WeaponVal.Text = player.EquippedWeapon != null
            ? $"{player.EquippedWeapon.Name} (+{player.EquippedWeapon.DamageBonus})"
            : "None";
        DeckVal.Text = $"{player.MasterDeck.Count} cards";
        GoldVal.Text = player.Gold.ToString();

        RealmTxt.Text = $"Realms Unlocked: {player.MaxRealmUnlocked}/10";
        TalentTxt.Text = $"Talent Points: {player.TalentPoints}";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
