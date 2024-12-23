using System.Collections.Generic;
using Godot;

public class Client :Node
{
	private NetworkedMultiplayerENet peer;
	private bool isConnected = false;

	public override void _Ready()
	{	
		// Initialize and connect the peer
		peer = new NetworkedMultiplayerENet();
		peer.CreateClient(Multiplayer_Constants.IP_ADDRESS, Multiplayer_Constants.PORT);
		GetTree().NetworkPeer = peer;

		// GD.Print(peer);
		// GD.Print("Client started and attempting to connect...");
		
	}

public override void _Process(float delta)
	{
		// Check if connected by verifying if network peer is set and unique ID is valid
		if (GetTree().NetworkPeer != null && GetTree().GetNetworkUniqueId() != 1 && !isConnected)
		{
			isConnected = true;
			// GD.Print("Successfully connected to the server.");
			// Start any connection-specific tasks here
			GetTree().ChangeScene("res://Scenes/Main Menu/LoginScreen.tscn");
		}
		else if (GetTree().NetworkPeer == null && isConnected)
		{
			isConnected = false;
			// GD.Print("Disconnected from server.");
			// Handle disconnection events here
		}
	}
	}






