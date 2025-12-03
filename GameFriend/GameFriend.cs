using Godot;
using System;

public partial class GameFriend : Node3D
{
    public InventoryFriend inventoryfriend; 
    public static GameFriend gameinstance; 
    public SaveFriend savefriend => GetNode<SaveFriend>("SaveFriend");
    Raven raven => GetNode<Raven>("%Raven");

    public override void _Ready()
    {
        SoundFriend.Play("background_SFX");
        gameinstance = this;
        inventoryfriend = new InventoryFriend();
        savefriend.LoadUpgrades(0);
    }

    public override void _Process(double delta)
    {

        if (raven != null)
        {
            if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("ravenSlash"))
            {
                raven.ProcessMode = ProcessModeEnum.Inherit;
                raven.Visible = true;
            }
            else
            {
                raven.ProcessMode = ProcessModeEnum.Disabled;
                raven.Visible = false;
            }
        }

        
    }
}
