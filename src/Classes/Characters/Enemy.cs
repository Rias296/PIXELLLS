using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public class Enemy : Character //TODO IMPROVE SEARCHING ABILITY OF ENEMY  
//TODO there is an issue with the fixing of constant chasing, where in search state, enemy will move towards wall (so appears no movement)
//TODO there is an issue where the enemy sometimes in searching goes beyond the last known player position, creating a weird logic
//TODO perhaps make different freeradius, whihc is a subcategory of the overeall intelligenec of the enemy
{
	protected new Vector2 _velocity = Vector2.Zero;
	private AnimatedSprite _animatedSprite;
	

	// State related fields
	private float _searchTimer = 0f;
	private bool reachedLastKnownPos;
	private int currentDirectionIndex;
	private Vector2 searchStartPos;
	private float searchDirectionMoveProgress;

	private Vector2 _lastKnownPlayerPos;
	private Node2D _player;

	// Enemy stats
	private EnemyStats stats;
	private PersonalityType personality;

	private RayCast2D _raycast;
	private Vector2 facingDirection = Vector2.Right;
	private float movementStepDistance = 50f; // how far to step in each direction
	
	private List<Vector2> searchDirections = new List<Vector2>();

	// For searching breadcrumbs
	private List<Breadcrumb> visibleBreadcrumbs = new List<Breadcrumb>();
	private Vector2 _patrolCenter;
	private Vector2 _currentPatrolTarget;
	private float _patrolTimer;
	private float chaseTimer = 0f;
	private float chaseLimit = 5f;
	private NavigationAgent2D _navigationAgent2D;

	public override void _Ready()
	{
		_player = GetNode<Node2D>("/root/TestMap/Player");
		_animatedSprite = GetNode<AnimatedSprite>("./Pivot/Character_Animation");
		_raycast = GetNode<RayCast2D>("./RayCast2D");
		_navigationAgent2D = GetNode<NavigationAgent2D>("./NavigationAgent2D");
		_raycast.Enabled = true;
		if (_animatedSprite == null)
		{
			// GD.PrintErr("AnimatedSprite not found!");
		}

		// Normalize to avoid faster diagonal movement
		if (_velocity.Length() > 0)
		{
			_velocity = _velocity.Normalized();
		}

		// Randomize personality
		Array personalities = Enum.GetValues(typeof(PersonalityType));
		personality = (PersonalityType)personalities.GetValue(new Random().Next(personalities.Length));

		// GD.Print(personality);
		// Clone base stats
		stats = new EnemyStats
		{
			BaseSpeed = EnemyRegistry.BaseEnemyStats.BaseSpeed,
			SearchingSpeed = EnemyRegistry.BaseEnemyStats.SearchingSpeed,
			RearVisionRange = EnemyRegistry.BaseEnemyStats.RearVisionRange,
			FrontVisionRange = EnemyRegistry.BaseEnemyStats.FrontVisionRange,
			AttackDamage = EnemyRegistry.BaseEnemyStats.AttackDamage,
			SearchDuration = EnemyRegistry.BaseEnemyStats.SearchDuration,
			HearingThresholdDb = EnemyRegistry.BaseEnemyStats.HearingThresholdDb,
			_patrolInterval = EnemyRegistry.BaseEnemyStats._patrolInterval,
			patrol_radius = EnemyRegistry.BaseEnemyStats.patrol_radius,
			_snapfreeRadius = EnemyRegistry.BaseEnemyStats._snapfreeRadius
		};
		
		// Apply personality modifier
		EnemyRegistry.ApplyPersonalityModifier(personality, stats);

		

		//Initialise Search Direction
		searchDirections.Add(Vector2.Left);
		searchDirections.Add(Vector2.Right);
		searchDirections.Add(Vector2.Up);
		searchDirections.Add(Vector2.Down);

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
				// GD.Print("Witch idle");
				break;
			case States.MOVE:
				_animatedSprite.Playing = true;
				_animatedSprite.Play("WitchRun");
				// GD.Print("Witch running");
				break;

		}
	}

	private RID GetRID(){
		return _navigationAgent2D.GetNavigationMap();
	}
	private Vector2 FindWaypoint(){
		Vector2 nextPathPoint = _navigationAgent2D.GetNextLocation();
		Vector2 direction = (nextPathPoint - GlobalPosition).Normalized();
		// Apply your speed
		Vector2 _velocity = direction * stats.BaseSpeed;
		return _velocity;

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
			
			GD.Print("Enemy in Idle");
				_velocity = Vector2.Zero;
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
			GD.Print("Enemy in MOVING");
				// MOVING might be used for patrolling or fleeing
				// If we can't see player and we are moving randomly or fleeing,
				// we might revert to idle if nothing else happens.
				if (canSeePlayer)
				{

					float distToTarget = GlobalPosition.DistanceTo(_lastKnownPlayerPos);
					float step = stats.BaseSpeed * delta;
					Vector2 directionToTarget = (_lastKnownPlayerPos - GlobalPosition).Normalized();

					if (distToTarget <= step)
					{
						// Snap directly to the last known position
						GlobalPosition = _lastKnownPlayerPos;
						_velocity = Vector2.Zero;
					}
					else
					{
						_velocity = directionToTarget * stats.BaseSpeed;
					}

					if (ShouldFlee())
					{
						Vector2 away = (GlobalPosition - _player.GlobalPosition).Normalized() * stats.BaseSpeed;
						_velocity = away;
					}

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
			GD.Print("Enemy in chasing");
				UpdateChaseLogic(delta,canSeePlayer);
				break;

			case Character_Constant.CharacterStates.SEARCHING:
			GD.Print("Enemy in SEARCHING");
				UpdateSearchLogic(delta,canSeePlayer);
				break;

			case Character_Constant.CharacterStates.PATROLLING:
				GD.Print("Enemy in Patrol");
				UpdatePatrolling(delta);
				//if (_velocity == Vector2.Zero){
				//	_stateMachine.ChangeState(Character_Constant.CharacterStates.IDLE);
				//}
				break;
		};
	}
	private void EnterSearchState()
{
	_stateMachine.ChangeState(Character_Constant.CharacterStates.SEARCHING);
	chaseTimer = 0f;    // Reset so it won't auto-expire again
	_searchTimer = 0f;
	reachedLastKnownPos = false;
	currentDirectionIndex = 0;
	searchStartPos = _lastKnownPlayerPos == Vector2.Zero ? GlobalPosition : _lastKnownPlayerPos;
	searchDirectionMoveProgress = 0f;
}

