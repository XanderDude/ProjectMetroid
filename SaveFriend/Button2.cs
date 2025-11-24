using Godot;
using System;

public partial class Button2 : Button
{
    
    SaveFriend savefriend;

    public override void _Ready()
    {
        savefriend = GetNode<SaveFriend>("/root/GameFriend/SaveFriend");
        this.Pressed += OnButtonPressed;
    }

    private void OnButtonPressed()
        {
           
           GD.Print("Load was pressed!");
           savefriend.LoadHashSet();
        }
}
