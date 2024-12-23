using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using File = System.IO.File;

public class Server : Node
{
	
	private UDPServer server = new UDPServer();
	private JObject database = new JObject();
	private JObject loggedUsers = new JObject();
	private string database_file_path = "./data/FakeData.json";
	public override void _Ready()
	{
		NetworkedMultiplayerENet EnetServer = new NetworkedMultiplayerENet();
		EnetServer.CreateServer(Multiplayer_Constants.PORT);
		GetTree().NetworkPeer = EnetServer;
		// GD.Print(EnetServer);
		

		   // Connect to MultiplayerAPI signals for player connections/disconnections
		
		// GD.Print("Server started on port: " + Multiplayer_Constants.PORT);

		// GD.Print(GetTree().IsNetworkServer());
		
		server.Listen(9999);
		// GD.Print("listening to server port");

		GetTree().Connect("network_peer_connected", this, nameof(OnClientConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(OnClientDisconnected));
	

		
		Load_database(database_file_path);

	}

	 private void OnClientConnected(int id)
	{
		// GD.Print($"Client {id} connected");
	}

	private void OnClientDisconnected(int id)
	{
		// GD.Print($"Client {id} disconnected");
	}

	public override void _Process(float delta)
	{	
		if(server.IsListening()){ //THIS DOESNT GET ACTIVATED
			// GD.Print(server.Poll());

		}
		
		if (server.IsConnectionAvailable()){
			var peer = server.TakeConnection();
			var message = JObject.Parse((string)peer.GetVar());
			
			if (message.ContainsKey("authenticate_credentials")){
				Authenticate_Player(peer,message);
			}
			if (message.ContainsKey("get_authentication_token")){
				GetAuthenticationToken(peer,message);
			}
			if (message.ContainsKey("get_avatar")){
				GetAvatar(peer,message);
			}
	}
}


	
	public void Load_database(string path_to_database_file){
		using (StreamReader reader = new StreamReader(path_to_database_file))
	{
		string fileContent = reader.ReadToEnd();
		database = JObject.Parse(fileContent);
	}
	}

	private void Authenticate_Player(PacketPeerUDP peer, JObject message){
		if (message["authenticate_credentials"] is JObject credentials){
			string user = credentials.Value<string>("user");
			string password = credentials.Value<string>("password");

			if (database.ContainsKey(user) && database[user]["password"].ToString() == password){

				uint token = GD.Randi();
				JObject response = new JObject
				{
					["token"] = token
				};

				loggedUsers[user]= (int)token;

				peer.PutVar(response.ToString());
				
			}
		}
	}

	private void GetAvatar(PacketPeerUDP peer, JObject message){
		 if (message.ContainsKey("user"))
		{
			string user = (string)message["user"];

			// Validate user token
			if (loggedUsers.ContainsKey(user) && message["token"] == loggedUsers[user])
			{
				// Retrieve avatar and nickname from the database
				string avatar = (string)database[user]["avatar"];
				string nickname = (string)database[user]["name"];

				// Create response dictionary
				var response = new JObject
				{
					["avatar"] = avatar,
					["name"] = nickname
				};

				// Send response back to client
				peer.PutVar(response.ToString());
			}
			else
			{
				// GD.Print("Invalid token for user.");
				peer.PutVar(""); // Optional: send an empty or error response
			}
		}
		else
		{
			// GD.Print("User key missing in message.");
			peer.PutVar(""); // Optional: send an empty or error response
		}
	}

	private void GetAuthenticationToken(PacketPeerUDP peer, JObject message){
		var credentials = message;
		if (credentials.ContainsKey("user")){
			string user = credentials["user"].ToString();

			if (credentials["token"] == loggedUsers[user]){
				JToken token = loggedUsers[credentials["user"]];

				var response = new JObject{
					["token"] = token,
					["user"] = user
				};
				peer.PutVar(JsonConvert.SerializeObject(token));
			}
		}
	}
}