private void UpdateChaseLogic(float delta, bool canSeePlayer)
{
	// If we lose sight, start the chase timer
	if (!canSeePlayer) {
		chaseTimer += delta;
		if (chaseTimer > chaseLimit) {
			GD.Print("Timer expired, enetered searching");
			FindNearestFreePoint(_velocity, stats._snapfreeRadius);
			EnterSearchState();
			return;
		}
	}

	// If we haven't reached the last known position
	if (!ReachedLastKnownPosition()) {
		// In Godot 3.5, the NavigationAgent2D can compute paths
		// In each frame, call something like:
		_navigationAgent2D.TargetLocation = _lastKnownPlayerPos;
		GD.Print(_navigationAgent2D.TargetLocation);
		Vector2 nextPoint = _navigationAgent2D.GetNextLocation(); // this is a constant
		GD.Print(nextPoint);
		// Move towards that point
		Vector2 direction = (nextPoint - GlobalPosition).Normalized();
		_velocity = direction * stats.BaseSpeed;
	}
	else {
		// If we've reached the last known pos (and can't see the player), switch to search
		if (!canSeePlayer) {
			GD.Print("enter search");
			EnterSearchState();
		}
	}
}

private void UpdateSearchLogic(float delta, bool canSeePlayer)
{
	_searchTimer += delta;

	if (canSeePlayer) {
		_stateMachine.ChangeState(Character_Constant.CharacterStates.CHASING);
		return;
	}

	if (!reachedLastKnownPos)
	{
		// Move to last known player position using nav agent
		_navigationAgent2D.TargetLocation = _lastKnownPlayerPos;
		Vector2 nextPoint = _navigationAgent2D.GetNextLocation();
		Vector2 direction = (nextPoint - GlobalPosition).Normalized();
		_velocity = direction * stats.SearchingSpeed;

		if (ReachedLastKnownPosition()) {
			reachedLastKnownPos = true;
			_velocity = Vector2.Zero;
			currentDirectionIndex = 0;
			searchDirectionMoveProgress = 0f;
			searchStartPos = GlobalPosition; 
		}
	}
	else
	{
		// Actually searching in multiple directions
		if (currentDirectionIndex < searchDirections.Count)
		{
			Vector2 dir = searchDirections[currentDirectionIndex].Normalized();
			PerformSearchDirectionCheck(dir, delta);
		}
		else
		{
			// Done searching
			EnterPatrolState();
		}
	}

	if (_searchTimer > stats.SearchDuration)
	{
		EnterPatrolState();
	}
}



