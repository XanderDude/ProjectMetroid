using Godot;
using System;
using System.Collections.Generic;

public partial class SlideVertColCheck : Node3D
{
	private PhysicsRayQueryParameters3D query;
	private PhysicsDirectSpaceState3D world;
	[Export] private ShapeCast3D shape;
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
		if(shape != null) return;
        world = GetWorld3D().DirectSpaceState;
		query = PhysicsRayQueryParameters3D.Create(GlobalPosition, GlobalPosition + Vector3.Up * 1.2f, 0 | 1);
    }	

	public bool VertCheckIsColliding()
    {
		if(shape == null)
        {
            var rayResults = world.IntersectRay(query);
			if (rayResults.Count > 0)
			{
				return true;
			}
			else return false;
        }

		else
        {
            var shapeResults = shape.CollisionResult;
			if (shapeResults.Count > 0)
			{
				return true;
			}
			else return false;
        }
    }
}
