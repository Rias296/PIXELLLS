using Godot;
using System;

public class Camera : Camera2D
{
	// A reference to the player node.
	// Assign this in the Godot Editor, or find the player at runtime.
	
	private Node2D _player;
	private Camera2D _Camera;

	public override void _Ready()
	{
		// Get a reference to the player node when the scene is ready.
		_player = GetNode<Node2D>("../Player");
		_Camera = GetNode<Camera2D>("../Camera2D");

		_Camera.Current  = true;

	}

	public override void _Process(float delta)
	{
		if (_player == null)
			return;

		// Directly set the camera’s position to the player’s position.
		Position = _player.Position;
	}
}
