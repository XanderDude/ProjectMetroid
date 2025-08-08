using Godot;
using System;

public partial class ArrowProjectile : CharacterBody3D
{
	[Export] public float speed = 25.0f;
	[Export] public float maxDistance = 50.0f;
	[Export] public Vector3 customScale = Vector3.One * 3.0f;
	[Export] public bool useGravity = false;
	
	public override void _Ready()
	{
		// apply custom scale when arrow spawns
		Scale = customScale;
	}
}
