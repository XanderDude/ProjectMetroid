using Godot;
using System;

public partial class AttackHandler : Node
{
    [Export] private PackedScene _projectile;
    private CharacterBody3D parent;
    private Node3D mesh;
    [Export] private Node3D spawnPoint;

    public override void _Ready()
    {
        _projectile = ResourceLoader.Load<PackedScene>(_projectile.ResourcePath);
        parent = GetParent<CharacterBody3D>();
        mesh = GetNode<Node3D>("%PlayerMesh");
        //spawnPoint = GetNode<Node3D>("projectileSpawn");
    }


    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("Shoot"))
        {
            GD.Print("Spawning");
            var newProj = _projectile.Instantiate();
            parent.GetParent().AddChild(newProj);
            newProj.GetNode<Node3D>(newProj.GetPath()).GlobalTransform = mesh.GlobalTransform;
            newProj.GetNode<Node3D>(newProj.GetPath()).GlobalPosition = spawnPoint.GlobalPosition;
            newProj.GetNode<Projectile>(newProj.GetPath()).speed *= MathF.Sign(mesh.RotationDegrees.Y);

        }

    } 
}
