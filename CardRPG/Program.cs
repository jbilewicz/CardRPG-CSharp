using CardRPG.Models;

Player hero = new Player("Warrior");
Console.WriteLine($"New player has been created! Name:\t {hero.Name} || HP: {hero.CurrentHp}/{hero.MaxHp}");

Card fireball = new Card("Fireball", 2, CardType.Attack, 15);
Console.WriteLine($"New card: {fireball.Name} | Cost: {fireball.Cost} mana | {fireball.Description}");

Console.WriteLine("\n--- Fight Test ---");
int dmg = fireball.Value + (hero.Strength / 2); 
Console.WriteLine($"Attacking with {dmg} damage (Base: {fireball.Value} + Strength Bonus)");

Console.WriteLine("\n--- Defense Test ---");
hero.Armor = 5;
Console.WriteLine($"You have {hero.Armor} armor.");
Console.WriteLine("Monster will attack with 12 dmg!");
hero.TakeDamage(12);
Console.WriteLine($"HP after attack: {hero.CurrentHp}/{hero.MaxHp} (Armor reduce damage)");

Console.ReadKey();