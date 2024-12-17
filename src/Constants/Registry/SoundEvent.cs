using Godot;

public class SoundEvent
{
    public Vector2 Position { get; set; }
    protected float baseDbAtOneMeter { get; set; }
    public float Timestamp { get; set; } // Time at which event occurred, can help with "latest" logic

    public SoundEvent(Vector2 position, float baseDbAtOneMeter, float timestamp)
    {
        Position = position;

        this.baseDbAtOneMeter = baseDbAtOneMeter;
        Timestamp = timestamp;
    }
        // Approximate attenuation: dB decrease by 20*log10(distance)
    public float GetPerceivedDb(float distance)
    {
        if (distance <= 1f)
            return baseDbAtOneMeter;
        return baseDbAtOneMeter - 20f * Mathf.Log(distance);
    }
}
