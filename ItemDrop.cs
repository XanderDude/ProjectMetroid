using Godot;
using System;
using System.Numerics;

public partial class ItemDrop : CharacterBody3D
{
    public float timer = 0.0f;
    
    
    enum Type { Upgrade, Consumables, Health}

    [Export] Type itemType = Type.Upgrade; 

    [Export] public string itemName = "";

    [Export] public int amount = 0;

    //if the player comes in contact make the object disappear
    public void OnBodyEntered(Node body)
    {
        if (body is PlayerManager player)
        {
            GD.Print($"Player picked up {itemName}");
            var pickupsound = GetNode<AudioStreamPlayer3D>("PickupSound");
            pickupsound.Play();
            //move item toward center of player and then disappear
            //when the sound is done playing, delete the item
            Visible = false;
            

            if (itemType == Type.Upgrade) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade(itemName);
            else if (itemType == Type.Consumables) GameFriend.gameinstance.inventoryfriend.AddConsumable(itemName, amount);
            
            var timer = GetTree().CreateTimer(15.0f);
            timer.Timeout += () =>  
            QueueFree();
           
            
        }
    }
    public override void _Ready()
    {
        var area = GetNode<Area3D>("Area3D");
        area.BodyEntered += OnBodyEntered;
        //make x direction random between -4 and 4 
        var random = new Random();
        int randomdir = random.Next(-4, 5);
        Velocity += new Godot.Vector3(randomdir, 3, 0);


    }


    public override void _PhysicsProcess(double delta)
    {

        if (itemType == Type.Consumables)
        {
                timer += (float)delta;

                if (timer > 0.2f)
                {
                    Velocity = new Godot.Vector3(0, Velocity.Y, 0);
                    Velocity += new Godot.Vector3(0, -9.8f, 0) * (float)delta;
                }
                MoveAndSlide();
        }

    }
}
