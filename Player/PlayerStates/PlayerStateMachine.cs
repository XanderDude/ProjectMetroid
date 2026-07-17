using Godot;

public partial class PlayerStateMachine : FiniteStateMachine
{
     public override void _Ready()
    {
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        current_node_state?.HandleInput(@event);
    }
}