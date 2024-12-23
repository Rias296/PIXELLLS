using Godot;
using System;


public class Assassin : Character
{
	
	private AnimatedSprite _animatedSprite;
   
	public override void _Ready()
	{
		base._Ready();  // Call the Character's _Ready method to initialize nodes
		_animatedSprite = GetNode<AnimatedSprite>("./Pivot/Character_Animation");
		// Set the default state to Idle
		_stateMachine.ChangeState(Character_Constant.CharacterStates.IDLE);
		if (_animatedSprite == null)
		{
			// GD.PrintErr("AnimatedSprite not found!");
		}
		

	}

	

	public void GetInput(float delta)
	{
		_velocity = new Vector2();
		
		if (Input.IsKeyPressed((int)KeyList.D))
		{
			_velocity.x += Character_Constant.MOVEMENT_SPEED;
		}
		if (Input.IsKeyPressed((int)KeyList.A))
		{
			_velocity.x -= Character_Constant.MOVEMENT_SPEED;
		}
		if (Input.IsKeyPressed((int)KeyList.S))
		{
			_velocity.y += Character_Constant.MOVEMENT_SPEED;
		}
		if (Input.IsKeyPressed((int)KeyList.W))
		{
			_velocity.y -= Character_Constant.MOVEMENT_SPEED;
		}
		
		// Normalize to avoid faster diagonal movement
		if (_velocity.Length() > 0)
		{
			SoundManager.Instance.AddSoundEvent(GlobalPosition, Character_Constant.PLAYER_RUN_SOUND_DB);
			_velocity = _velocity.Normalized();
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		//Calls the base node of Character Class
		base._PhysicsProcess(delta);
		//// GD.Print("phase 1");
		GetInput(delta);  // Handle specific player input
		
		_velocity = MoveAndSlide(_velocity * Character_Constant.MOVEMENT_SPEED);
		
		UpdateHorizontalDirection();
		//// GD.Print("phase 2");
		UpdateAnimation();
		//// GD.Print("phase 3");
	}

		private void UpdateHorizontalDirection(){
		if (_velocity.x > 0 ){
			_animatedSprite.FlipH = false;
		}
		if (_velocity.x < 0){
			_animatedSprite.FlipH = true;
		}
	}
	protected override void UpdateState()
	{
		// Similar to base logic: IDLE if _velocity = 0, MOVING otherwise
		// But we have more states: CHASING, SEARCHING
		// Let's just call base to handle basic IDLE/MOVING
		// and then handle other states conditions if needed.
		base.UpdateState();

		// If current AI logic sets _stateMachine to CHASING or SEARCHING,
		// that will overwrite the base logic. This means the AI logic in UpdateAI 
		// sets the correct states before calling UpdateState().
	}

	// Player.cs example snippet
public void MakeNoise(float intensity)
{
	SoundManager.Instance.AddSoundEvent(GlobalPosition, intensity);
}

	private void UpdateAnimation(){
		switch(current_state){
			case States.IDLE:
				_animatedSprite.Playing = true;
				_animatedSprite.Play("PlayerIdle");
				//// GD.Print("player idle");
				break;
			case States.MOVE:
				_animatedSprite.Playing = true;
				_animatedSprite.Play("PlayerRun");
				//// GD.Print("player running");
				break;

		}
	}


}
