namespace CardRPG.Models;

public enum CardType
{
    Attack,
    Defense,
    Power
}

public class Card
{
    public string Name {get; set;}
    public string Description {get;set;}
    public int Cost {get;set;}
    public CardType Type {get;set;}

//Core Value ex. 10 damage or 10 armor
    public int Value {get;set;}

    public Guid Id{get; private set;} = Guid.NewGuid();

    public Card(string name, int cost, CardType type, int value)
    {
        Name = name;
        Cost = cost;
        Type = type;
        Value = value;
        Description = type == CardType.Attack ? $"Deal {value} damage." : $"Earn {value} armor.";
    }

}