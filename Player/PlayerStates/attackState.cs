using Godot;
using System;

public partial class attackState : State
{
	[Export] public PackedScene arrowScene;
	[Export] public PackedScene bombArrowScene;
	[Export] public float arrowSpeed = 20.0f;
	[Export] public float shootCooldown = 0.1f;
	[Export] public AudioStream shootSound; 
	[Export] Godot.AudioStreamPlayer laserSound;
	private Node3D crossbowMesh;
	private PlayerManager playerManager;
	
	public override void _Ready() 
	{
		Node current = this;
		while (current != null && !(current is PlayerManager))
		{
			current = current.GetParent();
		}
		
		if (current is PlayerManager)
		{
			playerManager = current as PlayerManager;
		}
		
		if (playerManager != null)
		{
			crossbowMesh = playerManager.GetNode<Node3D>("PlayerMesh/Skeleton3D/Crossbow");
		}
	}
	
	
	public override void Enter()
	{
		if (Input.IsActionPressed("Shoot") && CanShoot())
		{
			ShootArrow(false);

		}

		if (Input.IsActionPressed("SpecialShoot") && CanShootBomb())
		{
			ShootArrow(true);

		}
	}
	
	public override void Exit()
	{
	}

	
	public override void HandleInput(InputEvent @event) 
	{
		if (@event.IsActionReleased("Shoot") && !Input.IsActionPressed("SpecialShoot")) 
		{
			asm.TransitionTo("noattackState");
		}
		
		if (@event.IsActionReleased("SpecialShoot") && !Input.IsActionPressed("Shoot")) 
		{
			asm.TransitionTo("noattackState");
		}
	}
	
	private bool CanShoot()
	{
		return playerManager != null && 
			   playerManager.HasRangedWeapon() && 
			   arrowScene != null;
	}
	
	private bool CanShootBomb()
	{
		if (!CanShoot() || bombArrowScene == null)
		{
			return false;
		}
		
		var arrowsInventory = playerManager.GetArrowsInventory();
		if (arrowsInventory == null)
		{
			return false;
		}
		
		return HasBombArrows(arrowsInventory);
	}
	
	private bool HasBombArrows(Inventory arrowsInventory)
	{
		for (int i = 0; i < arrowsInventory.inventorySize; i++)
		{
			var item = arrowsInventory.GetInventoryItem(i);
			if (item != null && item.ID == 6 && item.Qty > 0)
			{
				return true;
			}
		}
		return false;
	}
	
	private void ConsumeBombArrow()
{
	var arrowsInventory = playerManager.GetArrowsInventory();
	if (arrowsInventory == null) return;
	
	for (int i = 0; i < arrowsInventory.inventorySize; i++)
	{
		var item = arrowsInventory.GetInventoryItem(i);
		if (item != null && item.ID == 6 && item.Qty > 0)
		{
			item.Qty--;
			
			if (item.Qty <= 0)
			{
				arrowsInventory.RemoveInventoryItem(i);
			}
			else
			{
			
				if (!item.IsInfinite && item.MaxQty > 1)
				{
					arrowsInventory.SetItemText(i, item.Qty.ToString());
				}
			}
			return;
		}
	}
}
	
	private Vector2 GetShootDirection()
	{		
		Vector2 direction = new Vector2(Input.GetAxis("Left", "Right"), Input.GetAxis("Down", "Up"));

		if (direction == Vector2.Zero) direction = new(Math.Sign(parentMesh.RotationDegrees.Y), direction.Y);
		
		return direction.Normalized();
	}
	
	private void ShootArrow(bool isBombArrow)
	{
		parentMesh.GetNode<PlayerAnimationHandler>(parentMesh.GetPath()).Shoot();
		PackedScene projectileScene = isBombArrow ? bombArrowScene : arrowScene;
		
		if (projectileScene == null || crossbowMesh == null)
		{
			return;
		}
		
		if (isBombArrow)
		{
			ConsumeBombArrow();
		}
		
		var arrow = projectileScene.Instantiate() as RigidBody3D;
		if (arrow == null)
		{
			return;
		}

		GetTree().CurrentScene.AddChild(arrow);
		arrow.GlobalPosition = crossbowMesh.GlobalPosition;

		Vector2 shootDirection = GetShootDirection();
		
		float dot = shootDirection.Dot(new Vector2(Mathf.Abs(shootDirection.X), 0));
		float rotation = (shootDirection.Y >= 0)
		? (1f - dot) / 4f 
		: (dot + 3f) / 4f;

		if (float.IsNaN(rotation)) rotation = parentMesh.RotationDegrees.Y - 90;
		else rotation *= 360f;

		arrow.RotationDegrees = new(0, 0, rotation);
		arrow.LinearVelocity = new(shootDirection.X * arrowSpeed, shootDirection.Y * arrowSpeed, 0);
		GD.Print(arrow.LinearVelocity);

		arrow.Scale *= 3f;
		arrow.GravityScale = 0.3f;
		
		laserSound = GetNode<Godot.AudioStreamPlayer>("../../shootingsound");
		laserSound.Play();
	}
}
