using System.Collections.Generic;
using Godot;

public class Client :Node
{


	public override void _Ready()
	{
		ConnectToServer(Multiplayer_Constants.IP_ADDRESS,Multiplayer_Constants.PORT);
		GD.Print("Connected to server");
	}

	public void ConnectToServer(string ip, int port){
		var peer = new NetworkedMultiplayerENet();
		peer.CreateClient(ip, port);
		GetTree().NetworkPeer = peer;
		GetTree().SetMeta(Multiplayer_Constants.NETWORK_PEER,peer);

		 // Connect to MultiplayerAPI signals for network connection and disconnection
		GetTree().IsNetworkServer();
		GetTree().NetworkPeer = null;
	}





}
