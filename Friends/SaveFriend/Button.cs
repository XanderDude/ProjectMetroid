using Godot;
using System;

public partial class Button : Godot.Button
{
    
        [Export] public int saveslot = 0;
        public override void _Ready()
        {
        
            this.Pressed += OnButtonPressed;
        }
        private void OnButtonPressed()
        {
            
            //GD.Print("Button was pressed!");
            GameFriend.gameinstance.savefriend.SaveUpgrades(saveslot);
            ReleaseFocus();
        }

}
