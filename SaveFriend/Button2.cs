using Godot;
using System;

public partial class Button2 : Button
{
    
    [Export] SaveFriend savefriend;
    [Export] public int saveslot = 0;


    public override void _Ready()
    {
        if (savefriend == null) QueueFree();
        this.Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
        {
           
           //GD.Print("Load was pressed!");
           savefriend.LoadUpgrades(saveslot);
           ReleaseFocus();
        }
}
