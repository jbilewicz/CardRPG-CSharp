using CardRPG.Models;
using System.Text.Json;
using CardRPG.Data;

namespace CardRPG.Services;

public class GameManager
{
    private Player _player;
    private User _currentUser;
    private bool _isRunning = true;


    public GameManager(User user)
    {
        _currentUser = user;

        if (!string.IsNullOrEmpty(user.SavedGameData))
        {

            Console.WriteLine("📂 Found save data. Loading character...");
            try
            {

                _player = JsonSerializer.Deserialize<Player>(user.SavedGameData);
                Console.WriteLine($"✅ Welcome back, {_player.Name}! (Level/Stats loaded)");
            }
            catch
            {
                Console.WriteLine("⚠️ Save file corrupted. Starting new game.");
                _player = new Player(user.Username);
                InitializeStarterDeck();
            }
        }
        else
        {

            Console.WriteLine("✨ New character created.");
            _player = new Player(user.Username);
            InitializeStarterDeck();
            SaveGame();
        }
    }

    private void InitializeStarterDeck()
    {
        for (int i = 0; i < 5; i++) _player.MasterDeck.Add(new Card("Strike", 1, CardType.Attack, 6));
        for (int i = 0; i < 5; i++) _player.MasterDeck.Add(new Card("Defend", 1, CardType.Defense, 5));
    }


