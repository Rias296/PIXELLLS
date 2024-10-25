using Godot;

public class Global : Node
{
    public string PlayerName = "";
    public string SessionToken = "";

    // Singleton initialization (ready)
    public override void _Ready()
    {
        GD.Print("Autoload initialized");
    }

    // You can add more global functionality here, such as login/authentication methods.
    public void SetPlayerCredentials(string name, string token)
    {
        PlayerName = name;
        SessionToken = token;
    }
}