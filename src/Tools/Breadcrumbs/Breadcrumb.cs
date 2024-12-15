using Godot;
using System;

public class Breadcrumb : Node2D
{
    public float LifeTime = 10f; // Disappears after 10 seconds
    private float timer = 0f;

    public override void _Process(float delta)
    {
        timer += delta;
        if (timer > LifeTime)
            QueueFree();
    }


// Player code snippet (not full):
// In player's _PhysicsProcess(delta) or some timed method:
private float breadcrumbTimer = 0f;
public override void _PhysicsProcess(float delta)
{
    breadcrumbTimer += delta;
    if (breadcrumbTimer > 1f) // drop every second, for example
    {
        var crumb = new Breadcrumb();
        crumb.GlobalPosition = this.GlobalPosition;
        GetParent().AddChild(crumb);
        breadcrumbTimer = 0f;
    }
}
}