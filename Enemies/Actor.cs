using Godot;
using System;

public partial class Actor : CharacterBody3D
{
    [Export] public int health = 100;
	[Export] public int itemdropamount = 1;
	[Export] public int dropRate1 = 100, dropRate2 = 0;
	[Export] public float runspeed { get; set; } = 2.5f;
	[Export] public float walkspeed { get; set; } = 1.0f;

    public Vector3 walkspeedVector => new Vector3(walkspeed, 0, 0);
    public Vector3 runspeedVector => new Vector3(runspeed, 0, 0);
	[Export] public float idle { get; set; } = 0f;
	[Export] public float acceleration { get; set; } = 0.1f;
	[Export] public float jumpmaxheight { get; set; } = 10.0f;
	[Export] public int  damagedealt { get; set; } = 15;
	[Export] public float damagecooldown { get; set; } = 0.5f;
	[Export] public float gravity { get; set; } = -9.8f;
	[Export] public float attackspeed { get; set; } = 1.0f;
	[Export] public float attackknockback { get; set; } = 5;
	[Export] public float damageovertime { get; set; } = 0f;
	[Export] public float detectionrange { get; set; } = 10f;

    public float IDLE = 0.0f;
}
