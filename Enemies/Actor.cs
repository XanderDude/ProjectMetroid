using Godot;
using System;

public partial class Actor : CharacterBody3D
{
    [ExportGroup("Stats")]
    [Export] public int health = 100;
    [Export] public int numItemDrops = 1;
    [Export] public float maxRunSpeed { get; set; } = 2.5f;
	[Export] public float maxWalkSpeed { get; set; } = 1.0f;
	[Export] public float IDLE { get; set; } = 0f;
    [Export] public float acceleration { get; set; } = 0.1f;
    [Export] public float maxJumpHeight { get; set; } = 10.0f;
    [Export] public int  damagedealt { get; set; } = 15; 
    [Export] public float damagecooldown { get; set; } = 0.5f;
    [Export] public float gravity { get; set; } = -9.8f;
    [Export] public float attackspeed { get; set; } = 1.0f;
	[Export] public float attackknockback { get; set; } = 5;
	[Export] public float damageovertime { get; set; } = 0f;
	[Export] public float detectionrange { get; set; } = 10f;
	[Export] public bool isFlying = false; //NEED TO DO SOMETHING WITH THIS????

    [ExportGroup("Node References")]
    [Export] public Node3D mesh;

    
    
}
