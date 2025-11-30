using Godot;
using System;
using System.Collections.Generic;

public partial class SlideVertColCheck : Node3D
{
	private PhysicsRayQueryParameters3D query;
	private PhysicsDirectSpaceState3D world;

	public override void _Ready()
	{
		var state = GetParent().FindChild("slideState");
		if (state != null) state.GetNode<slideState>(state.GetPath()).vertColCheck = this; //assign this to slideState.cs
		state = null;
		state = GetParent().FindChild("crouchState");
		if (state != null) state.GetNode<groundedState>(state.GetPath()).vertColCheck = this; //assign this to groundedState.cs on crouchState
	}	
	public override void _PhysicsProcess(double delta)
    {
        world = GetWorld3D().DirectSpaceState;
		query = PhysicsRayQueryParameters3D.Create(GlobalPosition, GlobalPosition + Vector3.Up * 1.2f, 0 | 1);
    }	

	public bool RayIsColliding()
    {
        var results = world.IntersectRay(query);

		if (results.Count > 0)
		{
			return true;
		}
		else return false;
    }
}
