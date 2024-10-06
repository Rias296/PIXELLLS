using Godot;
using System;
using System.Text.RegularExpressions;

public class Character : KinematicBody2D
{
	private Vector2 _velocity = new Vector2();


    private AnimatedSprite _animatedSprite;
    private CollisionShape2D _collisionShape;
    private enum States {IDLE, MOVE};
    private States current_state = States.IDLE;
    protected StateMachine<string> _stateMachine;

    public override void _Ready()
    {
        // Initialize state machine
        _stateMachine = new StateMachine<string>();

        // Add states to the state machine
        _stateMachine.AddState(States.IDLE.ToString());
        _stateMachine.AddState(States.MOVE.ToString());

        // Set the default state to Idle
        _stateMachine.ChangeState(States.IDLE.ToString());
    }

    public override void _PhysicsProcess(float delta)
    {
        
        UpdateState();
        UpdateAnimation();
    }

    private void UpdateState(){

        // If character is not moving, set state to Idle
        if (_velocity.Length() == 0)
        {
            _stateMachine.ChangeState(States.IDLE.ToString());
        }
        else
        {
            _stateMachine.ChangeState(States.MOVE.ToString());
        }
    }

    private void UpdateAnimation(){
        switch(current_state){
            case States.IDLE:
                _animatedSprite.Play("Idle");
                break;
            case States.MOVE:
                _animatedSprite.Play("run");
                break;

        }
    }
    
   public bool IsIdle()
    {
        return _stateMachine.IsInState(States.IDLE.ToString());
    }

    public bool IsMoving()
    {
        return _stateMachine.IsInState(States.MOVE.ToString());
    }
}
