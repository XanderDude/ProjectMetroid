using Godot;
using System;

public partial class Button : Godot.Button
{
    
        SaveFriend savefriend;

        public override void _Ready()
        {
            savefriend = GetNode<SaveFriend>("/root/GameFriend/SaveFriend");
            this.Pressed += OnButtonPressed;
        }
        private void OnButtonPressed()
        {
            
            //GD.Print("Button was pressed!");
            savefriend.SaveUpgrades();
        }

}
