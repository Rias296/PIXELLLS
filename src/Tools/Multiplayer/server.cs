using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using File = System.IO.File;

public class Server : Node
{
	private Dictionary<int, PlayerData> connectedPlayers = new Dictionary<int, PlayerData>();
	private UDPServer server = new UDPServer();
	private JObject database = new JObject();
	private JObject loggedUsers = new JObject();
	private string database_file_path = "./data/FakeData.json";
	public override void _Ready()
	{
		StartServer(Multiplayer_Constants.PORT);
		server.Listen((ushort)Multiplayer_Constants.PORT);
		GD.Print("listening to server port");
		//_OnPlayerConnected();
		Load_database(database_file_path);

	}

	public override void _Process(float delta)
	{
		server.Poll();
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



	//Create server
	public void StartServer(int port){
		var peer = new NetworkedMultiplayerENet();
		peer.CreateServer(port);
		GetTree().NetworkPeer = peer;
		GetTree().SetMeta(Multiplayer_Constants.NETWORK_PEER,peer);

		   // Connect to MultiplayerAPI signals for player connections/disconnections
		
		GD.Print("Server started on port: " + port);

		GetTree().IsNetworkServer();
		GetTree().NetworkPeer = null;

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
				GD.Print("Invalid token for user.");
				peer.PutVar(""); // Optional: send an empty or error response
			}
		}
		else
		{
			GD.Print("User key missing in message.");
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
