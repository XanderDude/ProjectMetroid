using Godot;
using System;
public partial class BreakableBlock : Node3D
{
    [Export] public int health = 2;
    private StaticBody3D solidBody;
    private Area3D detectionArea;

    private MeshInstance3D breaking0;
    private MeshInstance3D breaking1;

    public override void _Ready()
    {
        solidBody = GetNode<StaticBody3D>("StaticBody3D");
        detectionArea = GetNode<Area3D>("Area3D");
        breaking0 = GetNode<MeshInstance3D>("StaticBody3D/Breaking0");
        breaking1 = GetNode<MeshInstance3D>("StaticBody3D/Breaking1");


        detectionArea.BodyEntered += OnBodyEntered;
    }
    
    private void OnBodyEntered(Node3D body)
    {
        GD.Print("Something entered: " + body.Name);
        
        if (body is BombArrowProjectile)
        {
            health -= 1;
            GD.Print("Hit by projectile! Health now: " + health);
            
            if (health == 1)
            {
                breaking1.Visible = true;
            }

            if (health <= 0)
            {
                GD.Print("Block destroyed!");
                QueueFree(); 
            }
        }
    }
}