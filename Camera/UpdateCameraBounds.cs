using Godot;
using System;

public partial class UpdateCameraBounds : Area3D
{
	[Export] private Node3D boundsObject;
	private Transform3D bounds;
	private CameraFriend camera;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera = GameFriend.gameinstance.camera;
		if (boundsObject != null) 
		{ 
			bounds = boundsObject.GlobalTransform;
		}
	}

	public void _body_entered(Node3D body)
	{
		//GD.Print("Camera bounds updated");
		if (boundsObject != null)
		{
			//origin = position(x, y), Basis.X.X = width, Basis.Y.Y = height
			var roomMinX = bounds.Origin.X - Math.Abs(bounds.Basis.X.X - 10) / 2; //left
			var roomMaxX = bounds.Origin.X + Math.Abs(bounds.Basis.X.X - 10) / 2; //right
			var roomMinY = bounds.Origin.Y - Math.Abs(bounds.Basis.Y.Y - 2) / 2; //bottom
			var roomMaxY = bounds.Origin.Y + (bounds.Basis.Y.Y/ 2); //top
			camera.SetRoomBounds(roomMinX, roomMaxX, roomMinY, roomMaxY);
		}
	}
}
