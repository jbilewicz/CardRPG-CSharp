using CardRPG.Core.Models;

namespace CardRPG.Core.Services;

public class CombatResult
{
    public string Message { get; set; }
    public int DamageDealt { get; set; }
    public bool IsCrit { get; set; }
    public bool IsDodge { get; set; }
    public string AbilityMessage { get; set; }
    public int ComboCount { get; set; }
    public int ComboBonus { get; set; }
}

public class StatusEffect
{
    public string Name { get; set; }
    public int DamagePerTurn { get; set; }
    public int TurnsLeft { get; set; }
    public bool AppliesToEnemy { get; set; }
}

public class CombatEngine
{
    private Player _player;
    private Enemy _enemy;
    private Random _rng = new();
    public List<StatusEffect> StatusEffects { get; } = new();
    public bool EnemyStunned { get; set; }
    public int EnemyWeakenStacks { get; set; }
    public bool FortifyActive { get; set; }
    public int BonusDrawNextTurn { get; set; }
    public int ThornsDamage { get; set; }
    public int ComboCount { get; set; }
    public CardType? LastCardType { get; set; }

    public CombatEngine(Player player, Enemy enemy)
    {
        _player = player;
        _enemy = enemy;
    }

    public List<Card> DrawHand()
    {
        foreach (var card in _player.MasterDeck)
        {
            if (card.CooldownTurns > 0) card.CooldownTurns--;
        }

        // Reset combo on new round
        ComboCount = 0;
        LastCardType = null;

        var availableCards = _player.MasterDeck.Where(c => c.CooldownTurns == 0 && !c.IsPassive).ToList();
        int baseDraw = 3 + BonusDrawNextTurn;
        BonusDrawNextTurn = 0;
        int countToTake = Math.Min(baseDraw, availableCards.Count);
        return availableCards.OrderBy(x => _rng.Next()).Take(countToTake).ToList();
    }

    public CombatResult PlayCard(Card card)
    {
        var result = new CombatResult();

        // Combo tracking
        if (LastCardType == card.Type)
            ComboCount++;
        else
            ComboCount = 1;
        LastCardType = card.Type;
        int comboBonus = ComboCount > 1 ? (ComboCount - 1) * 2 : 0;
        result.ComboCount = ComboCount;
        result.ComboBonus = comboBonus;

        if (card.Type == CardType.Attack)
        {
            int totalDamage = card.Value + _player.GetTotalDamage() + comboBonus;
            double critChance = _player.CalculateCritChance();

            if (_rng.Next(0, 100) < critChance)
            {
                totalDamage *= 2;
                result.IsCrit = true;
            }

            _enemy.TakeDamage(totalDamage);
            result.DamageDealt = totalDamage;
            result.Message = result.IsCrit
                ? $"CRITICAL! {card.Name} deals {totalDamage} damage!"
                : $"{card.Name} deals {totalDamage} damage to {_enemy.Name}.";

            if (card.Ability == CardAbility.DoubleStrike)
            {
                int secondHit = totalDamage / 2;
                _enemy.TakeDamage(secondHit);
                result.DamageDealt += secondHit;
                result.AbilityMessage = $"Double Strike! Extra {secondHit} damage!";
            }

            if (card.Ability == CardAbility.Lifesteal)
            {
                int heal = totalDamage / 2;
                _player.Heal(heal);
                result.AbilityMessage = $"Lifesteal heals you for {heal} HP!";
            }

            if (card.Ability == CardAbility.Burn)
            {
                StatusEffects.Add(new StatusEffect { Name = "Burn", DamagePerTurn = 3, TurnsLeft = 2, AppliesToEnemy = true });
                result.AbilityMessage = (result.AbilityMessage ?? "") + " Enemy is burning!";
            }

            if (card.Ability == CardAbility.Poison)
            {
                StatusEffects.Add(new StatusEffect { Name = "Poison", DamagePerTurn = 2, TurnsLeft = 3, AppliesToEnemy = true });
                result.AbilityMessage = (result.AbilityMessage ?? "") + " Enemy is poisoned!";
            }

            if (card.Ability == CardAbility.Bleed)
            {
                StatusEffects.Add(new StatusEffect { Name = "Bleed", DamagePerTurn = 2, TurnsLeft = 3, AppliesToEnemy = true });
                result.AbilityMessage = (result.AbilityMessage ?? "") + " Enemy is bleeding!";
            }

            if (card.Ability == CardAbility.Stun)
            {
                EnemyStunned = true;
                result.AbilityMessage = (result.AbilityMessage ?? "") + " Enemy is stunned!";
            }

            // Weapon enchantment proc
            if (_player.EquippedWeapon != null && _player.EquippedWeapon.Enchantment != CardAbility.None)
            {
                var ench = _player.EquippedWeapon.Enchantment;
                if (ench == CardAbility.Burn && card.Ability != CardAbility.Burn)
                {
                    StatusEffects.Add(new StatusEffect { Name = "Burn", DamagePerTurn = 2, TurnsLeft = 2, AppliesToEnemy = true });
                    result.AbilityMessage = (result.AbilityMessage ?? "") + " Weapon ignites the enemy!";
                }
                else if (ench == CardAbility.Poison && card.Ability != CardAbility.Poison)
                {
                    StatusEffects.Add(new StatusEffect { Name = "Poison", DamagePerTurn = 2, TurnsLeft = 2, AppliesToEnemy = true });
                    result.AbilityMessage = (result.AbilityMessage ?? "") + " Weapon poisons the enemy!";
                }
                else if (ench == CardAbility.Lifesteal && card.Ability != CardAbility.Lifesteal)
                {
                    int enchHeal = totalDamage / 4;
                    _player.Heal(enchHeal);
                    result.AbilityMessage = (result.AbilityMessage ?? "") + $" Weapon drains {enchHeal} HP!";
                }
                else if (ench == CardAbility.Bleed && card.Ability != CardAbility.Bleed)
                {
                    StatusEffects.Add(new StatusEffect { Name = "Bleed", DamagePerTurn = 2, TurnsLeft = 2, AppliesToEnemy = true });
                    result.AbilityMessage = (result.AbilityMessage ?? "") + " Weapon causes bleeding!";
                }
            }
        }
        else if (card.Type == CardType.Defense)
        {
            int totalArmor = card.Value + (_player.Agility / 2) + comboBonus;
            _player.Armor += totalArmor;
            result.DamageDealt = totalArmor;
            result.Message = $"Gained {totalArmor} Armor.";

            if (card.Ability == CardAbility.Fortify)
            {
                FortifyActive = true;
                result.AbilityMessage = "Armor will persist next turn!";
            }

            if (card.Ability == CardAbility.Thorns)
            {
                ThornsDamage = 3;
                result.AbilityMessage = "Thorns active! Reflects 3 damage when hit.";
            }

            if (card.Ability == CardAbility.Stun)
            {
                EnemyStunned = true;
                result.AbilityMessage = (result.AbilityMessage ?? "") + " Enemy is stunned!";
            }
        }
        else if (card.Type == CardType.Power)
        {
            if (card.Ability == CardAbility.Weaken)
            {
                EnemyWeakenStacks += 3;
                result.Message = $"Enemy weakened! Damage reduced by {EnemyWeakenStacks}.";
            }
            else if (card.Ability == CardAbility.Draw)
            {
                BonusDrawNextTurn++;
                result.Message = "Battle Trance! Draw 1 extra card next turn.";
            }
            else if (card.Ability == CardAbility.ManaSurge)
            {
                int restored = card.Value;
                _player.CurrentMana = Math.Min(_player.MaxMana, _player.CurrentMana + restored);
                result.Message = $"{card.Name} restores {restored} mana!";
            }
            else
            {
                int healValue = card.Value + comboBonus;
                _player.Heal(healValue);
                result.DamageDealt = healValue;
                result.Message = $"{card.Name} restores {healValue} HP.";
            }
        }

        card.CooldownTurns = 2;
        _player.CurrentMana -= card.Cost;
        return result;
    }

