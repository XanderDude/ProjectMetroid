using Godot;
using System;
public partial class ItemDrop : RigidBody3D
{
    [Export] public int amount = 0;
    private Area3D area;
    private Vector3 spawnDirection;
    private bool pickedUp = false; 

    private string sound = "ratGrunt1";
    public DropEntry.Type itemType = DropEntry.Type.Upgrade;
    public DropEntry.Upgrade upgrade = DropEntry.Upgrade.none;
    public enum Upgrade { none, wallJump, ravenSlash, mantling, chargeShot, slideBoost, bombArrows, ravenTeleport };

    public override void _Ready()
    {
        area = GetNode<Area3D>("Area3D");
        area.BodyEntered += OnBodyEntered;
        var random = new Random();
        spawnDirection = new Vector3(random.Next(-4, 4), random.Next(-4, 4), 0);
        LinearVelocity = spawnDirection.Normalized() * 4;
    }

    public void OnBodyEntered(Node body)
    {
        if (pickedUp) return; 
        if (body is PlayerManager player)
        {
            pickedUp = true;
            Visible = false;
            area.BodyEntered -= OnBodyEntered;
            
            var itemName = upgrade.ToString();
            if (itemType == DropEntry.Type.Upgrade) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade(itemName);
            else if (itemType == DropEntry.Type.Consumables) GameFriend.gameinstance.inventoryfriend.AddConsumable("bombArrows", amount);
            else if (itemType == DropEntry.Type.Health) player.Health += amount;
            SoundFriend.Play(sound);
            DespawnItem();
        }
    }

    private async void DespawnItem()
    {
        QueueFree();
    }
}