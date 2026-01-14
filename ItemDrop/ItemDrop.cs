using Godot;
using System;

public partial class ItemDrop : RigidBody3D
{
    public float timer = 0.0f;
    public enum Type { Upgrade, Consumables, Health}
    public enum Upgrade { none, wallJump, ravenSlash, mantling, chargeShot, slideBoost, bombArrows, ravenTeleport };
    
    [ExportGroup("Type")]
    

    [Export] Type itemType = Type.Upgrade; 


    [ExportGroup("Upgrade Type")]
     [Export]public Upgrade upgrade = Upgrade.none;


    [ExportGroup("ETC")]

    [Export] public int amount = 0;
    [Export] private float despawnTime = 0.5f;

    public string itemName = "";
    private Area3D area;
    private Vector3 spawnDirection;

    //if the player comes in contact make the object disappear
    public void OnBodyEntered(Node body)
    {
        itemName = upgrade.ToString();

        if (body is PlayerManager player)
        {
            area.CollisionMask = 0;
            GD.Print($"Player picked up {itemName}");
            var pickupsound = GetNode<AudioStreamPlayer3D>("PickupSound");
            pickupsound.Play();
            var crossbow = body.GetNode<Node3D>("PlayerMesh/Skeleton3D/Crossbow/Crossbow");
            //move item toward center of player and then disappear
            //when the sound is done playing, delete the item
            Visible = false;



            if (itemType == Type.Upgrade) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade(itemName);
            else if (itemType == Type.Consumables) GameFriend.gameinstance.inventoryfriend.AddConsumable("bombArrows", amount);
            else if (itemType == Type.Health) player.Health += amount;

            DespawnItem();

        }



    }
    private async void DespawnItem()
    {
        await ToSignal(GetTree().CreateTimer(despawnTime, false, true, false), "timeout");
        QueueFree();
    }
    public override void _Ready()
    {
        area = GetNode<Area3D>("Area3D");
        area.BodyEntered += OnBodyEntered;
        //make x direction random between -4 and 4 
        var random = new Random();
        spawnDirection = new Vector3(random.Next(-4,4), random.Next(-4,4), 0);
        LinearVelocity = spawnDirection.Normalized() * 4;
    }


    public override void _PhysicsProcess(double delta)
    {
        //ApplyCentralForce(new Vector3(spawnDirection * 10 -(float)delta, spawnDirection * 10 -(float)delta, 0));
        if (itemType == Type.Consumables)
        {
                timer += (float)delta;

        }
    }
}
