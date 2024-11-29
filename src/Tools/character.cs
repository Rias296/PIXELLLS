using Godot;
using System;
using System.Text.RegularExpressions;

public class Character : KinematicBody2D
{
	protected Vector2 _velocity = new Vector2();


	private AnimatedSprite _animatedSprite;
	private CollisionShape2D _collisionShape;
	private enum States {IDLE, MOVE};
	private States current_state = States.IDLE;
	protected CharacterStateMachine _stateMachine;


	// MAKE SURE TO INCLUDE ALL ANIMATION IN CHARACTER NODE CLASS ANIMATION
	public override void _Ready()
	{

		//Set AnimatedSprite to avoid Null Error
		_animatedSprite = GetNode<AnimatedSprite>("./Pivot/Character_Animation");
		if (_animatedSprite == null)
		{
			GD.PrintErr("AnimatedSprite not found!");
		}

		// Initialize state machine 
		_stateMachine = new CharacterStateMachine();

		// Add states to the state machine
		_stateMachine.AddState(Character_Constant.CharacterStates.IDLE, () => _animatedSprite.Play("Idle"));
		_stateMachine.AddState(Character_Constant.CharacterStates.MOVING, () => _animatedSprite.Play("run"));

		// Set the default state to Idle
		_stateMachine.ChangeState(Character_Constant.CharacterStates.MOVING);

		
	}

	public override void _PhysicsProcess(float delta)
	{
		
		UpdateState();
		UpdateAnimation();
		
		GD.Print(_stateMachine);
	}

	private void UpdateState(){

		// If character is not moving, set state to Idle
		if (_velocity.Length() == 0)
		{
			GD.Print("State Changed to Idle");
			_stateMachine.ChangeState(Character_Constant.CharacterStates.IDLE);
		}
		else
		{
			GD.Print("State changed to moving");
			_stateMachine.ChangeState(Character_Constant.CharacterStates.MOVING);
			// GD.Print(current_state);
		}
		current_state = (States)_stateMachine.GetCurrentState();
	}

	private void UpdateAnimation(){
		switch(current_state){
			case States.IDLE:
				_animatedSprite.Playing = true;
				_animatedSprite.Play("Idle");
				GD.Print("idle");
				break;
			case States.MOVE:
				_animatedSprite.Playing = true;
				_animatedSprite.Play("run");
				GD.Print("running");
				break;

		}
	}


	
   public bool IsIdle()
	{
		return _stateMachine.IsInState(Character_Constant.CharacterStates.IDLE);
	}

	public bool IsMoving()
	{
		return _stateMachine.IsInState(Character_Constant.CharacterStates.MOVING);
	}
}
