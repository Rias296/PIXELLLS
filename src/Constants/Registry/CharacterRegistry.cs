using Godot;
using System.Collections.Generic;

public class CharacterRegistry : Node
{
    public static CharacterRegistry Instance { get; private set; }

    // Example: Animation resources keyed by character type and animation name
    public Dictionary<string, Dictionary<string, string>> CharacterAnimations { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        CharacterAnimations = new Dictionary<string, Dictionary<string, string>>() {
            { "Player", new Dictionary<string, string>() {
                { "Idle", "res://Animations/PlayerIdle.tres" },
                { "Walk", "res://Animations/PlayerWalk.tres" },
                { "Attack", "res://Animations/PlayerAttack.tres" }
            }},
            { "EnemyGoblin", new Dictionary<string, string>() {
                { "Idle", "res://Animations/GoblinIdle.tres" },
                { "Walk", "res://Animations/GoblinWalk.tres" },
                { "Dash", "res://Animations/GoblinDash.tres" }
            }}
        };
    }
}