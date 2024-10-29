using Godot;

public class MainMenu : Control
{

	[Export] private PackedScene serverScene;
	[Export] private PackedScene clientScene;
	private Button _clientButton;
	private Button _serverButton;

	public override void _Ready()
	{
		_clientButton = GetNode<Godot.Button>("ClientButton");
		_serverButton = GetNode<Godot.Button>("ServerButton");
		GD.Print(_clientButton.GetType());

		_serverButton.GrabFocus();

		if (_clientButton ==null){
			GD.PrintErr("ClientButton missing or not found");
		}

		if (_serverButton == null){
			GD.PrintErr("ServerButton not found");
		}
		//_clientButton.Connect("pressed", this, nameof(OnClientButtonPressed));
		//_serverButton.Connect("pressed", this, nameof(OnServerButtonPressed));
	}

	private void OnClientButtonPressed()
{
	GetTree().ChangeScene("res://Scenes/Main Menu/Client.tscn");
}


	private void OnServerButtonPressed()
{
	
	// Change to the Server scene
		GetTree().ChangeScene("res://Scenes/Main Menu/Server.tscn");
}

}




