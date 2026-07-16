using Godot;

public partial class PlayerState : State
{
    protected PlayerManager pm => GetParent().GetParent<PlayerManager>();
}