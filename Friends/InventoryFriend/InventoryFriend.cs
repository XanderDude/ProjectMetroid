using Godot;
using System;
using System.Collections.Generic;

public partial class InventoryFriend : Node3D
{

    public HashSet <string> upgrades = new HashSet<string>();
    public Dictionary<string, int> consumables = new Dictionary<string, int>();

    public override void _Ready()
	{


    }

    public void UnlockUpgrade(string upgradeName)
    {
        if (!isUpgradeUnlocked(upgradeName)) 
            upgrades.Add(upgradeName);
        //GD.Print($"Unlocked: {upgradeName}!");
    }

    public void RemoveUpgrade(string upgradeName)
    {
        if (isUpgradeUnlocked(upgradeName))
        upgrades.Remove(upgradeName);
        //GD.Print($"Removed: {upgradeName}!");
    }

    public bool isUpgradeUnlocked(string upgradeName)
    {
        if (upgrades.Contains(upgradeName)) return true;
        
        return false;

    }

     public bool UseConsumable(string itemName, int amount)
    {
        if (consumables.ContainsKey(itemName) && consumables[itemName] >= amount)
        {
            consumables[itemName] -= amount;
            return true;
        }
        return false;
    }
    
       public void AddConsumable(string itemName, int amount)
    {
        if (consumables.ContainsKey(itemName))
        {
            consumables[itemName] += amount;
        }
        else
        {
            consumables[itemName] = amount;
        }
    }


}
