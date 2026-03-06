using System.Windows;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class ClassSelectionWindow : Window
{
    public PlayerClass SelectedClass { get; private set; } = PlayerClass.Warrior;

    public ClassSelectionWindow()
    {
        InitializeComponent();
    }

    private void WarriorButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedClass = PlayerClass.Warrior;
        DialogResult = true;
    }

    private void RogueButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedClass = PlayerClass.Rogue;
        DialogResult = true;
    }

    private void MageButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedClass = PlayerClass.Mage;
        DialogResult = true;
    }
}
