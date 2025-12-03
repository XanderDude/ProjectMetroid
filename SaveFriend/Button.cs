using Godot;
using System;

public partial class Button : Godot.Button
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
            
            //GD.Print("Button was pressed!");

            ReleaseFocus();
        }

}
