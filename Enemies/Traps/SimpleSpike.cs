using Godot;
using System;

public partial class SimpleSpike : Node3D
{

	[Export] private int damage = 5;
	[Export] private float trapKnockback = 2f;
	private PlayerManager player;

	private bool playerInRange = false;

	// Called when the node enters the scene tree for the first time.


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{ 
        if (playerInRange && GameFriend.gameinstance.player.canBeDamaged) 
		{
			//GD.Print("Player is touching spike");

			DamagePlayer(GameFriend.gameinstance.player);
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

	public void DamagePlayer(PlayerManager p)
	{
		float knockDir = Mathf.Sign(p.GlobalPosition.X - GlobalPosition.X);
		p.knockbackVelocity = new Vector3(knockDir, 1f, 0f).Normalized() * trapKnockback * 3f;
		p.StateMachine.TransitionTo("knockbackState");
		p.Health -= damage;
	}

}
