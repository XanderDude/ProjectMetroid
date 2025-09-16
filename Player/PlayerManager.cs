using Godot;
using System;

public partial class PlayerManager : CharacterBody3D
{
	[Export] private NodePath playerMeshPath = "%PlayerMesh";
	
	public bool jumpQueued;
	public bool slideQueued;
	public bool slideBoost;
	public int health = 1;

	public bool isDead = false;
	[Export] private ShaderMaterial invulnMat;
	
	public Item equippedRangedWeapon;
	
	// Reference to the inventory node in the scene tree
	private Node _inventory;
	private Inventory _crossbowsInventory;
	private Inventory _arrowsInventory;
	public Inventory _meleesInventory;
	
	
	
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
				
				
			}
		
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
		_invulnTimer = invulnTimer;
		invulnTimer = 0;
		invulnMat?.SetShaderParameter("alpha", 0f);
		
		InitializeInventory();
		
		// Small delay to ensure all nodes are ready
		var timer = GetTree().CreateTimer(0.1f);
		timer.Timeout += SyncWithInventory;
	}
	
	private void InitializeInventory()
	{
		// Get the inventory that's already a child of this player
		_inventory = GetNode("Inventory");
		
		if (_inventory != null)
		{
			var canvasLayer = _inventory.GetNode("CanvasLayer");
			if (canvasLayer != null)
			{
				_crossbowsInventory = canvasLayer.GetNode("Crossbows") as Inventory;
				_arrowsInventory = canvasLayer.GetNode("Arrows") as Inventory;
				_meleesInventory = canvasLayer.GetNode("Melees") as Inventory;
			}
		}
		
		// Start with inventory hidden
		HideInventory();
		
		/*GD.Print($"Inventory references initialized:");
		GD.Print($"  Crossbows: {_crossbowsInventory?.Name ?? "null"}");
		GD.Print($"  Arrows: {_arrowsInventory?.Name ?? "null"}");
		GD.Print($"  Melees: {_meleesInventory?.Name ?? "null"}");
		*/
	}
	
	public void SyncWithInventory()
	{
		//GD.Print("=== SyncWithInventory START ===");
		
		if (_crossbowsInventory == null) 
		{
			//GD.Print("Crossbows inventory is null, cannot sync");
			return;
		}
		
		//GD.Print($"Crossbows inventory found: {_crossbowsInventory.Name}");
		
		// Check for equipped items in crossbows inventory
		bool foundAnyItems = false;
		for (int i = 0; i < _crossbowsInventory.inventorySize; i++)
		{
			var item = _crossbowsInventory.GetInventoryItem(i);
			if (item != null)
			{
				foundAnyItems = true;
				//GD.Print($"Slot {i}: {item.Name}, Category: {item.Category}, Equipped: {item.Equipped}");
			}
		}
		
		if (!foundAnyItems)
		{
			//GD.Print("No items found in any slots!");
		}
		
		// Check specifically for equipped ranged items
		//GD.Print("Checking for equipped items using GetEquippedItemInCategory...");
		var equippedCrossbow = _crossbowsInventory.GetEquippedItemInCategory(ItemCategory.Ranged);
		if (equippedCrossbow != null)
		{
			equippedRangedWeapon = equippedCrossbow;
			//GD.Print($"SUCCESS: Synced equipped weapon: {equippedRangedWeapon.Name}");
		}
		else
		{
			//GD.Print("No equipped crossbow found in category Ranged");
			
			// Let's also check if any items are equipped at all
			bool foundEquippedItem = false;
			for (int i = 0; i < _crossbowsInventory.inventorySize; i++)
			{
				var item = _crossbowsInventory.GetInventoryItem(i);
				if (item != null && item.Equipped)
				{
					foundEquippedItem = true;
					//GD.Print($"Found equipped item in slot {i}: {item.Name} (Category: {item.Category})");
				}
			}
			
			if (!foundEquippedItem)
			{
				//GD.Print("No equipped items found at all!");
			}
			
			equippedRangedWeapon = null;
		}
		
		//GD.Print("=== SyncWithInventory END ===");
	}
	
	public Inventory GetCrossbowsInventory()
	{
		return _crossbowsInventory;
	}
	
	public Inventory GetArrowsInventory()
	{
		return _arrowsInventory;
	}
	
	public Inventory GetMeleesInventory()
	{
		return _meleesInventory;
	}
	
	// Keep this method for backward compatibility with existing code
	public Node GetInventory()
	{
		return _inventory;
	}
	
	public bool HasRangedWeapon()
	{
		bool hasWeapon = equippedRangedWeapon != null && equippedRangedWeapon.Category == ItemCategory.Ranged;
		//GD.Print($"HasRangedWeapon called: {hasWeapon} (weapon: {equippedRangedWeapon?.Name ?? "null"})");
		return hasWeapon;
	}
	
	public Item GetEquippedRangedWeapon()
	{
		//GD.Print($"GetEquippedRangedWeapon called: {equippedRangedWeapon?.Name ?? "null"}");
		return equippedRangedWeapon;
	}
	
	// Method to add items to appropriate inventory
	public bool AddItemToInventory(Item item)
	{
		switch (item.Category)
		{
			case ItemCategory.Ranged:
				return _crossbowsInventory?.AddInventoryItem(item) ?? false;
			case ItemCategory.Melee:
				return _meleesInventory?.AddInventoryItem(item) ?? false;
			case ItemCategory.Consumable:
				// Assuming arrows are consumables, adjust as needed
				return _arrowsInventory?.AddInventoryItem(item) ?? false;
			default:
				GD.PrintErr($"Unknown item category: {item.Category}");
				return false;
		}
	}
	
	// Helper methods to show/hide inventory
	public void ShowInventory()
	{
		if (_inventory != null)
		{
			var canvasLayer = _inventory.FindChild("CanvasLayer", false, false) as CanvasLayer;
			if (canvasLayer != null)
			{
				canvasLayer.Visible = true;
				canvasLayer.ProcessMode = Node.ProcessModeEnum.Always;
			}
		}
	}
	
	public void HideInventory()
	{
		if (_inventory != null)
		{
			var canvasLayer = _inventory.FindChild("CanvasLayer", false, false) as CanvasLayer;
			if (canvasLayer != null)
			{
				canvasLayer.Visible = false;
			}
		}
		SyncWithInventory();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (invulnTimer > 0)
		{
			invulnMat?.SetShaderParameter("alpha", 1f);
			GetNode<Node3D>(playerMeshPath).Visible = false;
			var timer = GetTree().CreateTimer(0.1f);
			timer.Timeout += () => { GetNode<Node3D>(playerMeshPath).Visible = true; };
			canBeDamaged = false;
			invulnTimer -= (float)delta;
		}
		else
		{
			canBeDamaged = true;
			invulnMat?.SetShaderParameter("alpha", 0f);
		}



	if (health <= 0 && !isDead)
{
    ProcessMode = Node.ProcessModeEnum.Always;
    void MakeNodeBlack(Node node)
    {
        if (node is MeshInstance3D mesh)
        {
            var material = new StandardMaterial3D();
            material.AlbedoColor = Colors.Black;
            material.Emission = Colors.Black;
            mesh.MaterialOverride = material;
        }
        foreach (Node child in node.GetChildren())
            MakeNodeBlack(child);
    }

    void MakeNodeWhite(Node node)
    {
        if (node is MeshInstance3D mesh)
        {
            var material = new StandardMaterial3D();
            material.AlbedoColor = Colors.White;
            material.EmissionEnabled = true;
            material.Emission = Colors.White;
            mesh.MaterialOverride = material;
        }
        foreach (Node child in node.GetChildren())
            MakeNodeWhite(child);
    }

    MakeNodeBlack(GetTree().CurrentScene);
    MakeNodeWhite(GetNode<Node3D>("%PlayerMesh"));
    GetTree().Paused = true;
			_movementStateMachine.TransitionTo("deathState");
    isDead = true;
}
else if (health > 0 && isDead)
{
    GetTree().Paused = false;
    ProcessMode = Node.ProcessModeEnum.Inherit;
    SetPhysicsProcess(true);
    SetProcess(true);
    
    void RestoreMaterials(Node node)
    {
        if (node is MeshInstance3D mesh)
        {
            mesh.MaterialOverride = null;
        }
        foreach (Node child in node.GetChildren())
            RestoreMaterials(child);
    }

    RestoreMaterials(GetTree().CurrentScene);
    isDead = false;
}
	}
}
