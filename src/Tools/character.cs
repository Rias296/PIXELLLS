using Godot;
using System;
using System.Text.RegularExpressions;

public class Character : KinematicBody2D
{
	protected Vector2 _velocity = Vector2.Zero;


	private AnimatedSprite _animatedSprite;
	private CollisionShape2D _collisionShape;
	public enum States {IDLE, MOVE};
	public States current_state = States.IDLE;
	protected CharacterStateMachine _stateMachine;



	// MAKE SURE TO INCLUDE ALL ANIMATION IN CHARACTER NODE CLASS ANIMATION
	public override void _Ready()
	{

		// Initialize state machine 
		_stateMachine = new CharacterStateMachine();

		// Add states to the state machine
		_stateMachine.AddState(Character_Constant.CharacterStates.IDLE);
		_stateMachine.AddState(Character_Constant.CharacterStates.MOVING);

		

		
	}

	public override void _PhysicsProcess(float delta)
	{
		
		UpdateState();
	
	}

	protected virtual void UpdateState(){

		// If character is not moving, set state to Idle
		if (_velocity.Length() == 0)
		{
			// GD.Print("State Changed to Idle");
			_stateMachine.ChangeState(Character_Constant.CharacterStates.IDLE);
		}
		else
		{
			// GD.Print("State changed to moving");
			_stateMachine.ChangeState(Character_Constant.CharacterStates.MOVING);
			// // GD.Print(current_state);
		}
		current_state = (States)_stateMachine.GetCurrentState();
	}
// make registry, using dict
	

	
   public bool IsIdle()
	{
		return _stateMachine.IsInState(Character_Constant.CharacterStates.IDLE);
	}

	public bool IsMoving()
	{
		return _stateMachine.IsInState(Character_Constant.CharacterStates.MOVING);
	}
}
