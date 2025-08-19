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
	
	private Vector3 GetShootDirection()
	{
		Vector3 direction = Vector3.Zero;
		
		bool up = Input.IsActionPressed("Up");
		bool down = Input.IsActionPressed("Down");
		bool left = Input.IsActionPressed("Left");
		bool right = Input.IsActionPressed("Right");
		
		if (up && left)
			direction = new Vector3(-1, 1, 0).Normalized();
		else if (up && right)
			direction = new Vector3(1, 1, 0).Normalized();
		else if (down && left)
			direction = new Vector3(-1, -1, 0).Normalized();
		else if (down && right)
			direction = new Vector3(1, -1, 0).Normalized();
		else if (up)
			direction = new Vector3(0, 1, 0);
		else if (down)
			direction = new Vector3(0, -1, 0);
		else if (left)
			direction = new Vector3(-1, 0, 0);
		else if (right)
			direction = new Vector3(1, 0, 0);
		else
		{
			var mesh = playerManager?.GetNode<Node3D>("PlayerMesh");
			if (mesh != null)
			{
				direction = mesh.Transform.Basis.Z.Normalized();
			}
			else
			{
				direction = new Vector3(1, 0, 0);
			}
		}
		
		return direction;
	}
	
	private void ShootArrow(bool isBombArrow)
{
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
	
	Vector3 shootDirection = GetShootDirection();
	arrow.LinearVelocity = shootDirection * arrowSpeed;
	
	if (arrow.LinearVelocity != Vector3.Zero)
	{
		arrow.LookAt(arrow.GlobalPosition + arrow.LinearVelocity, Vector3.Up);
		arrow.RotateObjectLocal(Vector3.Up, Mathf.Pi / 2);
	}
	
	arrow.Scale *= 3f;
	arrow.GravityScale = 0.3f;
	
	laserSound = GetNode<Godot.AudioStreamPlayer>("../../shootingsound");
	laserSound.Play();
}
}
