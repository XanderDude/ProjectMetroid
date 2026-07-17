using Godot;
using System;

public partial class SimpleSpike : Node3D
{

	[Export] private int damage = 5;
	[Export] private float trapKnockback = 2f;

	private PlayerManager playerInRange;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (playerInRange != null && playerInRange.canBeDamaged)
			DamagePlayer(playerInRange);
	}

	public void OnLeaveTrapTrigger(Node3D node)
    {
        if (node is PlayerManager p && p == playerInRange)
            playerInRange = null;
    }

    public void OnTrapTriggered(Node3D node)
    {
        if (node is PlayerManager p)
            playerInRange = p;
    }

	public void DamagePlayer(PlayerManager p)
	{
		float knockDir = Mathf.Sign(p.GlobalPosition.X - GlobalPosition.X);
		p.knockbackVelocity = new Vector3(knockDir, 1f, 0f).Normalized() * trapKnockback * 3f;
		p.psm.transition_to("knockbackState");
		p.health -= damage;
	}

}
