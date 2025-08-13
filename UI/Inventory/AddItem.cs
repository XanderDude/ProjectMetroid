using Godot;
using System;

public partial class AddItem : Button
{
	[Export] Inventory inv;
	[Export] int id;
	[Export] string name;
	[Export] Texture2D itemicon;
	[Export] int maxQty;
	[Export] int qty;
	
	public override void _Ready()
	{
		Pressed += () => AddNewItem();
	}
	
	private void AddNewItem()
	{
		Item item = new Item {
			ID = id,
			Name = name,
			Icon = itemicon,
			MaxQty = maxQty,
			Qty = qty
		};
		
		inv.AddInventoryItem(item);
	}
	
}
