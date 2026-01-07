using Godot;
using System;


public partial class ScreenTransitionScript : Godot.TextureRect
{
	[Export] public string roomPath;
	private AnimationPlayer blackScreen;
	private Area3D collider;
	private bool isEntering = false;
	private Node3D player; 
	private Node3D gameNode;
	private string currentRoomName;
	private string lastRoomName;
	
	public override void _Ready() {
		blackScreen = GetNode<AnimationPlayer>("/root/Game/GlobalTransition/%AnimationPlayer"); 
		roomPath =  "Room2";
		gameNode = GetNode<Node3D>("/root/Game");
		player = gameNode.GetNode<Node3D>("Player");
		
		currentRoomName = GetCurrentRoom()?.Name.ToString() ?? "";
		collider = GetNode<Area3D>($"/root/Game/{currentRoomName}/roomTeleportHitbox/%Area3D");
		if (currentRoomName == "Room1") collider.Monitoring = true;
	
}
	
	private bool isEnterFromLeft() {
		return collider.GlobalPosition.X >= player.GlobalPosition.X;
	}
	
	private bool isExitFromRight() {
		
		return isEnterFromLeft();
	}
	
	private void UnloadCurrentRoom() {
	var currentRoom = GetCurrentRoom();
	if (currentRoom != null) {
		currentRoom.Visible = false;
		currentRoom.ProcessMode = Node3D.ProcessModeEnum.Disabled;
		
		
		var currentRoomArea = currentRoom.GetNode<Area3D>("roomTeleportHitbox/Area3D");
		currentRoomArea.Monitoring = false;
	}
}
	
	private void LoadRoom(string roomName) {
		Modulate = new Color(0, 0, 0, 0);
		Vector3 teleportOffset =  new Vector3(7, 0, 0);
		
		GD.Print("Trying to load room: ", roomName);
		UnloadCurrentRoom();
		var newRoom = gameNode.GetNode<Node3D>(roomName);
		if (newRoom != null)  {
			//GD.Print("Successfully found room: ", roomName);
			newRoom.Visible = true;
			newRoom.ProcessMode = Node.ProcessModeEnum.Inherit;
			
			var newRoomTeleporter = newRoom.GetNode<Area3D>("roomTeleportHitbox/Area3D");
				//GD.Print("entering from left");
				player.GlobalPosition = newRoomTeleporter.GlobalPosition;
			
			 
			newRoomTeleporter.Monitoring = true;
	} 
	else { 
			//GD.Print("Failed to find room: ", roomName);
			//GD.Print("Available rooms:");
	}
}
	
	private Node3D GetCurrentRoom() {
		foreach (Node3D child in gameNode.GetChildren()) {
			if (child.Visible && child.Name.ToString().StartsWith("Room")) {
				return child;
			}
		}
		return null;
	}
	
	public void _on_area_3d_body_entered(Node3D body) {
	if (body == player && !isEntering && !string.IsNullOrEmpty(roomPath)) {
		isEntering = true;
		lastRoomName = GetCurrentRoom()?.Name.ToString() ?? "";
		blackScreen.Play("fade_in_black_screen");
		GetTree().CreateTimer(1.1f).Timeout += () => {
		LoadRoom(roomPath);
		blackScreen.Play("fade_out_black_screen");
		var camera = gameNode.GetNode<Camera3D>("Camera3D") as CameraFriend;
		camera.Position = new Vector3(player.Position.X, player.Position.Y + camera.cameraYOffset, 25.0f);
		};
		
		
		
	}
}
	public void _on_area_3d_body_exited(Node3D body) {
			//GD.Print("roomPath: ", roomPath);
		 	currentRoomName = GetCurrentRoom()?.Name.ToString() ?? "";
		 
			if (currentRoomName != lastRoomName) {
				blackScreen.ClearQueue();
				isEntering = false;
				roomPath = lastRoomName;
			}
		
	}
		
}