private Vector2 FindNearestFreePoint(Vector2 original, float SnapFreeRadius , int samples = 8)
{
	var space = GetWorld2d().DirectSpaceState;

	// sample directions in a circle around 'original'
	for (int i = 0; i < samples; i++)
	{
		float angle = i * (Mathf.Pi * 2f / samples);
		Vector2 checkPos = original + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * SnapFreeRadius;

		var result = space.IntersectPoint(checkPos, maxResults: 1, exclude: null, collisionLayer: 1, collideWithAreas: false, collideWithBodies: true);
		if (result.Count == 0)
		{
			// we found a collision-free sample
			return checkPos;
		}
	}

	// If all samples are blocked, fallback to something else
	return GlobalPosition; 
}

private void EnterPatrolState()
{
   _stateMachine.ChangeState(Character_Constant.CharacterStates.PATROLLING);

	// If you want to start patrolling around current position
	_patrolCenter = GlobalPosition;
	_currentPatrolTarget = GlobalPosition; // Will pick new target immediately

	_patrolTimer = 0f;
	_searchTimer = 0f;
	_velocity = Vector2.Zero;
}

private void UpdatePatrolling(float delta)
{
	// If we see the player mid-patrol, switch to CHASING or MOVING
	if (CanSeePlayer())
	{
		if (ShouldFlee())
			_stateMachine.ChangeState(Character_Constant.CharacterStates.MOVING);
		else
			_stateMachine.ChangeState(Character_Constant.CharacterStates.CHASING);
		return;
	}

	// Timer that picks a new random position every _patrolInterval seconds
	_patrolTimer += delta;
	if (_patrolTimer >= stats._patrolInterval)
	{
		_patrolTimer = 0f;
		// Pick a new random target within radius
		_currentPatrolTarget = PickRandomPointInRadius(_patrolCenter, stats.patrol_radius);
	}

	// Move toward the current patrol target
	if (GlobalPosition.DistanceTo(_currentPatrolTarget) < 10f)
	{
		// Reached target. Optionally, pick a new target right away or wait
		_velocity = Vector2.Zero;
	}
	else
	{
		Vector2 dir = (_currentPatrolTarget - GlobalPosition).Normalized();
		_velocity = dir * (stats.BaseSpeed * 0.5f); // patrolling speed
	}
}

