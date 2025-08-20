using Godot;
using System;
public partial class Raven : CharacterBody3D
{
    [Export] PlayerManager player;
    [Export] public float speed;
    [Export] public float maxDistance;

    public Vector3 direction = Vector3.Zero;
    public Vector3 targetPosition = Vector3.Zero;
    public bool isInAction = false;
    public bool isLaunched = false;
    public bool canTeleport = false;

   
    public override void _Ready()
    {

        //GD.Print("Raven: Loaded successfully");
        if (player == null)
        {
            GD.PrintErr("Raven: player not found");
        }
    }



    public override void _PhysicsProcess(double delta)
    {
        if (!isInAction)
        {
            Vector3 Yoffset = new Vector3(0, 1.5f, 0);
            Vector3 direction = GlobalPosition.DirectionTo(player.GlobalPosition + Yoffset);

            Velocity = (direction) * speed;

             if (Mathf.Abs(GlobalPosition.DistanceTo(player.GlobalPosition + Yoffset)) < 0.1f) { isLaunched = false; }
        }

        if (player.IsOnFloor()) { canTeleport = true; } 

        if (Input.IsActionPressed("RavenSpecial") && !isLaunched)
        {
            RavenLaunch();
        }
        if (Input.IsActionJustReleased("RavenSpecial"))
        {
            isLaunched = true;
            Velocity = Vector3.Zero;
        }

        if (Input.IsActionJustPressed("RavenSpecial") && isLaunched)
        {
            isInAction = false;
           
        }

        if (Input.IsActionJustPressed("RavenSlash") && isLaunched && canTeleport)
        {
            player.GlobalPosition = GlobalPosition;
            canTeleport = false;
            isInAction = false;
            player.Velocity = Vector3.Zero;

            
    
        }

            MoveAndSlide();
    }

    public Vector3 RavenDirection()
    {
        Vector3 dir = Vector3.Zero;
        if (Input.IsActionPressed("Up")) dir.Y += 1;
        if (Input.IsActionPressed("Down")) dir.Y -= 1;
        if (Input.IsActionPressed("Left")) dir.X -= 1;
        if (Input.IsActionPressed("Right")) dir.X += 1;

        if (dir != Vector3.Zero)
            return new Vector3(Input.GetAxis("Left", "Right"), Input.GetAxis("Down", "Up"), 0).Normalized();

        return Vector3.Zero;

    }
    



    public void RavenLaunch()
    {
        

        if (player == null)
        {
            GD.PrintErr("Raven: PlayerManager is null");
            return;
        }

       
        if (isInAction == false)
        {
            direction = RavenDirection();
            targetPosition = GlobalPosition + (direction * maxDistance);
        }

        isInAction = true;
        if (direction != Vector3.Zero)
        {

            GD.Print("Raven: Launched in Direction:  " + direction);
            Velocity = direction * speed;

            if (Mathf.Abs(GlobalPosition.DistanceTo(targetPosition)) < 0.1f)
            {
                GD.Print("Raven: Reached target position");
                GD.Print("Raven: targetPosition: " + targetPosition);
                Velocity = Vector3.Zero;
            }
            else
            {
                GD.Print("Raven: Moving towards target position");
                GD.Print("Raven: Current Position: " + GlobalPosition);
                GD.Print("Raven: targetPosition: " + targetPosition);
            }


        }




    }

    public void RavenRecall()
    {
        
    }

   
    public void RavenTeleport() { }
    public void RavenMeleeAttack() { }

}
