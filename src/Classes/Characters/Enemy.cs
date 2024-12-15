using Godot;
using System;
using System.Collections.Generic;

public class Enemy : KinematicBody2D
{
    protected Vector2 _velocity = Vector2.Zero;
    private AnimatedSprite _animatedSprite;
    private CharacterStateMachine _stateMachine;

    // State related fields
    private float _searchTimer = 0f;
    private Vector2 _lastKnownPlayerPos;
    private Node2D _player;

    // Enemy stats
    private EnemyStats stats;
    private PersonalityType personality;

    // For searching breadcrumbs
    private List<Breadcrumb> visibleBreadcrumbs = new List<Breadcrumb>();

    public override void _Ready()
    {
        _player = GetNode<Node2D>("/root/Player");
        _animatedSprite = GetNode<AnimatedSprite>("./Pivot/Character_Animation");
        if (_animatedSprite == null)
        {
            GD.PrintErr("AnimatedSprite not found!");
        }

        // Randomize personality
        Array personalities = Enum.GetValues(typeof(PersonalityType));
        personality = (PersonalityType)personalities.GetValue(new Random().Next(personalities.Length));

        // Clone base stats
        stats = new EnemyStats
        {
            BaseSpeed = EnemyRegistry.BaseEnemyStats.BaseSpeed,
            VisionRange = EnemyRegistry.BaseEnemyStats.VisionRange,
            AttackDamage = EnemyRegistry.BaseEnemyStats.AttackDamage,
            SearchDuration = EnemyRegistry.BaseEnemyStats.SearchDuration
        };
        // Apply personality modifier
        EnemyRegistry.ApplyPersonalityModifier(personality, stats);

        // Initialize state machine
        _stateMachine = new CharacterStateMachine();
        _stateMachine.AddState(Character_Constant.CharacterStates.IDLE, () => _animatedSprite.Play("Idle"));
        _stateMachine.AddState(Character_Constant.CharacterStates.MOVING, () => _animatedSprite.Play("run"));
        _stateMachine.AddState(Character_Constant.CharacterStates.CHASING, () => _animatedSprite.Play("run"));
        _stateMachine.AddState(Character_Constant.CharacterStates.SEARCHING, () => _animatedSprite.Play("Idle"));

        // Start in IDLE
        _stateMachine.ChangeState(Character_Constant.CharacterStates.IDLE);
    }

    public override void _PhysicsProcess(float delta)
    {
        UpdateAI(delta);
        UpdateMovement(delta);
        MoveAndSlide(_velocity);
    }

    private void UpdateAI(float delta)
    {
        // Check line of sight
        bool canSeePlayer = CanSeePlayer();
        if (canSeePlayer)
        {
            // Update last known player position
            _lastKnownPlayerPos = _player.GlobalPosition;
        }

        switch (_stateMachine.GetCurrentState())
        {
            case Character_Constant.CharacterStates.IDLE:
                if (canSeePlayer)
                {
                    // If personality says we’re cowardly/slothful and want to flee? 
                    // Otherwise chase
                    if (ShouldFlee())
                    {
                        // Move away from player (just treat flee as a form of MOVING away)
                        // For simplicity, let's still call it MOVING but in opposite direction
                        _stateMachine.ChangeState(Character_Constant.CharacterStates.MOVING);
                    }
                    else
                    {
                        _stateMachine.ChangeState(Character_Constant.CharacterStates.CHASING);
                    }
                }
                break;

            case Character_Constant.CharacterStates.MOVING:
                // MOVING might be used for patrolling or fleeing
                // If we can't see player and we are moving randomly or fleeing,
                // we might revert to idle if nothing else happens.
                if (!canSeePlayer)
                {
                    // If we lost sight, go to SEARCHING, but only if we had a last known position
                    if (_lastKnownPlayerPos != Vector2.Zero)
                    {
                        _stateMachine.ChangeState(Character_Constant.CharacterStates.CHASING);
                    }
                    else
                    {
                        _stateMachine.ChangeState(Character_Constant.CharacterStates.SEARCHING);
                    }
                }
                break;

            case Character_Constant.CharacterStates.CHASING:
                // Chasing last known position
                if (ReachedLastKnownPosition())
                {
                    // If we arrive and still no player in sight, go search
                    _stateMachine.ChangeState(Character_Constant.CharacterStates.SEARCHING);
                    _searchTimer = 0f;
                }
                break;

            case Character_Constant.CharacterStates.SEARCHING:
                // Wander around near last known position
                _searchTimer += delta;
                if (canSeePlayer)
                {
                    _stateMachine.ChangeState(Character_Constant.CharacterStates.CHASING);
                }
                else if (_searchTimer > stats.SearchDuration)
                {
                    // Give up and go back to idle if no sign of player
                    _stateMachine.ChangeState(Character_Constant.CharacterStates.IDLE);
                }
                break;
        }
    }

