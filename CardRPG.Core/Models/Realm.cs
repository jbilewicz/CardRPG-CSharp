namespace CardRPG.Core.Models;

public class RealmStage
{
    public string EnemyName { get; set; }
    public int EnemyHp { get; set; }
    public int EnemyDmg { get; set; }
    public bool IsBoss { get; set; }

    public RealmStage(string enemyName, int hp, int dmg, bool isBoss = false)
    {
        EnemyName = enemyName;
        EnemyHp = hp;
        EnemyDmg = dmg;
        IsBoss = isBoss;
    }
}

public class Realm
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Description { get; set; }
    public int RequiredLevel { get; set; }
    public List<RealmStage> Stages { get; set; }
    public int GoldReward { get; set; }
    public int XpReward { get; set; }
    public Card? CardReward { get; set; }

    public Realm(int id, string name, string icon, string desc, int reqLevel,
                 List<RealmStage> stages, int gold, int xp, Card? cardReward = null)
    {
        Id = id;
        Name = name;
        Icon = icon;
        Description = desc;
        RequiredLevel = reqLevel;
        Stages = stages;
        GoldReward = gold;
        XpReward = xp;
        CardReward = cardReward;
    }

    public static List<Realm> GetAllRealms() => new()
    {
        new Realm(1, "Verdant Meadows", "V", "Peaceful fields with lurking slimes.", 1,
            new List<RealmStage>
            {
                new("Green Slime", 30, 4),
                new("Blue Slime", 40, 5),
                new("Slime King", 70, 8, true),
            }, 40, 30,
            new Card("Power Strike", 2, CardType.Attack, 12, rarity: CardRarity.Uncommon)),

        new Realm(2, "Dark Forest", "F", "Ancient woods infested with beasts.", 2,
            new List<RealmStage>
            {
                new("Forest Wolf", 45, 6),
                new("Giant Spider", 55, 7),
                new("Elder Treant", 90, 10, true),
            }, 60, 50,
            new Card("Iron Wall", 2, CardType.Defense, 14, rarity: CardRarity.Uncommon)),

        new Realm(3, "Goblin Caves", "G", "Twisting tunnels ruled by goblins.", 3,
            new List<RealmStage>
            {
                new("Goblin Scout", 50, 8),
                new("Goblin Shaman", 60, 9),
                new("Goblin Chieftain", 110, 13, true),
            }, 80, 70,
            new Card("Fireball", 3, CardType.Attack, 20, ability: CardAbility.Burn, rarity: CardRarity.Rare)),

        new Realm(4, "Frozen Tundra", "T", "Blizzards hide dangerous ice creatures.", 4,
            new List<RealmStage>
            {
                new("Ice Sprite", 60, 9),
                new("Frost Bear", 80, 11),
                new("Ice Wyrm", 140, 15, true),
            }, 100, 90,
            new Card("Frost Shield", 2, CardType.Defense, 18, ability: CardAbility.Fortify, rarity: CardRarity.Rare)),

        new Realm(5, "Volcanic Depths", "D", "Rivers of lava and fire elementals.", 5,
            new List<RealmStage>
            {
                new("Magma Golem", 75, 12),
                new("Fire Elemental", 90, 14),
                new("Inferno Drake", 170, 18, true),
            }, 130, 120,
            new Card("Inferno Slash", 3, CardType.Attack, 28, ability: CardAbility.Burn, rarity: CardRarity.Rare)),

        new Realm(6, "Shadow Marsh", "S", "Cursed swamps teeming with undead.", 6,
            new List<RealmStage>
            {
                new("Skeleton Warrior", 85, 13),
                new("Wraith", 100, 15),
                new("Lich Lord", 200, 20, true),
            }, 160, 150,
            new Card("Soul Drain", 3, CardType.Attack, 20, ability: CardAbility.Lifesteal, rarity: CardRarity.Rare)),

        new Realm(7, "Crystal Caverns", "C", "Glittering caves guarded by golems.", 7,
            new List<RealmStage>
            {
                new("Crystal Golem", 100, 15),
                new("Diamond Sentinel", 120, 17),
                new("Crystal Colossus", 240, 22, true),
            }, 200, 180,
            new Card("Diamond Armor", 3, CardType.Defense, 25, ability: CardAbility.Thorns, rarity: CardRarity.Rare)),

        new Realm(8, "Dragon's Peak", "P", "Sky-high cliffs where wyverns nest.", 8,
            new List<RealmStage>
            {
                new("Young Wyvern", 110, 17),
                new("Storm Drake", 140, 19),
                new("Elder Dragon", 300, 25, true),
            }, 250, 220,
            new Card("Dragon Breath", 4, CardType.Attack, 40, ability: CardAbility.Burn, rarity: CardRarity.Rare)),

        new Realm(9, "Demon Wasteland", "W", "Scorched lands corrupted by demons.", 9,
            new List<RealmStage>
            {
                new("Imp Horde", 130, 19),
                new("Succubus", 160, 21),
                new("Demon Prince", 350, 28, true),
            }, 300, 280,
            new Card("Demonic Pact", 3, CardType.Attack, 35, ability: CardAbility.Lifesteal, rarity: CardRarity.Legendary)),

        new Realm(10, "Abyss of Eternity", "A", "The end of all things. Face the Void.", 10,
            new List<RealmStage>
            {
                new("Void Spawn", 160, 22),
                new("Abyssal Horror", 200, 25),
                new("The Void Emperor", 500, 35, true),
            }, 500, 400,
            new Card("Oblivion", 5, CardType.Attack, 60, ability: CardAbility.Burn, rarity: CardRarity.Legendary)),

        // --- Act II: The Shattered Lands ---

        new Realm(11, "Sunken Ruins", "R", "Ancient temples swallowed by the sea.", 11,
            new List<RealmStage>
            {
                new("Drowned Soldier", 180, 24),
                new("Sea Serpent", 220, 27),
                new("Kraken", 550, 38, true),
            }, 550, 450,
            new Card("Tidal Crush", 4, CardType.Attack, 45, ability: CardAbility.Stun, rarity: CardRarity.Rare)),

        new Realm(12, "Plaguelands", "P", "A rotting wasteland where disease reigns.", 12,
            new List<RealmStage>
            {
                new("Plague Rat Swarm", 200, 26),
                new("Blighted Abomination", 250, 29),
                new("Plague Doctor", 600, 40, true),
            }, 600, 500,
            new Card("Toxic Cloud", 3, CardType.Attack, 30, ability: CardAbility.Poison, rarity: CardRarity.Rare)),

        new Realm(13, "Windswept Highlands", "H", "Mountains where air elementals howl.", 13,
            new List<RealmStage>
            {
                new("Storm Hawk", 210, 27),
                new("Wind Elemental", 260, 30),
                new("Thunderbird", 650, 42, true),
            }, 650, 550,
            new Card("Lightning Bolt", 3, CardType.Attack, 35, ability: CardAbility.DoubleStrike, rarity: CardRarity.Rare)),

        new Realm(14, "Iron Fortress", "I", "A mechanical stronghold of war golems.", 14,
            new List<RealmStage>
            {
                new("Iron Sentry", 230, 28),
                new("Siege Golem", 280, 32),
                new("War Machine Alpha", 700, 44, true),
            }, 700, 600,
            new Card("Iron Bastion", 3, CardType.Defense, 30, ability: CardAbility.Fortify, rarity: CardRarity.Rare)),

        new Realm(15, "Enchanted Grove", "E", "A corrupted fairy forest full of trickery.", 15,
            new List<RealmStage>
            {
                new("Thorn Sprite", 240, 29),
                new("Dark Dryad", 300, 33),
                new("Fairy Queen", 750, 46, true),
            }, 750, 650,
            new Card("Nature's Wrath", 4, CardType.Attack, 50, ability: CardAbility.Bleed, rarity: CardRarity.Rare)),

        // --- Act III: The Forsaken Realms ---

        new Realm(16, "Necropolis", "N", "A city of the dead ruled by liches.", 16,
            new List<RealmStage>
            {
                new("Bone Colossus", 260, 31),
                new("Death Knight", 320, 35),
                new("Arch-Lich Malachar", 800, 48, true),
            }, 800, 700,
            new Card("Death's Embrace", 4, CardType.Attack, 40, ability: CardAbility.Lifesteal, rarity: CardRarity.Legendary)),

        new Realm(17, "Molten Core", "M", "Deep beneath the earth, magma flows eternal.", 17,
            new List<RealmStage>
            {
                new("Lava Elemental", 280, 33),
                new("Magma Wyrm", 340, 37),
                new("Infernal Titan", 850, 50, true),
            }, 850, 750,
            new Card("Eruption", 4, CardType.Attack, 55, ability: CardAbility.Burn, rarity: CardRarity.Legendary)),

        new Realm(18, "Astral Plane", "★", "A dimension between worlds, warping reality.", 18,
            new List<RealmStage>
            {
                new("Astral Phantom", 300, 34),
                new("Reality Bender", 360, 38),
                new("Dimensional Arbiter", 900, 52, true),
            }, 900, 800,
            new Card("Phase Shift", 3, CardType.Defense, 35, ability: CardAbility.Thorns, rarity: CardRarity.Legendary)),

        new Realm(19, "Blood Citadel", "B", "A vampire stronghold drenched in crimson.", 19,
            new List<RealmStage>
            {
                new("Vampire Thrall", 320, 36),
                new("Blood Mage", 380, 40),
                new("Vampire Lord Kael", 950, 55, true),
            }, 950, 850,
            new Card("Blood Frenzy", 4, CardType.Attack, 50, ability: CardAbility.Lifesteal, rarity: CardRarity.Legendary)),

        new Realm(20, "Frozen Abyss", "❄", "A glacier hiding an ancient evil beneath.", 20,
            new List<RealmStage>
            {
                new("Frost Revenant", 340, 38),
                new("Ice Titan", 400, 42),
                new("The Frozen One", 1000, 58, true),
            }, 1000, 900,
            new Card("Absolute Zero", 5, CardType.Attack, 65, ability: CardAbility.Stun, rarity: CardRarity.Legendary)),

        // --- Act IV: The Final War ---

        new Realm(21, "Chaos Wastes", "⚡", "Reality itself crumbles in this cursed land.", 21,
            new List<RealmStage>
            {
                new("Chaos Spawn", 360, 40),
                new("Chaos Knight", 420, 44),
                new("Lord of Chaos", 1100, 60, true),
            }, 1100, 1000,
            new Card("Chaos Bolt", 4, CardType.Attack, 55, ability: CardAbility.DoubleStrike, rarity: CardRarity.Legendary)),

        new Realm(22, "Celestial Spire", "☀", "A tower reaching into the heavens.", 22,
            new List<RealmStage>
            {
                new("Celestial Guardian", 380, 42),
                new("Seraphim Sentinel", 450, 46),
                new("Archangel Tyrael", 1200, 62, true),
            }, 1200, 1100,
            new Card("Divine Judgment", 5, CardType.Attack, 70, ability: CardAbility.Burn, rarity: CardRarity.Legendary)),

        new Realm(23, "Shadow Realm", "☾", "Eternal darkness where nightmares dwell.", 23,
            new List<RealmStage>
            {
                new("Shadow Stalker", 400, 44),
                new("Nightmare Incarnate", 480, 48),
                new("The Nightmare King", 1300, 65, true),
            }, 1300, 1200,
            new Card("Shadow Strike", 4, CardType.Attack, 60, ability: CardAbility.Bleed, rarity: CardRarity.Legendary)),

        new Realm(24, "World's Edge", "∞", "The boundary between existence and oblivion.", 24,
            new List<RealmStage>
            {
                new("Reality Fragment", 420, 46),
                new("Entropy Weaver", 500, 50),
                new("The Unmaker", 1500, 68, true),
            }, 1500, 1400,
            new Card("Entropy", 5, CardType.Attack, 75, ability: CardAbility.DoubleStrike, rarity: CardRarity.Legendary)),

        new Realm(25, "Throne of Gods", "♛", "The final battle. Challenge the Creator.", 25,
            new List<RealmStage>
            {
                new("Divine Warden", 450, 48),
                new("Titan of Creation", 550, 52),
                new("Primordial Watcher", 700, 56),
                new("The Creator", 2000, 75, true),
            }, 2000, 1800,
            new Card("Genesis", 5, CardType.Attack, 100, ability: CardAbility.Lifesteal, rarity: CardRarity.Legendary)),
    };
}