private Vector2 PickRandomPointInRadius(Vector2 center, float radius)
{
	// Pick a random direction, random distance up to 'radius'
	var rand = new Random();
	float angle = (float)(rand.NextDouble() * 2.0 * Math.PI);
	float dist = (float)(rand.NextDouble() * radius);
	Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
	return center + offset;
}




	private void UpdateSearching(float delta){
		_searchTimer += delta;

		// If we see player again while searching, start chasing
		if (CanSeePlayer())
		{
			_stateMachine.ChangeState(Character_Constant.CharacterStates.CHASING);
			return;
		}

		if (!reachedLastKnownPos)
		{
			// First, ensure we move to the last known player position
			if (ReachedLastKnownPosition())
			{
				// GD.Print("lAST PLAER POSITION KNOWN");
				reachedLastKnownPos = true;
				_velocity = Vector2.Zero;
				// Now begin directional checks
				currentDirectionIndex = 0;
				searchDirectionMoveProgress = 0f;
			}
			else
			{
				//Searching speed relative to player's last known position
				_velocity = (_lastKnownPlayerPos - GlobalPosition).Normalized() * stats.SearchingSpeed;
			}
		
		}
		else
		{
			// Perform the scanning of directions
			if (currentDirectionIndex < searchDirections.Count)
			{
				Vector2 dir = searchDirections[currentDirectionIndex].Normalized();
				PerformSearchDirectionCheck(dir, delta);
			}
			else
			{
				// After checking all directions, return to idle or maybe patrol
				_stateMachine.ChangeState(Character_Constant.CharacterStates.PATROLLING);
			}
		}


	}

	private void PerformSearchDirectionCheck(Vector2 direction, float delta) //  TODO COMPLETE THIS
	{

		
	// Attempt to see if we can path ~'movementStepDistance' in that direction
	Vector2 desiredTarget = GlobalPosition + direction * movementStepDistance;
	_navigationAgent2D.TargetLocation = desiredTarget;
	Vector2 nextPoint = _navigationAgent2D.GetNextLocation();

	if (nextPoint.DistanceTo(GlobalPosition) < 2f) {
		// Means the path is blocked or extremely short -> skip this direction
		currentDirectionIndex++;
		_velocity = Vector2.Zero;
		searchDirectionMoveProgress = 0f;
		return;
	}

	// Continue the original logic for forward/back movement, but using the nav agent.
	if (searchDirectionMoveProgress < movementStepDistance) {
		// Move forward
		Vector2 forwardDir = (nextPoint - GlobalPosition).Normalized();
		_velocity = forwardDir * stats.SearchingSpeed;
		searchDirectionMoveProgress += stats.SearchingSpeed * delta;
		
	}
	

		// There are two phases: move forward in that direction by a few steps, then return
		
		if (searchDirectionMoveProgress < movementStepDistance)
		{
			// Move forward in this direction
			_velocity = direction * stats.SearchingSpeed;
			searchDirectionMoveProgress += stats.SearchingSpeed * delta;
			if (searchDirectionMoveProgress >= movementStepDistance)
			{
				// Reached the forward limit, next step: go back to start pos
			}
		}
		else if (searchDirectionMoveProgress < (movementStepDistance * 2f))
		{
			// Move back to the last known player pos
			_velocity = (searchStartPos - GlobalPosition).Normalized() * stats.SearchingSpeed;
			float distToStart = GlobalPosition.DistanceTo(searchStartPos);
			if (distToStart < 5f) // close enough to start pos
			{
				// Reset progress and move to next direction
				_velocity = Vector2.Zero;
				currentDirectionIndex++;
				searchDirectionMoveProgress = 0f;
			}
			else
			{
				searchDirectionMoveProgress += stats.SearchingSpeed * delta;
			}
		}
	}

	private bool ReachedLastKnownPosition()
	{
		GD.Print(GlobalPosition.DistanceTo(_lastKnownPlayerPos));
		return GlobalPosition.DistanceTo(_lastKnownPlayerPos) < 10f;
	}


	private bool CanSeePlayer()
	{
		float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);

		// Directional vision check
		Vector2 directionToPlayer = (_player.GlobalPosition - GlobalPosition).Normalized();
		
		float angleindegrees = facingDirection.AngleTo(directionToPlayer) * (180/Mathf.Pi);

		float effectiveVisionRange = stats.RearVisionRange;
		if (Mathf.Abs(angleindegrees) < stats.VisionAngle)
		{
			effectiveVisionRange = stats.FrontVisionRange;
			//// GD.Print(effectiveVisionRange);
		}

		if (dist <= effectiveVisionRange && HasLineOfSight(_player.GlobalPosition))
		{
			//// GD.Print(_player.GlobalPosition);
			return true;
		}
		return false;
	}

	private bool HasLineOfSight(Vector2 targetPos)
	{
		 // Set the raycast's target point
	_raycast.CastTo = targetPos - GlobalPosition;

	//// GD.Print(_raycast.CastTo);
	// Force the raycast to update immediately
	_raycast.ForceRaycastUpdate();

	// If the raycast is colliding and the collider isn't the player, no line of sight
	if (_raycast.IsColliding())
	{
		var collider = _raycast.GetCollider();
		// GD.Print(collider);
		// GD.Print(_player);
		//GD.Print(_player == collider);
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


}


