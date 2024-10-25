using Godot;

public class MainMenu : Control
{
	private Button _clientButton;
	private Button _serverButton;

	public override void _Ready()
	{
		_clientButton = GetNode<Godot.Button>("ClientButton");
		_serverButton = GetNode<Godot.Button>("ServerButton");
		GD.Print(_clientButton.GetType());

		if (_clientButton ==null){
			GD.PrintErr("ClientButton missing or not found");
		}

		if (_serverButton == null){
			GD.PrintErr("ServerButton not found");
		}
		_clientButton.Connect("pressed", this, nameof(onClientButtonPressed));
		_serverButton.Connect("pressed", this, nameof(onClientButtonPressed));
	}

	private void onClientButtonPressed(){
		GetTree().ChangeScene("res://Scenes/Main Menu/ClientButton.tscn");

	}
	  private void OnServerButtonPressed()
	{
		// Change to the Server scene
		GetTree().ChangeScene("res://Scenes/Main Menu/ServerButton.tscn");
	}
}
