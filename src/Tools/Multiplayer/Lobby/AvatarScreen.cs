using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AvatarScreen: Node{

	private TextureRect textureRect;
	private Label label;
	public override void _Ready()
	{
		
	}

	public void RequestAuthentication(PacketPeerUDP packet){
		var request = new JObject
		{
			["get_authentication_token"] = true,
			["user"] = AuthenticationCredentials.user,
			["token"] = AuthenticationCredentials.SessionToken
		};

		packet.PutVar(request.ToString());

		while(packet.Wait() == Error.Ok){
			var data = JObject.Parse((string)packet.GetVar());


			if ((string)data == AuthenticationCredentials.SessionToken){
				RequestAvatar(packet);
				break;
			}
		}    


	}


	private void RequestAvatar(PacketPeerUDP packet)
	{
		var request = new JObject
		{
			["get_avatar"] = true,
			["token"] = AuthenticationCredentials.SessionToken,
			["user"] = AuthenticationCredentials.user
		};

		packet.PutVar(JsonConvert.SerializeObject(request));

		while (packet.Wait() == Error.Ok)
		{
			string responseJSON = packet.GetVar() as string;
			if (!string.IsNullOrEmpty(responseJSON))
			{
				var data = JObject.Parse(responseJSON);

				if (data.ContainsKey("avatar"))
				{
					var texture = (Texture)GD.Load((string)data["avatar"]);
					textureRect.Texture = texture;

					// Display name
					label.Text = (string)data["name"];
					break;
				}
			}
		}
	}


}
