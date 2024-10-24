using System.Collections.Generic;
using Godot;

public class Client :Node
{

	private Dictionary<int, PlayerData> otherPlayers = new Dictionary<int, PlayerData>();
	private PlayerData localPlayer;
	
	public void ConnectToServer(string ip, int port){
		var peer = new NetworkedMultiplayerENet();
		peer.CreateClient(ip, port);
		GetTree().NetworkPeer = peer;
		GetTree().SetMeta(Multiplayer_Constants.NETWORK_PEER,peer);

		 // Connect to MultiplayerAPI signals for network connection and disconnection
		GetTree().Connect("network_peer_connected", this, nameof(OnPlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

		GetTree().IsNetworkServer();
		GetTree().NetworkPeer = null;
	}




	 // Called when a new player connects (including yourself)
	public void OnPlayerConnected(int peerId)
	{
		GD.Print($"Connected to server. Peer ID: {peerId}");

		if (peerId != localPlayer.PeerId)
		{
			PlayerData newPlayer = new PlayerData(peerId);
			otherPlayers.Add(peerId, newPlayer);
		}
	}

	// Called when a player disconnects
	public void OnPlayerDisconnected(int peerId)
	{
		GD.Print($"Player {peerId} disconnected.");

		if (otherPlayers.ContainsKey(peerId))
		{
			otherPlayers.Remove(peerId);
		}
	}
	 // Update the local player position and notify the server
	public void UpdateLocalPlayerPosition(Vector3 newPosition)
	{
		localPlayer.Position = newPosition;

		// Notify the server of the updated position
		RpcUnreliable("UpdatePlayerPosition", localPlayer.PeerId, newPosition);
	}

	// Handle updates from the server about other players' positions
	[Remote]
	public void UpdatePlayerPosition(int peerId, Vector3 newPosition)
	{
		if (otherPlayers.ContainsKey(peerId))
		{
			otherPlayers[peerId].Position = newPosition;
			GD.Print($"Player {peerId} moved to {newPosition}");
		}
	}

	[Remote]
	public void NotifyPlayerJoined(int peerId)
	{
		GD.Print($"Player {peerId} joined the game.");

		if (peerId != localPlayer.PeerId && !otherPlayers.ContainsKey(peerId))
		{
			PlayerData newPlayer = new PlayerData(peerId);
			otherPlayers.Add(peerId, newPlayer);
		}
	}

	[Remote]
	public void NotifyPlayerLeft(int peerId)
	{
		GD.Print($"Player {peerId} left the game.");

		if (otherPlayers.ContainsKey(peerId))
		{
			otherPlayers.Remove(peerId);
		}
	}
}
