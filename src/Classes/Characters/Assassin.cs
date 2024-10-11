using Godot;
using System;


public class Assassin : Character
{
	private Vector2 _velocity = new Vector2();
	private AnimatedSprite _animatedSprite;
   
	public override void _Ready()
	{
		base._Ready();  // Call the Character's _Ready method to initialize nodes
		_animatedSprite = GetNode<AnimatedSprite>("./Pivot/Character_Animation");
	}

	

	public void GetInput(float delta)
	{
		_velocity = new Vector2();
		
		if (Input.IsKeyPressed((int)KeyList.D))
		{
			_velocity.x += 1;
		}
		if (Input.IsKeyPressed((int)KeyList.A))
		{
			_velocity.x -= 1;
		}
		if (Input.IsKeyPressed((int)KeyList.S))
		{
			_velocity.y += 1;
		}
		if (Input.IsKeyPressed((int)KeyList.W))
		{
			_velocity.y -= 1;
		}
		
		// Normalize to avoid faster diagonal movement
		if (_velocity.Length() > 0)
		{
			_velocity = _velocity.Normalized();
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		GetInput(delta);  // Handle specific player input
		
		_velocity = MoveAndSlide(_velocity * Character_Constant.MOVEMENT_SPEED);
		UpdateHorizontalDirection();
	}

		private void UpdateHorizontalDirection(){
		if (_velocity.x > 0 ){
			_animatedSprite.FlipH = false;
		}
		if (_velocity.x < 0){
			_animatedSprite.FlipH = true;
		}
	}


}
