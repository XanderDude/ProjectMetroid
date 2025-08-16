using Godot;
using System;

public enum ItemCategory
{
	None,
	Ranged,
	Melee,
	Consumable
}

public partial class Inventory : ItemList
{
	[Export] public int inventorySize = 20;
	[Export] public Texture2D blankIcon;
	
	private Item[] items;
	
	public override void _Ready() 
	{
		items = new Item[inventorySize];
		
		AddThemeConstantOverride("v_separation", 0);
		AddThemeConstantOverride("h_separation", 0);
		AddThemeConstantOverride("icon_margin", 0);
		AddThemeConstantOverride("line_separation", 0);
		
		IconMode = ItemList.IconModeEnum.Top;
		AddThemeConstantOverride("font_size", 0);
		
		// Remove the white outline around the entire ItemList box
		var emptyStyle = new StyleBoxEmpty();
		
		AddThemeStyleboxOverride("focus", emptyStyle);
		
		for (int i = 0; i < inventorySize; i++) {
			AddItem("", blankIcon); 
		}
		
		ItemClicked += OnInventoryItemClicked;
	}
	
	public bool AddInventoryItem(Item item) 
{
	if (item == null || item.Qty <= 0) return false;
	
	// Check if item is unique and already exists
	if (item.IsUnique && HasUniqueItem(item.ID))
	{
		GD.Print($"Cannot add {item.Name} - unique item already exists in inventory");
		return false;
	}
	
	// Try to stack first
	bool couldPickup = AddStackableItem(item);
	
	// If all items were stacked, we're done
	if (item.Qty == 0) return true; 
	
	// Find empty slot for remaining items
	for (int i = 0; i < inventorySize; i++) 
	{
		if (items[i] != null) continue;
		
		items[i] = item;
		SetItemIcon(i, item.Icon);
		
		// Show quantity for stackable items but not for infinite items
		if (!item.IsInfinite && item.MaxQty > 1) 
		{
			SetItemText(i, item.Qty.ToString());
		}
		else
		{
			SetItemText(i, "");
		}
		
		return true;
	}

	return couldPickup;
}
	