    private void UpdateMovement(float delta)
    {
        switch (_stateMachine.GetCurrentState())
        {
            case Character_Constant.CharacterStates.IDLE:
                _velocity = Vector2.Zero;
                break;

            case Character_Constant.CharacterStates.MOVING:
                // Could be patrolling or fleeing
                // If fleeing, move away from player
                if (ShouldFlee())
                {
                    Vector2 away = (GlobalPosition - _player.GlobalPosition).Normalized() * stats.BaseSpeed;
                    _velocity = away;
                }
                else
                {
                    // Patrolling logic or random movement if needed
                    _velocity = Vector2.Zero; // Placeholder
                }
                break;

            case Character_Constant.CharacterStates.CHASING:
                // Move towards last known player position or breadcrumbs
                _velocity = ComputeChaseVelocity();
                break;

            case Character_Constant.CharacterStates.SEARCHING:
                // Random small movements around last known position
                _velocity = ComputeSearchVelocity();
                break;
        }
    }

    private Vector2 ComputeChaseVelocity()
    {
        // Follow breadcrumbs or go directly to last known position
        // If you have breadcrumbs dropped by player, find the closest breadcrumb to last known position.

        Breadcrumb closestCrumb = FindClosestBreadcrumb(_lastKnownPlayerPos);
        Vector2 target = _lastKnownPlayerPos;
        if (closestCrumb != null)
            target = closestCrumb.GlobalPosition;

        Vector2 dir = (target - GlobalPosition).Normalized();
        return dir * stats.BaseSpeed;
    }

    private Vector2 ComputeSearchVelocity()
    {
        // Random wandering around _lastKnownPlayerPos
        // You could pick a random direction and move slightly.
        Vector2 randomDir = new Vector2((float)(GD.RandRange(-1,1)), (float)(GD.RandRange(-1,1))).Normalized();
        return randomDir * (stats.BaseSpeed * 0.5f);
    }

    private bool ReachedLastKnownPosition()
    {
        return GlobalPosition.DistanceTo(_lastKnownPlayerPos) < 10f;
    }

    private bool CanSeePlayer()
    {
        float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
        return dist <= stats.VisionRange && HasLineOfSight(_player.GlobalPosition);
    }

    private bool HasLineOfSight(Vector2 targetPos)
    {
        // Implement a raycast or simply return true for now
        // In a real scenario, cast a ray to check obstacles
        return true;
    }

    private bool ShouldFlee()
    {
        // Depends on personality.
        // For example, Slothful or Cowardly enemies flee more often:
        if (personality == PersonalityType.Slothful || personality == PersonalityType.Cowardly)
        {
            // Just always flee if see player
            return true;
        }
        // Otherwise, maybe check HP or other conditions
        return false;
    }

    private Breadcrumb FindClosestBreadcrumb(Vector2 referencePos)
    {
        Breadcrumb closest = null;
        float closestDist = Mathf.Inf;
        foreach (Breadcrumb b in GetTree().GetNodesInGroup("breadcrumbs"))
        {
            float dist = referencePos.DistanceTo(((Node2D)b).GlobalPosition);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = b;
            }
        }
        return closest;
    }
}
