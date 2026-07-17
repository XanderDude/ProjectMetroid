using Godot;
using System;

public partial class SimpleSpike : Node3D
{

	[Export] private int damage = 5;
	private PlayerManager player;

	private bool playerInRange = false;

	// Called when the node enters the scene tree for the first time.


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{ 
        if (playerInRange && GameFriend.gameinstance.player.canBeDamaged) 
		{
			GD.Print("Player is touching spike");
			GameFriend.gameinstance.player.Health -= damage;
		}
	}

	public void OnLeaveTrapTrigger(Node3D node)
    {
        playerInRange = false;
    }

    public void OnTrapTriggered(Node3D node)
    {
		//AssignPlayer();
        playerInRange = true;
    }

}
