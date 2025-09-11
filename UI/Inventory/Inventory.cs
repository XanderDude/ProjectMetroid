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
	
	if (item.IsUnique && HasUniqueItem(item.ID))
	{
		return false;
	}
	

	bool couldPickup = AddStackableItem(item);
	
	
	if (item.Qty == 0) return true; 
	
	
	for (int i = 0; i < inventorySize; i++) 
	{
		if (items[i] != null) continue;
		
		items[i] = item;
		SetItemIcon(i, item.Icon);
		
	
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

	if (item.IsUnique || item.IsInfinite) 
	{
		
		return false;
	}
	
	for (int i = 0; i < items.Length; i++) {
		if (items[i] == null) continue;
		
	
		
	
		if (items[i].ID != item.ID)
		{
	
			continue;
		}
		
		if (items[i].Qty >= items[i].MaxQty)
		{
			
			continue;
		}
		
		
		
	
		if (items[i].Qty + item.Qty > items[i].MaxQty)
		{
			int amountToAdd = items[i].MaxQty - items[i].Qty;
			
			items[i].Qty = items[i].MaxQty;
			item.Qty = item.Qty - amountToAdd; 
			
			couldPickup = true;
			
			
			if (!items[i].IsInfinite && items[i].MaxQty > 1)
			{
				SetItemText(i, items[i].Qty.ToString());
			}
			
			
			continue;
		}
		
	
		items[i].Qty += item.Qty;
		item.Qty = 0;
		
		
		if (!items[i].IsInfinite && items[i].MaxQty > 1)
		{
			SetItemText(i, items[i].Qty.ToString());
		}
		
	
		return true; 
	}
	

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
		//GD.Print($"EquipItem called with index: {index}");
		
		if (index < 0 || index >= inventorySize || items[index] == null) 
		{
	
			return;
		}
		
		var itemToEquip = items[index];
	
		
		if (itemToEquip.Category == ItemCategory.Ranged)
		{
			UnequipItemsByCategory(ItemCategory.Ranged);
		}
		else if (itemToEquip.Category == ItemCategory.Melee)
		{
			UnequipItemsByCategory(ItemCategory.Melee);
		}
		
		itemToEquip.Equipped = true;
		
		UpdateItemDisplay(index);
	}
	
	public void UnequipItemsByCategory(ItemCategory category)
	{
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] != null && items[i].Category == category && items[i].Equipped)
			{
				items[i].Equipped = false;
				
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
		
			if (items[index].Equipped)
			{
				
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
	public bool IsUnique; 
	public bool IsInfinite; 
}
