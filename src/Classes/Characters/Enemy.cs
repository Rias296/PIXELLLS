using Godot;
using System;
using System.Collections.Generic;

public class Enemy : Character
{
	protected new Vector2 _velocity = Vector2.Zero;
	private AnimatedSprite _animatedSprite;
	

	// State related fields
	private float _searchTimer = 0f;
	private Vector2 _lastKnownPlayerPos;
	private Node2D _player;

	// Enemy stats
	private EnemyStats stats;
	private PersonalityType personality;

	private RayCast2D _raycast;
	private Vector2 facingDirection = Vector2.Right;
	
	// For searching breadcrumbs
	private List<Breadcrumb> visibleBreadcrumbs = new List<Breadcrumb>();

	public override void _Ready()
	{
		_player = GetNode<Node2D>("/root/TestMap/Player");
		_animatedSprite = GetNode<AnimatedSprite>("./Pivot/Character_Animation");
		_raycast = GetNode<RayCast2D>("./RayCast2D");
		_raycast.Enabled = true;
		if (_animatedSprite == null)
		{
			GD.PrintErr("AnimatedSprite not found!");
		}

		// Normalize to avoid faster diagonal movement
		if (_velocity.Length() > 0)
		{
			_velocity = _velocity.Normalized();
		}

		// Randomize personality
		Array personalities = Enum.GetValues(typeof(PersonalityType));
		personality = (PersonalityType)personalities.GetValue(new Random().Next(personalities.Length));

		// Clone base stats
		stats = new EnemyStats
		{
			BaseSpeed = EnemyRegistry.BaseEnemyStats.BaseSpeed,
			RearVisionRange = EnemyRegistry.BaseEnemyStats.RearVisionRange,
			AttackDamage = EnemyRegistry.BaseEnemyStats.AttackDamage,
			SearchDuration = EnemyRegistry.BaseEnemyStats.SearchDuration,
			HearingThresholdDb = EnemyRegistry.BaseEnemyStats.HearingThresholdDb
		};
		// Apply personality modifier
		EnemyRegistry.ApplyPersonalityModifier(personality, stats);

		// Initialize state machine
		_stateMachine = new CharacterStateMachine();
		_stateMachine.AddState(Character_Constant.CharacterStates.IDLE);
		_stateMachine.AddState(Character_Constant.CharacterStates.MOVING);
		_stateMachine.AddState(Character_Constant.CharacterStates.CHASING);
		_stateMachine.AddState(Character_Constant.CharacterStates.SEARCHING);
		_stateMachine.AddState(Character_Constant.CharacterStates.PATROLLING);

		// Start in IDLE
		_stateMachine.ChangeState(Character_Constant.CharacterStates.IDLE);
	}


	public override void _PhysicsProcess(float delta)
	{
		UpdateAI(delta);
		UpdateMovement(delta);
		MoveAndSlide(_velocity);
		UpdateAnimation();
	}

	protected override void UpdateState()
	{
		base.UpdateState();
	}
	private void UpdateAnimation(){
		switch(current_state){
			case States.IDLE:
				_animatedSprite.Playing = true;
				_animatedSprite.Play("WitchIdle");
				GD.Print("Witch idle");
				break;
			case States.MOVE:
				_animatedSprite.Playing = true;
				_animatedSprite.Play("WitchRun");
				GD.Print("Witch running");
				break;

		}
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
		else
		{
			// Check for sound events
			var soundEvent = SoundManager.Instance.GetLatestSoundEventAboveThreshold(GlobalPosition, stats.HearingThresholdDb);
			if (soundEvent != null)
			{
				// Turn towards sound source
				Vector2 dirToSound = (soundEvent.Position - GlobalPosition).Normalized();
				facingDirection = dirToSound; 
				// Perhaps start moving towards it or set a searching state
			}
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

		// Directional vision check
		Vector2 directionToPlayer = (_player.GlobalPosition - GlobalPosition).Normalized();
		GD.Print(directionToPlayer);
		float angleindegrees = facingDirection.AngleTo(directionToPlayer) * (180/Mathf.Pi);

		float effectiveVisionRange = stats.RearVisionRange;
		if (Mathf.Abs(angleindegrees) < stats.VisionAngle)
		{
			effectiveVisionRange = stats.FrontVisionRange;
			GD.Print(effectiveVisionRange);
		}

		if (dist <= effectiveVisionRange && HasLineOfSight(_player.GlobalPosition))
		{
			return true;
		}
		return false;
	}

	private bool HasLineOfSight(Vector2 targetPos)
	{
		 // Set the raycast's target point
	_raycast.CastTo = targetPos - GlobalPosition;

	// Force the raycast to update immediately
	_raycast.ForceRaycastUpdate();

	// If the raycast is colliding and the collider isn't the player, no line of sight
	if (_raycast.IsColliding())
	{
		var collider = _raycast.GetCollider();
		// Check if the collider is the player
		if (collider == _player)
			return true;
		else
			return false;
	}

	// If not colliding with anything, line is clear
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