	private bool HasUniqueItem(int itemID)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] != null && items[i].ID == itemID && items[i].IsUnique)
			{
				return true;
			}
		}
		return false;
	}
	
	private bool AddStackableItem(Item item) 
{
	bool couldPickup = false; 
	
	GD.Print($"=== AddStackableItem DEBUG ===");
	GD.Print($"Trying to add: ID={item.ID}, Name={item.Name}, Qty={item.Qty}, MaxQty={item.MaxQty}, IsUnique={item.IsUnique}, IsInfinite={item.IsInfinite}");
	
	// Don't stack unique or infinite items
	if (item.IsUnique || item.IsInfinite) 
	{
		GD.Print("Item is unique or infinite - not stacking");
		return false;
	}
	
	for (int i = 0; i < items.Length; i++) {
		if (items[i] == null) continue;
		
		GD.Print($"Checking slot {i}: ID={items[i].ID}, Name={items[i].Name}, Qty={items[i].Qty}, MaxQty={items[i].MaxQty}, IsUnique={items[i].IsUnique}, IsInfinite={items[i].IsInfinite}");
		
		// Check if it's the same item type and can stack more
		if (items[i].ID != item.ID)
		{
			GD.Print($"Different ID: {items[i].ID} vs {item.ID}");
			continue;
		}
		
		if (items[i].Qty >= items[i].MaxQty)
		{
			GD.Print($"Slot full: {items[i].Qty} >= {items[i].MaxQty}");
			continue;
		}
		
		GD.Print($"Found stackable slot {i}! Adding {item.Qty} to existing {items[i].Qty}");
		
		// If adding this item would exceed max quantity
		if (items[i].Qty + item.Qty > items[i].MaxQty)
		{
			int amountToAdd = items[i].MaxQty - items[i].Qty;
			
			items[i].Qty = items[i].MaxQty;
			item.Qty = item.Qty - amountToAdd; 
			
			couldPickup = true;
			
			// Update display
			if (!items[i].IsInfinite && items[i].MaxQty > 1)
			{
				SetItemText(i, items[i].Qty.ToString());
			}
			
			GD.Print($"Partial stack: added {amountToAdd}, remaining {item.Qty}");
			continue;
		}
		
		// Can add all of the item to this stack
		items[i].Qty += item.Qty;
		item.Qty = 0;
		
		// Update display
		if (!items[i].IsInfinite && items[i].MaxQty > 1)
		{
			SetItemText(i, items[i].Qty.ToString());
		}
		
		GD.Print($"Full stack: added all {item.Qty}, slot now has {items[i].Qty}");
		return true; 
	}
	
	GD.Print("No stackable slots found");
	return couldPickup;
}
	
	public void RemoveInventoryItem(int index) 
	{
		if (index < 0 || index >= inventorySize) return; 
		
		items[index] = null;
		SetItemIcon(index, blankIcon);
		SetItemText(index, " ");
	}
	
	public Item GetInventoryItem(int index) 
	{
		if (index < 0 || index >= inventorySize) return null; 
		
		return items[index];
	}
	
	public void EquipItem(int index)
	{
		GD.Print($"EquipItem called with index: {index}");
		
		if (index < 0 || index >= inventorySize || items[index] == null) 
		{
			GD.Print($"EquipItem: Invalid conditions - index: {index}, inventorySize: {inventorySize}, item is null: {items[index] == null}");
			return;
		}
		
		var itemToEquip = items[index];
		GD.Print($"EquipItem: Found item to equip: {itemToEquip.Name}");
		
		if (itemToEquip.Category == ItemCategory.Ranged)
		{
			UnequipItemsByCategory(ItemCategory.Ranged);
		}
		else if (itemToEquip.Category == ItemCategory.Melee)
		{
			UnequipItemsByCategory(ItemCategory.Melee);
		}
		
		itemToEquip.Equipped = true;
		GD.Print($"Equipped {itemToEquip.Name}");
		
		UpdateItemDisplay(index);
	}
	
	public void UnequipItemsByCategory(ItemCategory category)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] != null && items[i].Category == category && items[i].Equipped)
			{
				items[i].Equipped = false;
				GD.Print($"Unequipped {items[i].Name}");
				UpdateItemDisplay(i);
			}
		}
	}
	
	// Check if any item of a category is equipped
	public bool IsAnyItemEquippedInCategory(ItemCategory category)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] != null && items[i].Category == category && items[i].Equipped)
			{
				return true;
			}
		}
		return false;
	}
	
	public Item GetEquippedItemInCategory(ItemCategory category)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] != null && items[i].Category == category && items[i].Equipped)
			{
				return items[i];
			}
		}
		return null;
	}
	
	private void UpdateItemDisplay(int index)
	{
		if (items[index] != null)
		{
			// You can change the background color or add a border for equipped items
			if (items[index].Equipped)
			{
				// Visual indication that item is equipped (example: change item color)
				SetItemMetadata(index, "equipped");
			}
			else
			{
				SetItemMetadata(index, "");
			}
		}
	}
	
	private void OnInventoryItemClicked(long index, Vector2 pos, long mouseButtonIndex)
	{
		GD.Print($"Signal fired! Index: {index}, Button: {mouseButtonIndex}");
		
		if (mouseButtonIndex == 1 && items[index] != null) 
		{
			EquipItem((int)index);
		}
	}
}

public class Item
{
	public int ID;
	public string Name;
	public Texture2D Icon;
	public int MaxQty;
	public int Qty;
	public bool Equipped;
	public ItemCategory Category;
	public bool IsUnique; // New parameter - if true, only one can exist in inventory
	public bool IsInfinite; // New parameter - if true, doesn't display quantity and can't be consumed
}
