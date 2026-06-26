using Godot;
using System;

public partial class DebugFriend : CanvasLayer
{

    private Label labelbombarrow => GetNode<Label>("BombArrowCount");
    private OptionButton roomteleporter => GetNode<OptionButton>("RoomTeleporter");
    private OptionButton doorid => GetNode<OptionButton>("IDTeleporter");
    private OptionButton upgradeselectorbutton => GetNode<OptionButton>("Upgrades");
    private Godot.Button addupgradebutton => GetNode<Godot.Button>("Upgrades/AddUpgrade");
    private Godot.Button removeupgradebutton => GetNode<Godot.Button>("Upgrades/RemUpgrade");
    private Godot.Button addbombarrowbutton => GetNode<Godot.Button>("BombArrowCount/AddBombArrow");

    private Godot.Button resetbutton => GetNode<Godot.Button>("RESET");

    private Label healthlabel => GetNode<Label>("Health");
    
    private ItemDrop.Upgrade selected;

    private string selectedRoom;

    public void init_debugfriend()
    {
        
        labelbombarrow.Text = "Bomb Arrows: ";
        ////////////////////////////////////////////////
        if (GameFriend.gameinstance.roomfriend != null)
        {
            foreach (string name in GameFriend.gameinstance.roomfriend.tab1.Values)
            {
            if (!OptionHasText(roomteleporter, name))
                roomteleporter.AddItem(name);
            }
            foreach (string name in GameFriend.gameinstance.roomfriend.tab2.Values)
            {
            if (!OptionHasText(roomteleporter, name))
                roomteleporter.AddItem(name);
            }
            roomteleporter.ItemSelected += OnDropdownRoomSelected;
    }
        
            
        ////////////////////////////////////////////////
        
        

        foreach (string name in Enum.GetNames(typeof(ItemDrop.Upgrade)))
        {
            upgradeselectorbutton.AddItem(name);
        }
        upgradeselectorbutton.ItemSelected += OnDropdownItemSelected;

        addupgradebutton.Pressed += OnAddUpgradePressed;
        removeupgradebutton.Pressed += OnRemoveUpgradePressed;
        addbombarrowbutton.Pressed += OnAddBombArrowPressed;



    }
    private bool OptionHasText(OptionButton ob, string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        for (int i = 0; i < ob.GetItemCount(); i++)
        {
            if (string.Equals(ob.GetItemText(i), text, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
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

    private void OnDropdownRoomSelected(long index)
    {
        //store door ids from selected room from both tables
        selectedRoom = roomteleporter.GetItemText((int)index);
        if (GameFriend.gameinstance.roomfriend != null) GameFriend.gameinstance.roomfriend.room_init(selectedRoom);
        roomteleporter.ReleaseFocus();
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
