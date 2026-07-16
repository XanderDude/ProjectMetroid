using Godot;

public partial class PlayerState : State
{
    protected PlayerManager pm => GetParent().GetParent<PlayerManager>();

    public override void _Ready()
    {
        if (pm == null)
        {
            GD.PushError("PlayerState: PlayerManager not found in parent nodes.");
            QueueFree();
        }
    }
}