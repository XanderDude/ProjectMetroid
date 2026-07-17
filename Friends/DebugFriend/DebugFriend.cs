using Godot;
using System;
using System.Collections.Generic;

public partial class DebugFriend : CanvasLayer
{
    private const string LEVELS_ROOT = "res://Environment/Levels";

    private Label labelbombarrow => GetNode<Label>("BombArrowCount");
    private OptionButton upgradeselectorbutton => GetNode<OptionButton>("Upgrades");
    private Godot.Button addupgradebutton => GetNode<Godot.Button>("Upgrades/AddUpgrade");
    private Godot.Button removeupgradebutton => GetNode<Godot.Button>("Upgrades/RemUpgrade");
    private Godot.Button addbombarrowbutton => GetNode<Godot.Button>("BombArrowCount/AddBombArrow");

    private Godot.Button resetbutton => GetNode<Godot.Button>("RESET");

    private Label healthlabel => GetNode<Label>("Health");
    private OptionButton levellist => GetNode<OptionButton>("LevelList");

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

        foreach (string path in levelScenePaths)
        {
            levellist.AddItem(path.GetFile());
        }
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
        RoomFriend.instance.DebugLoadRoom(levelScenePaths[(int)index]);
        levellist.ReleaseFocus();
    }

    public override void _Process(double delta)
    {
        healthlabel.Text = $"Health: {GameFriend.gameinstance.player.health}";
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
