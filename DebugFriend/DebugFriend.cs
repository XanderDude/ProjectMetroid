using Godot;
using System;

public partial class DebugFriend : CanvasLayer
{

    private Label labelbombarrow => GetNode<Label>("BombArrowCount");
    private LineEdit roomteleporter => GetNode<LineEdit>("RoomTeleporter");
    private RoomFriend roomfriend => GetNode<RoomFriend>("/root/GameFriend/RoomFriend");


    private OptionButton upgradeselectorbutton => GetNode<OptionButton>("Upgrades");
    private Godot.Button addupgradebutton => GetNode<Godot.Button>("Upgrades/AddUpgrade");
    private Godot.Button removeupgradebutton => GetNode<Godot.Button>("Upgrades/RemUpgrade");
    private Godot.Button addbombarrowbutton => GetNode<Godot.Button>("BombArrowCount/AddBombArrow");

    private Label healthlabel => GetNode<Label>("Health");
    
    private ItemDrop.Upgrade selected;

    PlayerManager pm => GetNode<PlayerManager>("/root/GameFriend/Player");

    public override void _Ready()
    {
        
        labelbombarrow.Text = "Bomb Arrows: ";
        ////////////////////////////////////////////////
        roomteleporter.PlaceholderText = "Scene Name";
        roomteleporter.TextChanged += OnTextChanged;
        roomteleporter.TextSubmitted += OnTextSubmitted;
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

    public override void _Process(double delta)
    {
        healthlabel.Text = $"Health: {pm.health}";
    }


   private void OnAddBombArrowPressed()
    {
        //GD.Print("Adding 5 Bomb Arrows");
        GameFriend.gameinstance.inventoryfriend.AddConsumable("bombArrows", 5);

        var inventory = GameFriend.gameinstance.inventoryfriend;
        int bombCount = 0;
        if (inventory?.consumables != null && inventory.consumables.TryGetValue("bombArrows", out var cnt))
            bombCount = cnt;
        labelbombarrow.Text = $"Bomb Arrows: {bombCount}";
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


    private void OnTextChanged(string newText)
    {
        //GD.Print($"Text changed to: {newText}");
    }
    private void OnTextSubmitted(string text)
    {
        //GD.Print($"User pressed Enter: {text}");
        
        string currentText = roomteleporter.Text;

        roomfriend.room_init(currentText);
        upgradeselectorbutton.ReleaseFocus();
    }

}
