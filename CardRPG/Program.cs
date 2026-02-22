using CardRPG.Models;
using CardRPG.Services;

// 1. Setup Game
Console.WriteLine("=== INITIALIZING GAME ===");
Player hero = new Player("Hero");

// Fill Master Deck with 20 basic cards
for (int i = 0; i < 10; i++) hero.MasterDeck.Add(new Card("Sword Slash", 1, CardType.Attack, 6));
for (int i = 0; i < 10; i++) hero.MasterDeck.Add(new Card("Shield Up", 1, CardType.Defense, 5));

Enemy goblin = new Enemy("Goblin Warrior", 50, 8); // 50 HP, 8 DMG
CombatEngine engine = new CombatEngine(hero, goblin);

// 2. Battle Loop
Console.WriteLine($"A wild {goblin.Name} appears!");

while (hero.CurrentHp > 0 && goblin.CurrentHp > 0)
{
    Console.WriteLine("\n=================================");
    Console.WriteLine($"PLAYER HP: {hero.CurrentHp} (Armor: {hero.Armor}) | MANA: {hero.CurrentMana}");
    Console.WriteLine($"ENEMY HP: {goblin.CurrentHp} | INTENT: {goblin.CurrentIntent} ({goblin.IntentValue})");
    Console.WriteLine("=================================");

    // A. Player Draw Phase
    var hand = engine.DrawHand();
    
    Console.WriteLine("Choose a card to play (1-3):");
    for (int i = 0; i < hand.Count; i++)
    {
        Console.WriteLine($"[{i + 1}] {hand[i].Name} (Val: {hand[i].Value}) - {hand[i].Description}");
    }

    // B. Player Choice
    Console.Write("> ");
    string input = Console.ReadLine();
    
    if (int.TryParse(input, out int choice) && choice > 0 && choice <= hand.Count)
    {
        Card selectedCard = hand[choice - 1];
        engine.PlayCard(selectedCard);
    }
    else
    {
        Console.WriteLine("Invalid choice! You skipped your turn (punishment).");
    }

    // Check if enemy died
    if (goblin.CurrentHp <= 0) break;

    // C. Enemy Turn
    engine.EnemyTurn();
    
    // Wait for key press to continue
    Console.WriteLine("[Press Enter for next turn...]");
    Console.ReadLine();
}

// 3. Battle End
if (hero.CurrentHp > 0)
    Console.WriteLine("VICTORY! You defeated the enemy.");
else
    Console.WriteLine("DEFEAT... You died.");