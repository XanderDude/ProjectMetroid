using Godot;
using System;

public partial class ResetButton : Godot.Button
{
    
        [Export] public int saveslot = 0;
        public override void _Ready()
        {
        
            this.Pressed += OnButtonPressed;
        }
        private void OnButtonPressed()
        {
           GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
           ReleaseFocus();
        }

}
