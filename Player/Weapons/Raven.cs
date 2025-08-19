using Godot;
using System;
public partial class Raven : CharacterBody3D
{
    [Export] PlayerManager player;
    [Export] public float speed;
    [Export] public float maxDistance;
    public bool isAbility = false;

    public override void _Ready()
    {

        //GD.Print("Raven: Loaded successfully");
        if (player == null)
        {
            //GD.PrintErr("Raven: player not found");
        }
    }



    public override void _PhysicsProcess(double delta)
    {
        if (!isAbility)
        {
            Vector3 Yoffset = new Vector3(0, 1.5f, 0);
            Vector3 direction = GlobalPosition.DirectionTo(player.GlobalPosition + Yoffset);

            Velocity = (direction) * speed;
            MoveAndSlide();
        }
    }

    public Vector2 RavenDirection()
    {
        Vector2 dir = Vector2.Zero;
        if (Input.IsActionPressed("Up")) dir.Y += 1;
        if (Input.IsActionPressed("Down")) dir.Y -= 1;
        if (Input.IsActionPressed("Left")) dir.X -= 1;
        if (Input.IsActionPressed("Right")) dir.X += 1;

        if (dir != Vector2.Zero)
            return new Vector2(Input.GetAxis("Left", "Right"), Input.GetAxis("Up", "Down")).Normalized();

        return Vector2.Zero;

    }

    public void RavenLaunch()
    {
        if (player == null)
        {
           // GD.PrintErr("Raven: PlayerManager is null");
            return;
        }

        if (isAbility) return;

        isAbility = true;
        Vector2 direction = RavenDirection();
        var currentPosition = GlobalPosition;


        if (direction != Vector2.Zero)
        {

           //GD.Print("Raven: Launched in Direction:  " + direction);
        }
        else
        {
           // GD.Print("Raven: Direction is zero");
        }


    }


    public void RavenRecall() { }
    public void RavenTeleport() { }
    public void RavenMeleeAttack() { }

}
