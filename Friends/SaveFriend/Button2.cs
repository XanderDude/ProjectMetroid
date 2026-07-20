using Godot;
using System;

public partial class Button2 : Godot.Button
{
    
    [Export] public int saveslot = 0;


    public override void _Ready()
    {
        
        this.Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
        {
           
           //GD.Print("Load was pressed!");
           GameFriend.gameinstance.savefriend.LoadUpgrades(saveslot);
           ReleaseFocus();
        }
}
