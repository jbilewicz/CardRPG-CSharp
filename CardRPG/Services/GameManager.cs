using CardRPG.Models;

namespace CardRPG.Services;

public class GameManager
{
    private Player _player;
    private bool _isRunning = true;

    public GameManager()
    {
        Console.WriteLine("Enter your heroe's name: ");
        string name = Console.ReadLine();
        _player = new Player(name);

        InitializeStarterDeck();
    }

    private void InitializeStarterDeck()
    {
        for (int i = 0; i < 5; i++) _player.MasterDeck.Add(new Card("Strike", 1, CardType.Attack, 6));
        for (int i = 0; i < 5; i++) _player.MasterDeck.Add(new Card("Defend", 1, CardType.Defense, 5));
    }

    public void Run()
    {
        while (_isRunning)
        {
            Console.Clear();
            Console.WriteLine($"=== CITY HUB ===");
            Console.WriteLine($"Player: {_player.Name} | Gold: {_player.Gold} | HP: {_player.CurrentHp}/{_player.MaxHp}");
            Console.WriteLine("1. 🧭 Start Journey (Fight!)");
            Console.WriteLine("2. 🛒 Shop (Buy Items)");
            Console.WriteLine("3. 🍺 Tavern (Heal)");
            Console.WriteLine("4. ❌ Exit Game");

            Console.Write("> ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    StartJourney();
                    break;
                case "2":
                    EnterShop();
                    break;
                case "3":
                    EnterTavern();
                    break;
                case "4":
                    _isRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
    }

    private void StartJourney()
    {
        Console.WriteLine("\nYou leave the city...");

        Enemy goblin = new Enemy("Goblin Scavenger", 40, 6);

        CombatEngine battle = new CombatEngine(_player, goblin);
        bool victory = battle.StartBattle();

        if (victory)
        {
            Console.WriteLine("\nVICTORY!");
            int goldReward = new Random().Next(10, 25);
            _player.Gold += goldReward;
            Console.WriteLine($"You found {goldReward} Gold!");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("\nDEFEAT! You drag yourself back to town...");
            _player.CurrentHp = 1; // lost, 1 hp remain
            Console.ReadKey();
        }
    }

    private void EnterShop()
    {
        bool inShop = true;
        while (inShop)
        {
            Console.Clear();
            Console.WriteLine("=== 🛒 GENERAL STORE ===");
            Console.WriteLine($"Gold: {_player.Gold} 💰");
            Console.WriteLine("\n-- Current Stats --");
            Console.WriteLine($"Strength: {_player.Strength} | Agility: {_player.Agility} | Intelligence: {_player.Intelligence}");
            Console.WriteLine($"Max HP: {_player.MaxHp} | Max Mana: {_player.MaxMana} | Armor: {_player.Armor}");

            Console.WriteLine("\n-- Wares --");
            Console.WriteLine("1. 💪 Potion of Strength (+1 STR)   - 50 Gold");
            Console.WriteLine("2. 🦵 Elixir of Agility (+1 AGI)    - 50 Gold");
            Console.WriteLine("3. 🧠 Scroll of Wisdom (+1 INT)     - 50 Gold");
            Console.WriteLine("4. ❤️ Heart Container (+10 Max HP)  - 75 Gold");
            Console.WriteLine("5. 🛡️ Blacksmith Armor (+5 Armor)   - 30 Gold");
            Console.WriteLine("6. 🔙 Exit Shop");

            Console.Write("> ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    TryBuyAttribute("Strength", 50, () => _player.Strength++);
                    break;
                case "2":
                    TryBuyAttribute("Agility", 50, () => _player.Agility++);
                    break;
                case "3":
                    TryBuyAttribute("Intelligence", 50, () => _player.Intelligence++);
                    break;
                case "4":
                    TryBuyAttribute("Max HP", 75, () =>
                    {
                        _player.MaxHp += 10;
                        _player.CurrentHp += 10;
                    });
                    break;
                case "5":
                    TryBuyAttribute("Armor", 30, () => _player.Armor += 5);
                    break;
                case "6":
                    inShop = false;
                    break;
                default:
                Console.WriteLine("Invalid Choice.");
                Console.ReadKey();
                break;
            }

        }
    }

    private void TryBuyAttribute(string itemName, int cost, Action onSuccess)
    {
        if (_player.Gold >= cost)
        {
            _player.Gold-=cost;
            onSuccess(); //do stats increment
            Console.WriteLine($"\nBought {itemName}! Gold left {_player.Gold}");

        }
        else
        {
            Console.WriteLine($"\n Not enough gold! You need {cost -= _player.Gold} more.");

        }
        Console.WriteLine("[Press any key]");
        Console.ReadKey();
    }

    public void EnterTavern()
    {
        Console.WriteLine("\n TAVERN ");
        int cost = 10;
        int healAmount = 20;

        Console.WriteLine($"Welcome! A meal costs {cost} Gold and heals {healAmount} HP.");
        Console.WriteLine($"Your Gold: {_player.Gold} | HP: {_player.CurrentHp}/{_player.MaxHp}");
        Console.WriteLine("Buy? (y/n)");

        if (Console.ReadLine().ToLower() == "y")
        {
            if (_player.Gold >= cost)
            {
                _player.Gold -= cost;
                _player.Heal(healAmount);
                Console.WriteLine("Yummy!");

            }
            else
            {
                Console.WriteLine("Not enough gold!");
            }
        }
        Console.ReadKey();
    }
}