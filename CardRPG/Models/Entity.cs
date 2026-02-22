namespace CardRPG.Models;

public abstract class Entity
{
    public string Name { get; set; }
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public bool IsDead => CurrentHp <= 0;

    protected Entity(string name, int maxHp)
    {
        Name = name;
        MaxHp = maxHp;
        CurrentHp = maxHp;
    }

    //virtual allows to override methods
    public virtual void TakeDamage(int damage)
    {
        CurrentHp -= damage;
        if (CurrentHp < 0) CurrentHp = 0;
    }

    public void Heal(int amount)
    {
        CurrentHp += amount;
        if (CurrentHp > MaxHp) CurrentHp = MaxHp;
    }
}