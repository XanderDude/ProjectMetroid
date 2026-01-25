using Godot;
using System;
using System.Collections.Generic;

public partial class VerticalCollisionCheck : Node3D
{
	[Export] public ShapeCast3D shape;


	public bool VertCheckIsColliding()
    {
		shape.ForceShapecastUpdate();
		var shapeResults = shape.CollisionResult;
		if (shapeResults.Count > 0)
		{
			return true;
		}
		else return false;

    }
}
