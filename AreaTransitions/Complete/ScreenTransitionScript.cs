using Godot;
using System;


public partial class ScreenTransitionScript : Godot.TextureRect
{
	[Export] public string roomPath = "";
	private AnimationPlayer blackScreen;
	private Area3D collider;
	private bool isEntering = false;
	private Node3D player; 
	private Node3D gameNode;
	
	public override void _Ready() {
		blackScreen = GetNode<AnimationPlayer>("%AnimationPlayer"); 
		collider = GetNode<Area3D>("%Area3D");
		gameNode = GetNode<Node3D>("/root/Game");
		player = gameNode.GetNode<Node3D>("Player");
		
		var currentRoomName = GetParent().GetParent().GetParent().Name; 
		collider.Monitoring = (currentRoomName == "Room1");
	
}
	
	private bool isEnterFromLeft() {
		return collider.GlobalPosition.X > player.GlobalPosition.X;
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
		GD.Print("Trying to load room: ", roomName);
		UnloadCurrentRoom();
		var newRoom = gameNode.GetNode<Node3D>(roomName);
		if (newRoom != null)  {
			GD.Print("Successfully found room: ", roomName);
			newRoom.Visible = true;
			newRoom.ProcessMode = Node.ProcessModeEnum.Inherit;
		
			var targetTeleporter = newRoom.GetNode<Node3D>("roomTeleportHitbox");
				player.GlobalPosition = targetTeleporter.GlobalPosition + new Vector3(7, 15, 0);
				
		
			var newRoomArea = newRoom.GetNode<Area3D>("roomTeleportHitbox/Area3D");
			newRoomArea.Monitoring = true;
	} 
	else { 
			GD.Print("Failed to find room: ", roomName);
			GD.Print("Available rooms:");
	}
}
	
	private Node3D GetCurrentRoom() {
		foreach (Node3D child in gameNode.GetChildren()) {
			if (child.Visible) {
				return child;
			}
		}
		return null;
	}
	
	public void _on_area_3d_body_entered(Node3D body) {
		
	if (body == player && !isEntering && !string.IsNullOrEmpty(roomPath)) {
		isEntering = true;
		
		blackScreen.Play("fade_in_black_screen");
		GetTree().CreateTimer(1.1f).Timeout += () => {
		blackScreen.Play("fade_out_black_screen");
		GD.Print("Playing Fade Out");
		GD.Print("Loading Room");
		LoadRoom(roomPath);
		GD.Print("Setting Modulate to 0");
		
		GD.Print("Getting Camera Node");
		var camera = gameNode.GetNode<Camera3D>("Camera3D") as Camera3d;
		camera.Position = new Vector3(player.Position.X, player.Position.Y + camera.cameraYOffset, 25.0f);
		};
		
	}
}
	public void _on_area_3d_body_exited(Node3D body) {
		GD.Print("I Exited");
		isEntering = false;
		blackScreen.ClearQueue();
		
	}
		
}
