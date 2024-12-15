using System;
using System.Collections.Generic;
using Godot;

public enum PersonalityType { Prideful, Slothful, Aggressive, Cowardly, Neutral }

public class EnemyStats
{
    public float BaseSpeed;
    public float VisionRange;         // Vision radius
    public float AttackDamage;
    public float SearchDuration;      // How long to search
    public float HP;
        // You can add more as needed
}

// A registry/singleton that holds base stats and personality modifiers
public static class EnemyRegistry
{
    // Base stats for this type of enemy
    public static EnemyStats BaseEnemyStats = new EnemyStats
    {
        BaseSpeed = 150f,
        VisionRange = 25f,
        AttackDamage = 10f,
        SearchDuration = 5f,
        HP = 200f
    };

    // Personality modifiers: each personality changes stats differently
    private static Dictionary<PersonalityType, Action<EnemyStats>> personalityModifiers = new()
    {
        { PersonalityType.Prideful, (stats) => {
            stats.AttackDamage *= 1.5f; 
            stats.VisionRange += 10f; 
            stats.HP -=3f;
            // Prideful enemies rarely run away, maybe ignore fleeing logic entirely
        }},
        { PersonalityType.Slothful, (stats) => {
            stats.VisionRange -= 5f; 
            stats.BaseSpeed *= 0.8f; 
            stats.HP +=5f;
            // Slothful might flee sooner or never chase aggressively
        }},
        { PersonalityType.Aggressive, (stats) => {
            stats.VisionRange += 5f;
            stats.BaseSpeed *= 1.2f;
            stats.AttackDamage *= 1.2f;
            stats.HP -=6f;
        }},
        { PersonalityType.Cowardly, (stats) => {
            stats.VisionRange += 5f;
            stats.BaseSpeed *=1.5f;
            stats.HP +=3f;
            // Cowardly flees at higher HP threshold and moves erratically
        }},
        { PersonalityType.Neutral, (stats) => {
            // No changes
        }}
    };

    public static void ApplyPersonalityModifier(PersonalityType personality, EnemyStats stats)
    {
        if (personalityModifiers.ContainsKey(personality))
            personalityModifiers[personality](stats);
    }
}
