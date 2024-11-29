using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;


public class LoginScreen : Control

{
	private Label errorLabel;
	private Button loginButton;
	private LineEdit userLineEdit;
	private LineEdit passwordLineEdit;
	public override void _Ready()
	{
		userLineEdit = GetNode<LineEdit>("UserLineEdit");
		passwordLineEdit = GetNode<LineEdit>("PasswordLineEdit");
		errorLabel = GetNode<Label>("ErrorLabel");
		loginButton = GetNode<Button>("LoginButton");

		errorLabel.Text = "Insert Username and Password";
		userLineEdit.GrabFocus();
		
		

	}

	private void OnPasswordLineEditEntered(String new_text)
	{
	
		if (!string.IsNullOrEmpty(userLineEdit.Text))
		{
			Send_credentials();
		}
		else
		{
			errorLabel.Text = "Insert password";
			userLineEdit.GrabFocus();
		}
	}


	private void OnUserLineEditEntered(String new_text)
	{
		if (!string.IsNullOrEmpty(passwordLineEdit.Text))
		{
			GD.Print("Send Credentials made");
			Send_credentials();
		}
		else
		{
			errorLabel.Text = "Insert username";
			passwordLineEdit.GrabFocus();
		}
	}
	private void OnLoginButtonPressed()
	{
	if (userLineEdit.Text == ""){
			errorLabel.Text = "Insert Username";
			userLineEdit.GrabFocus();
			
		}
	if (passwordLineEdit.Text == ""){
		errorLabel.Text = "Insert Password";
		passwordLineEdit.GrabFocus();
	}else{
		Send_credentials();
	}
		
	}
	public async void Send_credentials()
	{
		// Creating a message with user credentials


		var message = new JObject
		{
			["authenticate_credentials"] = new JObject
			{
				["user"] = userLineEdit.Text,
				["password"] = passwordLineEdit.Text
			}
		};

		// Creating and connecting PacketPeerUDP
		GD.Print("packet class made");
		PacketPeerUDP packet = new PacketPeerUDP();
		Error connectResult = packet.ConnectToHost(Multiplayer_Constants.IP_ADDRESS, Multiplayer_Constants.PORT);

		if (connectResult != Error.Ok)
		{
			errorLabel.Text = "Failed to connect to the server.";
			GD.Print("Issue with host connection");
			return;
		}

		packet.PutVar(message.ToString());
		GD.Print("before while loop");
		//WAIT FOR SERVER response
		for (int i =0;i<5;i++){
			if (packet.GetAvailablePacketCount() > 0){  //No response from this. Possibly server not even connected
			GD.Print("responseJSON made");
			string responseJson = packet.GetVar() as string;
			var response = JObject.Parse(responseJson);

			//check if response has a token
			if (response.ContainsKey("token"))
			{
				GD.Print("have token");

				//store token and user credentials
				AuthenticationCredentials.user = (string)message["authenticate_credentials"]["user"];
				AuthenticationCredentials.SessionToken = (string)response["token"];

				errorLabel.Text = "Logged in!";

				GetTree().ChangeScene("res://Scenes/Main Menu/AvatarScreen.tscn");
				return;
			}
			else
			{
				errorLabel.Text = "login failed, check credentials";
				
			}
		 } 
		 // Wait asynchronously for the response
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");  // Allow some delay for response
		}
		 errorLabel.Text = "No response from server";	//this gets activated

		 

	}

}







