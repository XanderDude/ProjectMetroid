using Godot;
using System;

public partial class Button2 : Button
{
    
    SaveFriend savefriend;
    [Export] public int saveslot = 0;


    public override void _Ready()
    {
        savefriend = GetNode<SaveFriend>("/root/GameFriend/SaveFriend");
        this.Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
        {
           
           //GD.Print("Load was pressed!");
           savefriend.LoadUpgrades(saveslot);
           ReleaseFocus();
        }
}
