using Godot;
using System;


public class Assassin : KinematicBody2D
{
	public int Speed = 250;
	private Vector2 _velocity = new Vector2();
   
	public Character character = new Character();
	
	public void GetInput()
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
		GetInput();
		// Use MoveAndSlide for proper movement with collisions and sliding
		MoveAndSlide(_velocity * Speed);
	}
}
