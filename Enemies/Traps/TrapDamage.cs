using Godot;
using System;
using System.Threading.Tasks;

public partial class TrapDamage : Area3D
{
    public int damage = 5;
    public float damageCooldown = 2f;
    private float _damageCooldown;
    private PlayerManager player => GameFriend.gameinstance.player;
    private bool playerInRange = false;
    private Area3D collider;

    public override void _Ready()
    {
        _damageCooldown = damageCooldown;
        damageCooldown = 0;
        collider = GetNode<Area3D>(GetPath());
        collider.Monitoring = true;
        //AssignPlayer();
    }
    private async void AssignPlayer()
    {
        await ToSignal(GetTree().GetCurrentScene(), Node.SignalName.Ready);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (damageCooldown > 0) damageCooldown -= (float)delta;
        if (damageCooldown <= 0 && playerInRange && player.canBeDamaged) //trap is ready and there is a target
        {
            damageCooldown = _damageCooldown;
            player.health -= damage;
        }
    }

    public void OnLeaveCollider(Node3D node)
    {
        playerInRange = false;
    }

    public void OnCollide(Node3D node)
    {
        playerInRange = true;
        if (damageCooldown <= 0 && player.canBeDamaged) //damaging collider 
        {
            damageCooldown = _damageCooldown;
            player.health -= damage;
        }
    }
}
