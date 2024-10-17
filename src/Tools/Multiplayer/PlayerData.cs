
using Godot;
public class PlayerData
{
    public int PeerId { get; }
    public Vector3 Position { get; set; }

    public PlayerData(int peerId)
    {
        PeerId = peerId;
        Position = Vector3.Zero;
    }
}