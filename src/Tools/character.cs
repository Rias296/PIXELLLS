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
    private States new_state;

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

    private void UpdateState(){

        if(IsOnFloor()){
            if(_velocity.x ==0){
                current_state = States.IDLE;
            }
            else{
                current_state = States.MOVE;
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
