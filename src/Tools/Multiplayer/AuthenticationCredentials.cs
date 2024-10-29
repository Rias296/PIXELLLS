using Godot;

public class AuthenticationCredentials : Node
{
    public static string user = "";
    public static string SessionToken = "";

    // Singleton initialization (ready)
    public override void _Ready()
    {
        GD.Print("Autoload initialized");
        
    }

    
}