using System;
using System.Collections.Generic;
using Godot;

public enum PersonalityType { Prideful, Slothful, Aggressive, Cowardly, Neutral }

public class EnemyStats
{
    public float BaseSpeed;
    public float RearVisionRange;         // Vision radius
    public float AttackDamage;
    public float SearchDuration;      // How long to search
    public float HP;
    public float HearingThresholdDb;
    public float FrontVisionRange;
    public float VisionAngle;
    public float SearchingSpeed;
    public float patrol_radius;
    public float _patrolInterval;               // How often to pick a new random point
    public float _patrolTimer;
    public float _snapfreeRadius; //Radius to prevent constant chasing

        // You can add more as needed
}

// A registry/singleton that holds base stats and personality modifiers
public static class EnemyRegistry
{
    // Base stats for this type of enemy
    public static EnemyStats BaseEnemyStats = new EnemyStats
    {
        BaseSpeed = 100f,
        SearchingSpeed = 80f,
        RearVisionRange = 200f,
        FrontVisionRange = 300f,
        VisionAngle = 90f,
        AttackDamage = 10f,
        SearchDuration = 8f,
        HP = 200f,
        HearingThresholdDb = 45f,
        patrol_radius = 250f,
        _patrolInterval = 3f,
        _snapfreeRadius = 40f
    };

    // Personality modifiers: each personality changes stats differently
    private static Dictionary<PersonalityType, Action<EnemyStats>> personalityModifiers = new()
    {
        { PersonalityType.Prideful, (stats) => {
            stats.AttackDamage *= 1.5f; 
            stats.RearVisionRange += 10f; 
            stats.HP -=3f;
            // Prideful enemies rarely run away, maybe ignore fleeing logic entirely
        }},
        { PersonalityType.Slothful, (stats) => {
            stats.RearVisionRange -= 5f; 
            stats.BaseSpeed *= 0.8f; 
            stats.HP +=5f;
            // Slothful might flee sooner or never chase aggressively
        }},
        { PersonalityType.Aggressive, (stats) => {
            stats.RearVisionRange += 5f;
            stats.BaseSpeed *= 1.2f;
            stats.AttackDamage *= 1.2f;
            stats.HP -=6f;
        }},
        { PersonalityType.Cowardly, (stats) => {
            stats.RearVisionRange += 5f;
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
