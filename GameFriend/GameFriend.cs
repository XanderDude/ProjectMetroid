using Godot;
using System;

public partial class GameFriend : Node3D
{
    

    public InventoryFriend inventoryfriend; 

    public static GameFriend gameinstance; 

    public override void _Ready()
    {
        gameinstance = this;
        inventoryfriend = new InventoryFriend();
    

    }


}
