using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class FlickerLight : OmniLight3D
{
    public float _FlickerSpeed = .3f; //default total time the light stays dimmed
    private float flickerSpeed;
    public float _FlickerDelay = 2f; //default time between dims
    private float flickerDelay;
    private OmniLight3D light { get; set; }
    private float defaultEnergy;
    private float dimTimer = 0; //for timing the current dim
    private float delayTimer = 0; //for timing between each dim

    public override void _Ready()
    {
        light = this;
        defaultEnergy = light.LightEnergy;
        flickerSpeed = _FlickerSpeed;
        flickerDelay = _FlickerDelay;
    }

    public override void _Process(double delta)
    {
        if (delayTimer > -1)
        {
            delayTimer += (float)delta;
        }
        if (dimTimer > -1)
        {
            dimTimer += (float)delta;
        }
        //GD.Print("Timer: " + delayTimer);

        if (dimTimer >= flickerSpeed) //flicker is over
        {
            GD.Print("Stop Dimming");
            dimTimer = -1; //stop dim timer
            delayTimer = 0; //start timer between flickers
            light.LightEnergy = defaultEnergy;//set light back to default
            delayTimer = (float)(new Random().NextDouble() * _FlickerDelay);
        }
        else if (delayTimer >= flickerDelay) //start flicker
        {
            GD.Print("Dimming");
            delayTimer = -1; //stop timer between flickers
            dimTimer = 0; //dim timer starts
            light.LightEnergy = defaultEnergy - .1f; //dim light (begin flicker)
            dimTimer = (float)(new Random().NextDouble() * _FlickerSpeed);
        }
    }
}
