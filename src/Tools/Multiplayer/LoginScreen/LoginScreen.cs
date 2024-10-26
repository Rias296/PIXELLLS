using Godot;
using System;

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

        loginButton.Connect("pressed", this, nameof(OnLoginButtonPressed));
    }

    private void OnLoginButtonPressed(){
        send_credentials();
    }

    public void send_credentials(){
        //create message with user credentials
        var message = new Godot.Collections.Dictionary
        {
            { "authenticate_credentials", new Godot.Collections.Dictionary
            {
                {"user", userLineEdit.Text},
                {"password", passwordLineEdit.Text}
            }}

        };

        //Create UDP Packet
        PacketPeerUDP packet = new PacketPeerUDP();
        Error err = packet.ConnectToHost(Multiplayer_Constants.IP_ADDRESS,Multiplayer_Constants.PORT);
        

    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta)
 {
     
 }
}
