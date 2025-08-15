using Godot;
using System;

public partial class PlayerManager : CharacterBody3D
{
	private NodePath playerMeshPath = "%PlayerMesh";
	[Export] private GeometryInstance3D playerGeo;
	[Export] public PackedScene inventoryScene;
	
	public bool jumpQueued;
	public bool slideQueued;
	public bool slideBoost;
	private HUD hud;
	private int health = 100;
	[Export] private Material invulnMat;
	private bool gamePaused;
	
	
	public Item equippedRangedWeapon;
	
	
	public Node _inventory;
	private bool _inventoryInstantiated = false;
	
	public bool GamePaused
	{
		get { return gamePaused; }
		set
		{
			gamePaused = value;
			GetTree().Paused = value;
		}
	}
	
	public int Health
	{
		get { return health; }
		set
		{
			if (value < health) invulnTimer = _invulnTimer;
			health = value;
			if (health <= 0)
			{
				health = 0;
				hud?.PlayerDied();
				GamePaused = true;
			}
			hud?.UpdateHealthBar(Health);
		}
	}
	
	[Export] public bool canBeDamaged = true;
	[Export] private float invulnTimer = 1f;
	private float _invulnTimer;
	private MovementStateMachine _movementStateMachine;
	
	[Export]
	public MovementStateMachine StateMachine
	{
		get { return _movementStateMachine; }
		set
		{
			_movementStateMachine = value;
			StateMachine.Parent = this;
			StateMachine.ParentManager = this;
			StateMachine.parentMesh = GetNode<Node3D>(playerMeshPath);
		}
	}
	
	private AttackStateMachine _attackStateMachine;
	
	[Export]
	private AttackStateMachine AttackStateMachine
	{
		get { return _attackStateMachine; }
		set
		{
			_attackStateMachine = value;
			AttackStateMachine.Parent = this;
			AttackStateMachine.ParentManager = this;
			AttackStateMachine.parentMesh = GetNode<Node3D>(playerMeshPath);
		}
	}
	
	[Export] public Area3D slidingCollider;
	
	public override void _Ready()
	{
		invulnMat = ResourceLoader.Load<Material>("res://Environment/Materials/glowingMaterial.tres");
		hud = (HUD)GetTree().GetFirstNodeInGroup("hud");
		hud?.UpdateHealthBar(Health);
		_invulnTimer = invulnTimer;
		invulnTimer = 0;
		playerGeo.MaterialOverlay = null;
		
		InitializeInventory();
		
		var timer = GetTree().CreateTimer(0.1f);
		timer.Timeout += SyncWithInventory;
	}
	
	private void InitializeInventory()
	{
		if (inventoryScene != null && !_inventoryInstantiated)
		{
			_inventory = inventoryScene.Instantiate();
			_inventory.ProcessMode = Node.ProcessModeEnum.Always;
			_inventoryInstantiated = true;
			GD.Print("Inventory initialized");
		}
	}
	
	public void SyncWithInventory()
	{
		GD.Print("=== SyncWithInventory START ===");
		
		if (_inventory == null) 
		{
			GD.Print("Inventory is null, cannot sync");
			return;
		}
		
		GD.Print($"Inventory found: {_inventory.Name}");
		
		var crossbowsInventory = _inventory.FindChild("Crossbows", true, false) as Inventory;
		if (crossbowsInventory != null)
		{
			GD.Print("Found Crossbows inventory, checking for equipped items...");
			
			for (int i = 0; i < crossbowsInventory.inventorySize; i++)
			{
				var item = crossbowsInventory.GetInventoryItem(i);
				if (item != null)
				{
					GD.Print($"Slot {i}: {item.Name}, Category: {item.Category}, Equipped: {item.Equipped}");
				}
			}
			
			var equippedCrossbow = crossbowsInventory.GetEquippedItemInCategory(ItemCategory.Ranged);
			if (equippedCrossbow != null)
			{
				equippedRangedWeapon = equippedCrossbow;
				GD.Print($"SUCCESS: Synced equipped weapon: {equippedRangedWeapon.Name}");
			}
			else
			{
				GD.Print("No equipped crossbow found in category Ranged");
				equippedRangedWeapon = null;
			}
		}
		else
		{
			GD.Print("Crossbows inventory not found");
		}
		
		GD.Print("=== SyncWithInventory END ===");
	}
	
	public Node GetInventory()
	{
		return _inventory;
	}
	
	public bool HasRangedWeapon()
	{
		return equippedRangedWeapon != null && equippedRangedWeapon.Category == ItemCategory.Ranged;
	}
	
	public Item GetEquippedRangedWeapon()
	{
		return equippedRangedWeapon;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (invulnTimer > 0)
		{
			playerGeo.MaterialOverlay = invulnMat;
			canBeDamaged = false;
			invulnTimer -= (float)delta;
		}
		else
		{
			canBeDamaged = true;
			playerGeo.MaterialOverlay = null;
		}
	}
}
