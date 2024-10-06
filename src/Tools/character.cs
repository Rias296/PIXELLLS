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
    

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite>("Assassin_Idle");
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape");
    }

    public override void _PhysicsProcess(float delta)
    {
        
        UpdateState();
        UpdateAnimation();
    }

    private void ChangeState(States new_state)
    {
        if(current_state != new_state){
            current_state = new_state;
            GD.Print("State Changed into: " + new_state);
        }
    }
    private void UpdateState(){

        if(IsOnFloor()){
            if(_velocity.x ==0){
                ChangeState(States.IDLE);
            }
            else{
                ChangeState(States.MOVE);
            }
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
    
   
}
