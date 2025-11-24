using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SaveFriend : Node3D
{
    private const string SavePath = "user://game_data.sav";
    private HashSet<string> upgrades;
    private Dictionary<string, int> consumables;
    
    
    
    
    public void SaveUpgrades()
    {
        upgrades = GameFriend.gameinstance.inventoryfriend.upgrades;
        var itemsList = new List<string>(upgrades);
        string jsonString = JsonSerializer.Serialize(itemsList);
        
        using var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        if (saveFile != null)
        {
            saveFile.StoreString(jsonString);
            GD.Print("HashSet saved successfully!");
        }
        else
        {
            GD.PrintErr($"Failed to save: {FileAccess.GetOpenError()}");
        }
    }
    
    public void LoadUpgrades()
    {
        upgrades = GameFriend.gameinstance.inventoryfriend.upgrades;
        if (!FileAccess.FileExists(SavePath))
        {
            GD.Print("Save file doesn't exist yet.");
            return;
        }
        
        using var saveFile = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        if (saveFile != null)
        {
            string jsonString = saveFile.GetAsText();
            var itemsList = JsonSerializer.Deserialize<List<string>>(jsonString);
            
            upgrades.Clear();
            foreach (var item in itemsList)
            {
                upgrades.Add(item);
            }
            
            GD.Print($"HashSet loaded successfully! Items: {string.Join(", ", upgrades)}");
        }
        else
        {
            GD.PrintErr($"Failed to load: {FileAccess.GetOpenError()}");
        }
    }
}