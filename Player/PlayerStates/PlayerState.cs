using Godot;

public partial class PlayerState : State
{
    protected PlayerManager pm => GetParent().GetParent<PlayerManager>();
    protected PlayerStateMachine psm => GetParent<PlayerStateMachine>();

    protected PlayerStateMachine GetMachinesm => GetParent<PlayerStateMachine>();
    protected Node3D parentMesh => pm.GetNode<Node3D>(pm.playerMeshPath);
    protected CharacterBody3D player => pm;
}