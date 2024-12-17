using Godot;
using System;
using System.Collections.Generic;

public class SoundManager : Node
{
    public static SoundManager Instance { get; private set; }

    private List<SoundEvent> soundEvents = new List<SoundEvent>();
    private float eventLifetime = 5f; 

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(float delta)
    {
        CleanUpOldEvents();
    }

    private void CleanUpOldEvents()
    {
        float currentTime = OS.GetTicksMsec() / 1000f;
        soundEvents.RemoveAll(se => (currentTime - se.Timestamp) > eventLifetime);
    }

    public void AddSoundEvent(Vector2 position, float baseDbAtOneMeter)
    {
        float currentTime = OS.GetTicksMsec() / 1000f;
        soundEvents.Add(new SoundEvent(position, baseDbAtOneMeter, currentTime));
    }
    public SoundEvent GetLatestSoundEventAboveThreshold(Vector2 referencePos, float hearingThresholdDb)
    {
        SoundEvent latestEvent = null;
        float latestTime = -Mathf.Inf;

        foreach (var se in soundEvents)
        {
            float dist = referencePos.DistanceTo(se.Position);
            // Calculate perceived dB at enemy’s location
            float perceivedDb = se.GetPerceivedDb(dist);
            if (perceivedDb >= hearingThresholdDb && se.Timestamp > latestTime)
            {
                latestEvent = se;
                latestTime = se.Timestamp;
            }
        }
        return latestEvent;
    }
}
