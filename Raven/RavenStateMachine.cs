using Godot;
using System;
using System.Collections.Generic;

public partial class RavenStateMachine : FiniteStateMachine
{
   public override void _Ready()
    {
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        var raven = GetParent<Raven>();
        if (raven?.pm == null || !raven.pm.inventoryfriend.isUpgradeUnlocked("ravenSlash")) return;

        current_node_state?.HandleInput(@event);
    }
}