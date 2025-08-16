using Godot;
public partial class AddItem : Button
{
	[Export] Inventory inv;
	[Export] int id;
	[Export] string name;
	[Export] Texture2D itemicon;
	[Export] int maxQty;
	[Export] int qty;
	[Export] bool isequip;
	[Export] ItemCategory category;
	[Export] bool isUnique;
	[Export] bool isInfinite; 
	
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
			Qty = qty,
			Equipped = isequip,
			Category = category,
			IsUnique = isUnique, 
			IsInfinite = isInfinite 
		};
		
		inv.AddInventoryItem(item);
	}
}
