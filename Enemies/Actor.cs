using Godot;
using System;
using System.ComponentModel;

public partial class Actor : CharacterBody3D
{

	//Actor Stats 
	[Export] public int health { get; set; } = 0;

	[Export] public Godot.Collections.Array<DropEntry> dropTable { get; set;} = new();

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
	public Vector3 GravityVector => new Vector3(0, gravity, 0);
	[Export] public float attackspeed { get; set; } = 1.0f;
	[Export] public float attackknockback { get; set; } = 5;
	[Export] public float damageovertime { get; set; } = 0f;
	[Export] public float detectionrange { get; set; } = 10f;
	public float IDLE = 0.0f;

	public void DropItems()
	{
		if (dropTable == null || dropTable.Count == 0) return;
		int pitycount = 0;
		foreach (var entry in dropTable)
			{
				if (entry == null) continue;

				for (int j = 0; j <= entry.dropAmount; j++)
				{
					float roll = (float)GD.RandRange(0f, 100f);
					
					if (roll <= entry.dropRate || (pitycount == entry.dropAmount && entry.pitydrop))
					{
						
						var instance = entry.itemScene.Instantiate<ItemDrop>();
						GetParent().AddChild(instance);
						instance.itemType = entry.itemType;
						instance.upgrade = entry.upgrade;
						instance.amount = entry.amount;
						instance.GlobalPosition = GlobalPosition;
					}
					else pitycount++;
					
				}
			}
		
	}

	
}


