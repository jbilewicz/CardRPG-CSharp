using System.Windows;
using CardRPG.Core.Models;

namespace CardRPG.UI;

public partial class ArenaWindow : Window
{
    private readonly Player _player;
    private int _bestWave = 0;

    public ArenaWindow(Player player)
    {
        InitializeComponent();
        _player = player;
        RefreshUI();
    }

    private void RefreshUI()
    {
        HpTxt.Text = $"{_player.CurrentHp}/{_player.MaxHp}";
        GoldTxt.Text = _player.Gold.ToString();
        BestWaveTxt.Text = _bestWave.ToString();
    }

    private void StartArena_Click(object sender, RoutedEventArgs e)
    {
        var rng = new Random();
        int wave = 0;

        while (true)
        {
            wave++;
            int enemyHp = 25 + (wave * 15) + (wave * wave * 2);
            int enemyDmg = 3 + (wave * 2);
            bool isBoss = wave % 5 == 0;

            string enemyName = isBoss
                ? $"Arena Champion Lv.{wave}"
                : $"Arena Fighter Lv.{wave}";

            if (isBoss)
            {
                enemyHp = (int)(enemyHp * 1.8);
                enemyDmg = (int)(enemyDmg * 1.4);
            }

            var enemy = new Enemy(enemyName, enemyHp, enemyDmg, isBoss);
            var combat = new CombatWindow(_player, enemy, wave, 999, "Arena") { Owner = this };
            bool? result = combat.ShowDialog();

            if (result != true)
            {
                if (wave > _bestWave) _bestWave = wave - 1;
                StatusTxt.Foreground = System.Windows.Media.Brushes.OrangeRed;
                StatusTxt.Text = $"Defeated at wave {wave}! Best: {_bestWave}";
                RefreshUI();
                return;
            }

            int goldReward = rng.Next(10, 31) + (wave * 3);
            int xpReward = 10 + (wave * 8);
            _player.Gold += goldReward;
            bool leveledUp = _player.GainXp(xpReward);

            int heal = (int)(_player.MaxHp * 0.15);
            _player.Heal(heal);

            string msg = $"Wave {wave} cleared!\n+{goldReward} Gold\n+{xpReward} XP\n+{heal} HP healed";

            if (isBoss)
            {
                var bonusCard = CardLibrary.GetRandomCard(rng, _player.Level);
                _player.MasterDeck.Add(bonusCard);
                msg += $"\nBoss bonus: NEW CARD [{bonusCard.Name}]!";
            }

            if (leveledUp)
                msg += $"\n\nLEVEL UP! You are now Level {_player.Level}!";

            MessageBox.Show(msg, $"Arena Wave {wave}");

            if (wave > _bestWave) _bestWave = wave;
            RefreshUI();
        }
    }

    private void LeaveButton_Click(object sender, RoutedEventArgs e) => Close();
}
