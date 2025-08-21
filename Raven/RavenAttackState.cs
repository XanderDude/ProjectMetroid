using Godot;
using System;

public partial class RavenAttackState : State
{
    public override void Enter()
    {
        GD.Print("Raven: Entered Attack State");
    }

    public override void Exit()
    {
    }

    public override void Update(float delta)
    {
    }

    public override void PhysicsUpdate(float delta)
    {
    }

    public override void HandleInput(InputEvent @event)
    {
    }
}