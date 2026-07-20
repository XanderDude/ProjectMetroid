using Godot;
using System;
using System.Collections.Generic;

public partial class DebugFriend : CanvasLayer
{
    private const string LEVELS_ROOT = "res://Environment/Levels/Dungeon_WallJumpUnlock";

    private Label labelbombarrow => GetNode<Label>("BombArrowCount");
    private OptionButton upgradeselectorbutton => GetNode<OptionButton>("Upgrades");
    private Godot.Button addupgradebutton => GetNode<Godot.Button>("Upgrades/AddUpgrade");
    private Godot.Button removeupgradebutton => GetNode<Godot.Button>("Upgrades/RemUpgrade");
    private Godot.Button addbombarrowbutton => GetNode<Godot.Button>("BombArrowCount/AddBombArrow");

    private Godot.Button resetbutton => GetNode<Godot.Button>("RESET");

    private Label healthlabel => GetNode<Label>("Health");
    private Godot.Button addhealthbutton => GetNode<Godot.Button>("Health/AddHealth");
    private OptionButton levellist => GetNode<OptionButton>("RoomTeleporter");

    private ItemDrop.Upgrade selected;

    private List<string> levelScenePaths = new();

    public void init_debugfriend()
    {
        labelbombarrow.Text = "Bomb Arrows: ";

        foreach (string name in Enum.GetNames(typeof(ItemDrop.Upgrade)))
        {
            upgradeselectorbutton.AddItem(name);
        }
        upgradeselectorbutton.ItemSelected += OnDropdownItemSelected;

        addupgradebutton.Pressed += OnAddUpgradePressed;
        removeupgradebutton.Pressed += OnRemoveUpgradePressed;
        addbombarrowbutton.Pressed += OnAddBombArrowPressed;

        ////////////////////////////////////////////////

        PopulateLevelList();
        levellist.ItemSelected += OnLevelListItemSelected;
    }

    private void PopulateLevelList()
    {
        levelScenePaths.Clear();
        ScanScenesRecursive(LEVELS_ROOT, levelScenePaths);
        levelScenePaths.Sort();

        levellist.AddItem(""); // blank placeholder - nothing loaded until you actually pick one
        foreach (string path in levelScenePaths)
        {
            levellist.AddItem(path.GetFile());
        }
        levellist.Selected = 0;
    }

    private void ScanScenesRecursive(string path, List<string> results)
    {
        var dir = DirAccess.Open(path);
        if (dir == null) return;

        dir.ListDirBegin();
        var entry = dir.GetNext();
        while (entry != "")
        {
            string fullPath = $"{path}/{entry}";
            if (dir.CurrentIsDir())
            {
                ScanScenesRecursive(fullPath, results);
            }
            else if (entry.EndsWith(".tscn"))
            {
                results.Add(fullPath);
            }
            entry = dir.GetNext();
        }
        dir.ListDirEnd();
    }

   private void OnLevelListItemSelected(long index)
{
    if (index == 0) { levellist.ReleaseFocus(); return; }
    //GD.Print($"index: {index} path: {levelScenePaths[(int)index - 1]}");
    RoomFriend.instance.DebugLoadRoom(levelScenePaths[(int)index - 1]);
    levellist.ReleaseFocus();
}

    public override void _Process(double delta)
    {
        healthlabel.Text = $"Health: {GameFriend.gameinstance.player.Health}";
        int bombCount = 0;
        var inventory = GameFriend.gameinstance.inventoryfriend;
        if (inventory?.consumables != null && inventory.consumables.TryGetValue("bombArrows", out var cnt))
            bombCount = cnt;
        labelbombarrow.Text = $"Bomb Arrows: {bombCount}";

       
    }

   private void OnAddBombArrowPressed()
    {
        //GD.Print("Adding 5 Bomb Arrows");
        GameFriend.gameinstance.inventoryfriend.AddConsumable("bombArrows", 5);

        
        
        upgradeselectorbutton.ReleaseFocus();
    }

    private void OnAddHealthPressed()
    {
       
    }
    private void OnAddUpgradePressed()
    {
        //GD.Print($"Adding upgrade: {selected}");
        
        GameFriend.gameinstance.inventoryfriend.UnlockUpgrade(selected.ToString());
        upgradeselectorbutton.ReleaseFocus();
    }

    private void OnRemoveUpgradePressed()
    {
        //GD.Print($"Removing upgrade: {selected}");
        GameFriend.gameinstance.inventoryfriend.RemoveUpgrade(selected.ToString());
        upgradeselectorbutton.ReleaseFocus();
    }




    private void OnDropdownItemSelected(long index)
    {
        selected = (ItemDrop.Upgrade)index;
        //GD.Print($"Selected: {selected}");     
        upgradeselectorbutton.ReleaseFocus();
    }


   

}