using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SaveFriend : Node3D
{
    private const string SavePath = "user://game_data.sav";
    private HashSet<string> inventory;
    

    
    
    
    public void SaveHashSet()
    {
        inventory = GameFriend.gameinstance.inventoryfriend.upgrades;
        var itemsList = new List<string>(inventory);
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
    
    public void LoadHashSet()
    {
        inventory = GameFriend.gameinstance.inventoryfriend.upgrades;
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
            
            inventory.Clear();
            foreach (var item in itemsList)
            {
                inventory.Add(item);
            }
            
            GD.Print($"HashSet loaded successfully! Items: {string.Join(", ", inventory)}");
        }
        else
        {
            GD.PrintErr($"Failed to load: {FileAccess.GetOpenError()}");
        }
    }
}