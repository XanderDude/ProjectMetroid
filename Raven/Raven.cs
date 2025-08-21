using Godot;
using System;
public partial class Raven : CharacterBody3D
{
    [Export] public PlayerManager player = null;
    [Export] public float speed = 0.0f;
    [Export] public float maxDistance = 0.0f;

    public Vector3 direction = Vector3.Zero;
    public Vector3 targetPosition = Vector3.Zero;

    public Vector3 Yoffset = new Vector3(0, 1.5f, 0);
    public bool isInAction = false;

    public bool isOnPlayer = false;
    public bool isLaunching = false;
    public bool canTeleport = true;


    public override void _Ready()
    {
        if (player == null)
        {
            GD.PrintErr("Raven: player not found");
        }

        this.CollisionLayer = 1 << 4;
        this.CollisionMask = (1 << 0) | (1 << 1);
    }

    public override void _PhysicsProcess(double delta)
    {

        MoveAndSlide();
    }


   

}
