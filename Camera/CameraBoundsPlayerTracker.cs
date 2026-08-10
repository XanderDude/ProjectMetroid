using Godot;
using System;
using System.Collections.Generic;

public partial class CameraBoundsPlayerTracker : Node
{
	[Export] private Node3D[] boundObjs;
	private List<Rect2> cameraBounds = [], collisionBounds = []; 
	private Rect2 currentRect;
	[Export] private float sideOffset = 5f, botOffset = 1f;
	private Node3D playerPos;
	private CameraFriend camera;
	float timeAccumulator = 0f;
	float updateDelay = 0.1f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera = GameFriend.gameinstance.camera;
		playerPos = GameFriend.gameinstance.player;
		foreach (Node3D obj in boundObjs)
		{
			var rect = new Rect2(new Vector2(obj.GlobalPosition.X - obj.Scale.X / 2, obj.GlobalPosition.Y - obj.Scale.Y / 2), new Vector2(obj.Scale.X, obj.Scale.Y));
			collisionBounds.Add(rect);
			if (obj.HasMeta("CameraBounds")) cameraBounds.Add((Rect2)obj.GetMeta("CameraBounds"));
			else cameraBounds.Add(rect);
		}
		if (camera == null || playerPos == null)
		{
			GD.Print("Camera or Player not found in CameraBoundsPlayerTracker");
			return;
		}
		CallDeferred("InitiateCameraBounds");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if (boundObjs == null || cameraBounds.Count == 0) return;
		timeAccumulator += (float)delta;
		if (timeAccumulator >= updateDelay) timeAccumulator -= updateDelay;
		else return;
		CheckPlayerPos();
	}

	private void CheckPlayerPos()
	{
		Rect2 newRect = new();
		for (int i = 0; i < collisionBounds.Count; i++)
		{
			if (collisionBounds[i].HasPoint(new(playerPos.GlobalPosition.X,playerPos.GlobalPosition.Y)))
			{
				if (!newRect.HasArea()) newRect = cameraBounds[i];
				else
				{
					newRect.Merge(cameraBounds[i]);
				}
			}
		}
		if (currentRect != newRect && newRect.HasArea()) 
		{
			SetRoomBoundsRect(newRect);
		}
		
	}

	public void InitiateCameraBounds()
	{
		if (camera == null || playerPos == null)
		{
			GD.Print("Camera or Player not found in CameraBoundsPlayerTracker");
			return;
		}

		if (boundObjs == null || cameraBounds.Count == 0) //no bound is set
		{
			camera.SetRoomBounds(-999f, 999f, -999f, 999f); //no bounds
			return;
		}
		else if (cameraBounds.Count > 1) //more than 1 bound in the level
		{
			var closestRect = cameraBounds[0];
			var distance = GetDistanceToClosestSide(new(playerPos.GlobalPosition.X,playerPos.GlobalPosition.Y), cameraBounds[0]);
			for (int i = 1; i < cameraBounds.Count; i++)
			{
				var squaredDist = GetDistanceToClosestSide(new(playerPos.GlobalPosition.X,playerPos.GlobalPosition.Y), cameraBounds[i]);
				if (squaredDist < distance) 
				{
					distance = squaredDist;
					closestRect = cameraBounds[i];
				}			
			}
			currentRect = closestRect;
		}
		else currentRect = cameraBounds[0]; //one bound
	
		SetRoomBoundsRect(currentRect);
		//SetRoomBounds(bounds);
	}

	private float GetDistanceToClosestSide(Vector2 target, Rect2 rect)
	{
		if (rect.HasPoint(target)) return 0f; //target inside rect

		float closestX = Math.Abs(target.X - rect.Position.X) < Math.Abs(target.X - rect.End.X) ? rect.Position.X : rect.End.X; 
		float closestY = Math.Abs(target.Y - rect.Position.Y) < Math.Abs(target.Y - rect.End.Y) ? rect.Position.Y : rect.End.Y; 
		Vector2 closestPoint = new(closestX, closestY); //closest corner
		if (target.X > rect.Position.X && target.X < rect.End.X) closestPoint = new(target.X, closestY);
		else if (target.Y > rect.Position.Y && target.Y < rect.End.Y) closestPoint = new(closestX, target.Y);
		var minDistance = target.DistanceSquaredTo(closestPoint); //closest point

		GD.Print($"Closest Point: {closestPoint}, Distance: {minDistance}");
		return minDistance;
	}

	private void SetRoomBounds(Transform3D rect)
	{
		var sideOffset = 5f;
		//origin = position(x, y), Basis.X.X = width, Basis.Y.Y = height

		var roomMinX = rect.Basis.X.X <= sideOffset*2 ? rect.Origin.X : rect.Origin.X - rect.Basis.X.X / 2 + sideOffset; //left
		var roomMaxX = rect.Basis.X.X <= sideOffset*2 ? rect.Origin.X : rect.Origin.X + rect.Basis.X.X / 2 - sideOffset; //right
		var roomMinY = rect.Origin.Y - rect.Basis.Y.Y / 2; //bottom
		var roomMaxY = rect.Origin.Y + rect.Basis.Y.Y / 2; //top

		roomMinY += 2f;

		camera.SetRoomBounds(roomMinX, roomMaxX, roomMinY, roomMaxY);
	}
	private void SetRoomBoundsRect(Rect2 rect)
	{
		currentRect = rect;
		GD.Print("[Source Rect]Pos: " + currentRect.Position + " End: " + currentRect.End);
		var roomMinX = rect.Size.X <= sideOffset * 2 ? rect.GetCenter().X : rect.Position.X + sideOffset; //left
		var roomMaxX = rect.Size.X <= sideOffset * 2 ? rect.GetCenter().X : rect.End.X - sideOffset; //right
		var roomMinY = rect.Position.Y; //bottom
		var roomMaxY = rect.End.Y; //top

		roomMinY += botOffset;
		GD.Print($"[CameraBounds]Pos: ({roomMinX}, {roomMinY}) End: ({roomMaxX}, {roomMaxY})");
		camera.SetRoomBounds(roomMinX, roomMaxX, roomMinY, roomMaxY);
	}
}
