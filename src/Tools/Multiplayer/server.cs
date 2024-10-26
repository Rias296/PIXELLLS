using Godot;
using System.Collections.Generic;

public class Server : Node
{
	private Dictionary<int, PlayerData> connectedPlayers = new Dictionary<int, PlayerData>();

    public override void _Ready()
    {
        StartServer(Multiplayer_Constants.PORT);
        //_OnPlayerConnected();
        


    }

    //Create server
    public void StartServer(int port){
		var peer = new NetworkedMultiplayerENet();
		peer.CreateServer(port);
		GetTree().NetworkPeer = peer;
		GetTree().SetMeta(Multiplayer_Constants.NETWORK_PEER,peer);

		   // Connect to MultiplayerAPI signals for player connections/disconnections
		GetTree().Connect("network_peer_connected", this, nameof(_OnPlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(_OnPlayerDisconnected));

		GD.Print("Server started on port: " + port);

		GetTree().IsNetworkServer();
		GetTree().NetworkPeer = null;

	}

	// Called when a client connects
	public void _OnPlayerConnected(int peerId)
	{
		
		GD.Print($"Player with peer ID {peerId} connected.");

		// Initialize player data
		PlayerData player = new PlayerData(peerId);
		connectedPlayers.Add(peerId, player);

		// Notify other players
		Rpc("NotifyPlayerJoined", peerId);
	}

	// Called when a client disconnects
	public void _OnPlayerDisconnected(int peerId)
	{
		
		GD.Print($"Player with peer ID {peerId} disconnected.");

		connectedPlayers.Remove(peerId);

		// Notify other players
		Rpc("NotifyPlayerLeft", peerId);
	}
	 // Broadcast player position to all clients
	public void BroadcastPlayerPosition(int peerId, Vector3 newPosition)
	{
		RpcUnreliable("UpdatePlayerPosition", peerId, newPosition);
	}

	[Remote]
	public void NotifyPlayerJoined(int peerId)
	{
		GD.Print($"Player {peerId} has joined the game.");
	}

	[Remote]
	public void NotifyPlayerLeft(int peerId)
	{
		GD.Print($"Player {peerId} has left the game.");
	}

	[Remote]
	public void UpdatePlayerPosition(int peerId, Vector3 newPosition)
	{
		// Handle the player movement update on the client-side
		if (connectedPlayers.ContainsKey(peerId))
		{
			connectedPlayers[peerId].Position = newPosition;
		}
	}
	

}
