using Godot;
using System;
using System.ComponentModel;

public partial class Actor : CharacterBody3D
{

	//Actor Stats 
	[Export] public int health { get; set; } = 0;

	[Export] public Godot.Collections.Array<DropEntry> dropTable { get; set;} = new();

	[Export] public int parryDamage { get; set; } = 25;


	[Export] public float runspeed { get; set; } = 2.5f;

	[Export] public float maxspeed { get; set; } = 5.0f;
	[Export] public float walkspeed { get; set; } = 1.0f;


	[Export] public bool isdodgeprojectile { get; set; } = false;
	public Vector3 walkspeedVector;
    public Vector3 runspeedVector;
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
	[Export] public float detectionrange { get; set; } = 1.5f;

	[Export] public float attackrange { get; set; } = 1.5f;
	public float IDLE = 0.0f;

	public void DropItems()
	{
		if (dropTable == null || dropTable.Count == 0) return;
		int pitycount = 0;
		foreach (var entry in dropTable)
		{
			if (entry == null || entry.dropAmount == 0) continue;

			for (int j = 1; j <= entry.dropAmount; j++)
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


