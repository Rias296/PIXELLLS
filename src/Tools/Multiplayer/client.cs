using System.Collections.Generic;
using Godot;

public class Client :Node
{
	private NetworkedMultiplayerENet peer;

	public override void _Ready()
	{	
		ConnectToServer();
	}

	private void ConnectToServer()
	{
		peer = new NetworkedMultiplayerENet();
		
		Error connectionResult = peer.CreateClient(Multiplayer_Constants.IP_ADDRESS, Multiplayer_Constants.PORT);
		if (connectionResult != Error.Ok)
		{
			GD.PrintErr($"Unable to connect to the server on port {Multiplayer_Constants.PORT}. Error: {connectionResult}");
			return;
		}

		// Set the network peer directly on the SceneTree
		GetTree().NetworkPeer = peer;
		
		// Connect to the server connection signal
		
		if (GetTree().Connect("connected_to_server", this, nameof(OnConnectedToServer)) == Error.Ok){
			GD.Print("Successfully connected");
			GetTree().ChangeScene("res://Scenes/Main Menu/LoginScreen.tscn");
		}else{
			GD.Print("Did not connect");
		}

		GD.Print("Client started and attempting to connect...");
	}

	private void OnConnectedToServer()
	{
		GD.Print("Successfully connected to server!");
		// Activate login screen or change scenes here
		
	}

	 public override void _ExitTree()
	{
		if (peer != null)
		{
			peer.CloseConnection();
			peer = null;
			GD.Print("Client did not connect to server");
		}
	}





}
