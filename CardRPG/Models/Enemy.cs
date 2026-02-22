namespace CardRPG.Models;

public enum EnemyIntent
{
    Attack,
    Defend,
    Buff
}

public class Enemy : Entity
{
    public int BaseDamage { get; set; }
    public EnemyIntent CurrentIntent { get; private set; }
    public int IntentValue { get; private set; }

    public Enemy(string name, int hp, int damage) : base(name, hp)
    {
        BaseDamage = damage;
        PlanNextMove();
    }

    public void PlanNextMove()
    {
        Random rnd = new Random();
        int roll = rnd.Next(1, 101);

        if (roll <= 60) 
        {
            CurrentIntent = EnemyIntent.Attack;
            IntentValue = BaseDamage + rnd.Next(-1, 2); 
        }
        else 
        {
            CurrentIntent = EnemyIntent.Defend;
            IntentValue = 5; 
        }
    }
}