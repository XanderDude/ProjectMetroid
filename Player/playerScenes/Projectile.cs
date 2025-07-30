using Godot;
using System;
using System.Diagnostics;


public partial class Projectile : CharacterBody3D
{
    [Export] public float speed = 0.5f;
    private float _speed;
    [Export] private int damage = 30;
    [Export] private float lifetime = 2f;
    Vector3 velocity = Vector3.Zero;

    public override void _Ready()
    {
        _speed = speed;
        velocity.Y = Input.GetAxis("Down", "Up") * speed;
        if (velocity.Y != 0 && Input.GetAxis("Down", "Up") == 0)
        {
            speed = 0;
        }
        else velocity.X = speed;
    }


    public override void _PhysicsProcess(double delta)
    {
        velocity.X = speed;
        KinematicCollision3D col = MoveAndCollide(velocity, false, 0.01f, false, 1);
        if (col != null)
        {
            ProcessMode = ProcessModeEnum.Disabled;
        }
        else return;
        PhysicsBody3D obj;
        if (!IsInstanceValid((PhysicsBody3D)col.GetCollider(0))) return;
        obj = (PhysicsBody3D)col.GetCollider(0);
        if (obj != null && obj.GetCollisionLayerValue(4))
        {
            GD.Print("Enemy hit");
            GetNode<EnemyController>(obj.GetPath()).DamagedRecieved(damage);
        }
        QueueFree();
        
    }



}
