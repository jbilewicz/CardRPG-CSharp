using System.Text.Json.Serialization; //for save

namespace CardRPG.Models;

public enum ItemType
{
    Weapon,
    Armor,
    Consumable 
}

[JsonDerivedType(typeof(Weapon),typeDiscriminator:"weapon")]
[JsonDerivedType(typeof(Consumable), typeDiscriminator: "consumable")]
public class Item
{
    public string Name {get;set;}
    public string Description {get;set;}
    public int Price {get;set;}
    public ItemType Type{get;set;}

    public Item(){}
    public Item(string name, string description, int price, ItemType type)
    {
        Name = name;
        Description = description;
        Price = price;
        Type= type;
    }
}

public class Weapon : Item
{
    public int DamageBonus {get;set;}

    public Weapon(){}
    public Weapon(string name, int damage, int price):base(name, $"Adds + {damage} DMG", price, ItemType.Weapon)
    {
        DamageBonus = damage;
    }
}

public class Consumable : Item
{
    public int HealAmount {get;set;}

    public Consumable(){}
    public Consumable(string name, int heal, int price):base(name, $"Heals {heal} HP", price, ItemType.Consumable)
    {
        HealAmount = heal;
    }
}