    public List<string> ProcessStatusEffects()
    {
        var messages = new List<string>();
        var expired = new List<StatusEffect>();

        foreach (var effect in StatusEffects)
        {
            if (effect.AppliesToEnemy)
            {
                _enemy.TakeDamage(effect.DamagePerTurn);
                messages.Add($"{effect.Name} deals {effect.DamagePerTurn} to {_enemy.Name}!");
            }
            else
            {
                _player.TakeDamage(effect.DamagePerTurn);
                messages.Add($"{effect.Name} deals {effect.DamagePerTurn} to you!");
            }

            effect.TurnsLeft--;
            if (effect.TurnsLeft <= 0) expired.Add(effect);
        }

        foreach (var e in expired)
        {
            StatusEffects.Remove(e);
            messages.Add($"{e.Name} wore off.");
        }

        return messages;
    }

    public string GetStatusSummary()
    {
        var parts = new List<string>();
        foreach (var e in StatusEffects.Where(e => e.AppliesToEnemy))
            parts.Add($"{e.Name}({e.TurnsLeft})");
        if (EnemyStunned) parts.Add("Stunned");
        if (EnemyWeakenStacks > 0) parts.Add($"Weak(-{EnemyWeakenStacks})");
        if (ThornsDamage > 0) parts.Add($"Thorns({ThornsDamage})");
        if (FortifyActive) parts.Add("Fortify");
        return parts.Count > 0 ? string.Join(" | ", parts) : "";
    }

    public bool HasAffordableCard(List<Card> hand)
    {
        return hand.Any(c => c.Cost <= _player.CurrentMana);
    }

    public CombatResult EnemyTurn()
    {
        var result = new CombatResult();

        if (EnemyStunned)
        {
            EnemyStunned = false;
            result.Message = $"{_enemy.Name} is stunned and cannot act!";
            _enemy.PlanNextMove();
            return result;
        }

        if (_enemy.CurrentIntent == EnemyIntent.Attack)
        {
            int damage = _enemy.IntentValue;

            if (EnemyWeakenStacks > 0)
            {
                damage = Math.Max(1, damage - EnemyWeakenStacks);
                EnemyWeakenStacks = Math.Max(0, EnemyWeakenStacks - 1);
            }

            double dodgeChance = Math.Min(50, _player.Agility * 0.5);

            if (_rng.Next(0, 100) < dodgeChance)
            {
                result.IsDodge = true;
                result.Message = "DODGED! You avoided the attack!";
            }
            else
            {
                _player.TakeDamage(damage);
                result.DamageDealt = damage;
                result.Message = $"{_enemy.Name} attacks for {damage} damage!";

                if (ThornsDamage > 0)
                {
                    _enemy.TakeDamage(ThornsDamage);
                    result.AbilityMessage = $"Thorns reflect {ThornsDamage} damage!";
                }
            }
        }
        else if (_enemy.CurrentIntent == EnemyIntent.Defend)
        {
            _enemy.Heal(_enemy.IntentValue);
            result.DamageDealt = _enemy.IntentValue;
            result.Message = $"{_enemy.Name} heals for {_enemy.IntentValue} HP.";
        }

        if (!FortifyActive)
        {
        }
        else
        {
            FortifyActive = false;
        }

        ThornsDamage = 0;
        _enemy.PlanNextMove();
        return result;
    }
}