    private void SaveGame()
    {
        using (var db = new GameDBContext())
        {

            string json = JsonSerializer.Serialize(_player);


            var userInDb = db.Users.FirstOrDefault(u => u.Id == _currentUser.Id);

            if (userInDb != null)
            {
                userInDb.SavedGameData = json;
                db.SaveChanges();


                _currentUser.SavedGameData = json;
                Console.WriteLine("💾 Game Saved!");
            }
        }
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
            Console.WriteLine("4. 🎒 Inventory");
            Console.WriteLine("5. ❌ Save & Exit Game");

            Console.Write("> ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    StartJourney();
                    SaveGame();
                    break;
                case "2":
                    EnterShop();
                    SaveGame();
                    break;
                case "3":
                    EnterTavern();
                    SaveGame();
                    break;
                case "4":
                    OpenInventory();
                    break;
                case "5":
                    SaveGame();
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
        Console.Clear();
        Console.WriteLine("🗺️ You open your map. Three dangerous stages lie ahead...");
        Console.WriteLine("[ Slime ] ----> [  Goblin ] ----> [  ORC BOSS ]");
        Console.WriteLine("\nPress any key to start the adventure...");
        Console.ReadKey();

        int totalStages = 3;
        for (int stage = 1; stage <= totalStages; stage++)
        {
            //Generating enemy 
            Enemy enemy = GenerateEnemy(stage);

            Console.Clear();
            Console.WriteLine($"\n=== STAGE {stage}/{totalStages}: {enemy.Name} ===");
            Console.WriteLine("Get ready for battle!");
            Thread.Sleep(1000);

            //Fight
            CombatEngine battle = new CombatEngine(_player, enemy);
            bool victory = battle.StartBattle();

            if (!victory)
            {
                Console.WriteLine("\nDEFEAT! You were knocked out and dragged back to town...");
                _player.CurrentHp = 1; // Punishment
                _player.Gold = Math.Max(0, _player.Gold - 10); // -10 gold per lose
                Console.ReadKey();
                return;
            }

            // rewards after win

            Console.WriteLine($"\nStage {stage} cleared!");

            //hp regeneration after fight (20% maxHP)
            int heal = (int)(_player.MaxHp * 0.2);
            _player.Heal(heal);
            Console.WriteLine($"You patch your wounds and recover {heal} HP.");

            //gold and cards
            if (stage == totalStages)//boss reward
            {
                int bossGold = 100;
                _player.Gold += bossGold;
                Console.WriteLine($"BOSS DEFEATED! You found a chest with {bossGold} Gold!");

                Card rewardCard = new Card("Execute", 2, CardType.Attack, 25);
                _player.MasterDeck.Add(rewardCard);
                Console.WriteLine($"New card unlocked: {rewardCard.Name} | {rewardCard.Value} DMG !!");

            }
            else //common reward
            {
                int gold = new Random().Next(10, 20);
                _player.Gold += gold;
                Console.WriteLine($"You found {gold} Gold.");
            }

            Console.WriteLine("\nPress any key to continue to the next stage...");
            Console.ReadKey();
        }
        Console.WriteLine("\nJOURNEY COMPLETE! You return to the city as a hero.");
        Console.ReadKey();
    }

    private Enemy GenerateEnemy(int stage)
    {
        switch (stage)
        {
            case 1:
                return new Enemy("Green Slime", 30, 4); // weak
            case 2:
                return new Enemy("Goblin Scout", 50, 8); // avg
            case 3:
                return new Enemy(" ORC WARLORD", 120, 12); // BOSS 
            default:
                return new Enemy("Unknown Entity", 10, 1);
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
            Console.WriteLine("6. ⚔️ Iron Sword (+3 DMG)       - 100 Gold");
            Console.WriteLine("7. 🧪 Big Health Potion (+50 HP) - 40 Gold");
            Console.WriteLine("8. 🔙 Exit Shop");

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
                    if (_player.Gold >= 100)
                    {
                        _player.Gold -= 100;
                        _player.Inventory.Add(new Weapon("Iron Sword", 3, 100));
                        Console.WriteLine("Bought Iron Sword!");
                    }
                    else Console.WriteLine("Not enough gold!");
                    break;

                case "7":
                    if (_player.Gold >= 40)
                    {
                        _player.Gold -= 40;
                        _player.Inventory.Add(new Consumable("Big Health Potion", 50, 40));
                        Console.WriteLine("Bought Potion!");
                    }
                    else Console.WriteLine("Not enough gold!");
                    break;
                case "8":
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
            _player.Gold -= cost;
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

    private void OpenInventory()
    {
        bool inInventory = true;
        while (inInventory)
        {
            Console.Clear();
            Console.WriteLine("===Inventory===");

            if (_player.EquippedWeapon != null)
            {
                Console.WriteLine($"Currently equipped: [{_player.EquippedWeapon.Name}]");

            }
            else
            {
                Console.WriteLine($"Currently equipped: Fists (0 DMG)");
            }

            Console.WriteLine("\n---Backpack---");
            if (_player.Inventory.Count == 0)
            {
                Console.WriteLine("Empty");
            }
            else
            {
                for (int i = 0; i < _player.Inventory.Count; i++)
                {
                    Item item = _player.Inventory[i];
                    string typeIcon = item.Type == ItemType.Weapon ? "⚔️" : "🧪";
                    Console.WriteLine($"{i + 1}. {typeIcon} {item.Name} | {item.Description}");

                }
            }
            Console.WriteLine("\n[Type number to use/equip item, or '0' to exit]");
            Console.Write("> ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 0)
                {
                    inInventory = false;
                }
                else if (choice > 0 && choice <= _player.Inventory.Count)
                {
                    Item selectedItem = _player.Inventory[choice - 1];
                    UseItem(selectedItem);
                }
            }
        }
    }

    private void UseItem(Item item)
    {
        if (item is Weapon newWeapon)
        {
            if (_player.EquippedWeapon != null)
            {
                _player.Inventory.Add(_player.EquippedWeapon);
                Console.WriteLine($"You unequipped {_player.EquippedWeapon.Name}.");

            }

            _player.EquippedWeapon = newWeapon;
            _player.Inventory.Remove(item);
            Console.WriteLine($"You equipped {newWeapon.Name}!");
        }
        else if (item is Consumable potion)
        {
            _player.Heal(potion.HealAmount);
            _player.Inventory.Remove(item);

            Console.WriteLine($"You healed for {potion.HealAmount} HP.");

        }
        SaveGame();
        Console.ReadKey();
    }
}