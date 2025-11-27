using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class SaveFriend : Node3D
{
    private string[] SavePath = new string[]{ "res://save.sav" , "res://save2.sav" , "res://save3.sav" };

    [Export] RoomFriend roomfriend;



    public override void _Ready()
    {
        
    }
    public class SaveData
    {
        public string CurrentRoom { get; set; }
        public List<string> Upgrades { get; set; } = new();
        public Dictionary<string, int> Consumables { get; set; } = new();
    }

    public void SaveUpgrades(int slot)
    {
        if (roomfriend == null)
        {
            GD.PrintErr("SaveFriend: roomfriend is null!");
            return;
        }
        if (GameFriend.gameinstance?.inventoryfriend == null)
        {
            GD.PrintErr("SaveFriend: inventoryfriend is null!");
            return;
        }

        var inv = GameFriend.gameinstance.inventoryfriend;
        var data = new SaveData
        {
            CurrentRoom = roomfriend.currentRoomName,
            Upgrades = new List<string>(inv.upgrades),
            Consumables = new Dictionary<string, int>(inv.consumables)
        };

        string json = JsonSerializer.Serialize(data);
        using var file = FileAccess.Open(SavePath[slot], FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(json);
            GD.Print("Save successful!");
        }
        else
        {
            GD.PrintErr($"Failed to save: {FileAccess.GetOpenError()}");
        }
    }

        public void LoadUpgrades(int slot)
        {

            if (!FileAccess.FileExists(SavePath[slot]))
            {
                GD.Print("No save file found.");
                return;
            }

            if (GameFriend.gameinstance?.inventoryfriend == null)
            {
                GD.PrintErr("SaveFriend: inventoryfriend is null!");
                return;
            }

            using var file = FileAccess.Open(SavePath[slot], FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to load: {FileAccess.GetOpenError()}");
                return;
            }

            var data = JsonSerializer.Deserialize<SaveData>(file.GetAsText());
            if (data == null)
            {
                GD.PrintErr("Failed to deserialize save data.");
                return;
            }

            var inv = GameFriend.gameinstance.inventoryfriend;
            inv.upgrades.Clear();
            foreach (var upgrade in data.Upgrades)
                inv.upgrades.Add(upgrade);

            inv.consumables.Clear();
            foreach (var kv in data.Consumables)
                inv.consumables[kv.Key] = kv.Value;

            var upgrades = GameFriend.gameinstance.inventoryfriend.upgrades;
            GD.Print("Current Upgrades:");
            foreach (var upgrade in upgrades)
            {
                GD.Print("  - " + upgrade);
            }
            string currentRoom = data.CurrentRoom;
            if (roomfriend != null) roomfriend.room_init(currentRoom);
        }
}
