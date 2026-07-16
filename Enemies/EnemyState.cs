using Godot;

public partial class EnemyState : State
{
    protected Enemy ec => GetParent().GetParent<Enemy>();
    protected EnemyStateMachine esm => GetParent<EnemyStateMachine>();
}