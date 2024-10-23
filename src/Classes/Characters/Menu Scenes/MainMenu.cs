using Godot;

public class MainMenu : Control
{
    private Button _clientButton;
    private Button _serverButton;

    public override void _Ready()
    {
        _clientButton = GetNode<Button>("ClientButton");
        _serverButton = GetNode<Button>("ServerButton");

        _clientButton.Connect("pressed", this, nameof(onClientButtonPressed));
        _serverButton.Connect("pressed", this, nameof(onClientButtonPressed));
    }

    private void onClientButtonPressed(){
        GetTree().ChangeScene("res://Client.tscn");

    }
      private void OnServerButtonPressed()
    {
        // Change to the Server scene
        GetTree().ChangeScene("res://Server.tscn");
    }
}