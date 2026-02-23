using CardRPG.Models;

namespace CardRPG.Services;

public class CombatEngine
{
    private Player _player;
    private Enemy _enemy;
    private Random _rng = new Random();

    public CombatEngine(Player player, Enemy enemy)
    {
        _player = player;
        _enemy = enemy;
    }

    // **Card Logic**
    public List<Card> DrawHand()
    {
        //decreasing all cooldowns for all cards in deck (start of turn)
        foreach (var card in _player.MasterDeck)
        {
            if (card.CooldownTurns > 0) card.CooldownTurns--;
        }

        //filter available cards (Cooldown == 0)

        var availableCards = _player.MasterDeck.Where(c=>c.CooldownTurns == 0).ToList();

        //shuffle cards, and pick 3
        //if we have less than 3 cards, take all available
        int countToTake = Math.Min(3, availableCards.Count); //3, if less , take all
        var hand = availableCards.OrderBy(x => _rng.Next()).Take(countToTake).ToList();

        return hand;
        
    }

    public void PlayCard(Card card)
    {
        Console.WriteLine($"Player uses: {card.Name}!");

        //Apply effect based on type
        if (card.Type == CardType.Attack)
        {
            int totalDamage = card.Value + _player.GetTotalDamage();

            double critChance = _player.CalculateCritChance();
            if (_rng.Next(0, 100) < critChance)
            {
                totalDamage *= 2;
                Console.WriteLine("Critical Hit!");
            }
            _enemy.TakeDamage(totalDamage);
            Console.WriteLine($"Dealt {totalDamage} damage to {_enemy.Name}.");
        }else if(card.Type == CardType.Defense){
            int totalArmor = card.Value + (_player.Agility/2);
            _player.Armor += totalArmor;
            Console.WriteLine($"Gained {totalArmor} armor.");
        }
        card.CooldownTurns = 3;
    }

    public void EnemyTurn()
    {
        Console.WriteLine($"{_enemy.Name}'s Turn!");
        if(_enemy.CurrentIntent == EnemyIntent.Attack)
        {
            //Enemy attack -> Player tries to dodge (agility)
            double dodgeChance = Math.Min(50,_player.Agility * 0.5);

            if (_rng.Next(0, 100) < dodgeChance)
            {
                Console.WriteLine("DODGED! Player avoided the attack.");
            }
            else
            {
                _player.TakeDamage(_enemy.IntentValue);
                Console.WriteLine($"Enemy attacked for {_enemy.IntentValue}. Player HP: {_player.CurrentHp}");

            }
        }else if(_enemy.CurrentIntent == EnemyIntent.Defend)
        {
            //Enemy heals or buffs
            _enemy.CurrentHp += _enemy.IntentValue;
            if(_enemy.CurrentHp>_enemy.MaxHp) _enemy.CurrentHp = _enemy.MaxHp;
            Console.WriteLine($"Enemy healed for {_enemy.IntentValue} HP.");

        }
        _enemy.PlanNextMove();
    }

    public bool StartBattle()
    {
        Console.WriteLine($"A wild {_enemy.Name} appears!");

        while (!_player.IsDead && !_enemy.IsDead)
        {
            Console.WriteLine($"\nPLAYER HP: {_player.CurrentHp} | ENEMY: {_enemy.CurrentHp} (Intent: {_enemy.CurrentIntent}");


            //Player's turn
            var hand = DrawHand();
            Console.WriteLine("Cards: ");
            for(int i=0; i<hand.Count; i++)Console.WriteLine($"[{i+1}] {hand[i].Name} ({hand[i].Value})");

            Console.WriteLine("> ");
            if(int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= hand.Count)
            {
                PlayCard(hand[choice-1]);
            }

            if(_enemy.IsDead) return true;

            EnemyTurn();

            if(_player.IsDead) return false;
        }
        return false;
    }
}