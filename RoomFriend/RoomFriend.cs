using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;



	public partial class RoomFriend : Node3D
	{


		//Need these to use Room Friend
		[Export] public string roomfolder = "RoomFriend";

		[Export] public string roomprefix = "mr";


		public int currentDoorNumber = 0;

		public CharacterBody3D player;

		[Export] public NodePath playerpath = "/root/Game/Player";


		//Random pointers / checks 
		public bool isTransitioning = false;

		private double transitionCooldown = 0.0;
		private const double COOLDOWN_TIME = 0.5;
		
		public Node3D currentRoomScene; 
		public string currentRoomName = "";

		public Dictionary<int, string> tab1 = new Dictionary<int, string>();
    	public Dictionary<int, string> tab2 = new Dictionary<int, string>();

		public int roomCount = 0;

		
		[Export] public PackedScene initialRoom; 

		

		public override void _Ready()
		{
			player = GetNode<PlayerManager>(playerpath);
			player = GetNode<CharacterBody3D>(playerpath);
        
			room_table_init(tab1, tab2);
			currentRoomName = System.IO.Path.GetFileNameWithoutExtension(initialRoom.ResourcePath);

			string fullPath = initialRoom.ResourcePath;
			room_init(System.IO.Path.GetFileNameWithoutExtension(fullPath));
		}

		

		public override void _Process(double delta)
		{
			if (transitionCooldown > 0)
			{
				transitionCooldown -= delta;
			}
			
			if (isTransitioning && transitionCooldown <= 0)
			{
				TeleportToDoor(currentDoorNumber);
				isTransitioning = false;
				transitionCooldown = COOLDOWN_TIME; // Start cooldown
			}
		}

		

			
		public void room_init(string name)
    	{
			if (currentRoomScene != null) 
			{
				room_del();
				room_player_animation();
			}
			currentRoomName = name;
			currentRoomScene = ResourceLoader.Load<PackedScene>(roomfolder + "/" + name + ".tscn").Instantiate() as Node3D;
			CallDeferred("add_child", currentRoomScene);
			
			
			
    	}

		public void room_del()
    	{
			currentRoomScene.Free();
    	}

		public void room_player_animation()
		{
			player.GlobalPosition = new Godot.Vector3(0, 0, 0); // Default position if door not found
		}

		public int room_table_init(Dictionary<int, string> tab1, Dictionary<int, string> tab2)
		{
			
			//find folder using folder path from origin
			//find every PackedScene with the given prefix
			//count every room that has been found
			//iterate through every room i < roomCount
			//Add All Door ID's to the table (1 room might have 2 places in the table if it has 2 different room keys), Door IDs are located on a Door class on an Area3D 
			//if the door ID is already in table 1, then add the door ID to table 2 
			//The idea is on teleport -> Check other table -> teleport to the scene in the other table 

			List<string> roomFiles = ScanRoomFolder();

			foreach (var roomName in roomFiles)
			{
				ProcessRoomDoors(roomName, tab1, tab2);
			}



			PrintTableSummary(tab1, tab2);
			return roomCount;
		}


	// You can ignore everything below this line ------------------------------------------- [Table Functions] 

		private List<string> ScanRoomFolder()
		{
			var roomFiles = new List<string>();
			roomCount = 0;
			
			var dir = DirAccess.Open(roomfolder);
			if (dir == null)
			{
				//GD.PrintErr($"Failed to open directory: {roomfolder}");
				return roomFiles;
			}
			
			dir.ListDirBegin();
			var fileName = dir.GetNext();
			
			while (fileName != "")
			{
				if (!dir.CurrentIsDir() && fileName.StartsWith(roomprefix) && fileName.EndsWith(".tscn"))
				{
					roomFiles.Add(fileName.TrimSuffix(".tscn"));
					roomCount++;
				}
				fileName = dir.GetNext();
			}
			dir.ListDirEnd();
			
			//GD.Print($"Found {roomCount} rooms with prefix '{roomprefix}'");
			return roomFiles;
		}

		 private void ProcessRoomDoors(string roomName, Dictionary<int, string> tab1, Dictionary<int, string> tab2)
		{
			
			var roomScene = ResourceLoader.Load<PackedScene>($"{roomfolder}/{roomName}.tscn");
			if (roomScene == null)
			{
				//GD.PrintErr($"Failed to load room: {roomName}");
				return;
			}
			
			var roomInstance = roomScene.Instantiate() as Node3D;
			
			
			var doors = GetAllDoorsInRoom(roomInstance);
			
		
			foreach (var door in doors)
			{
				int doorID = (int)door.Get("DoorNumber");
				AddDoorToTable(doorID, roomName, tab1, tab2);
			}
			
		
			roomInstance.Free();
		}

		private void AddDoorToTable(int doorID, string roomName, Dictionary<int, string> tab1, Dictionary<int, string> tab2)
		{
			if (!tab1.ContainsKey(doorID))
			{
				tab1[doorID] = roomName;
				//GD.Print($"Added door {doorID} to tab1 (Room: {roomName})");
			}
			else
			{
				tab2[doorID] = roomName;
				//GD.Print($"Added door {doorID} to tab2 (Room: {roomName}) - links to {tab1[doorID]}");
			}
		}

		private List<Node> GetAllDoorsInRoom(Node3D room)
		{
			var doors = new List<Node>();
			FindDoorsRecursive(room, doors);
			return doors;
		}

		private void FindDoorsRecursive(Node node, List<Node> doors)
		{
			
			var variant = node.Get("DoorNumber");
			if (variant.VariantType != Variant.Type.Nil)
			{
				doors.Add(node);
			}
			foreach (Node child in node.GetChildren())
			{
				FindDoorsRecursive(child, doors);
			}
		}

		 private void PrintTableSummary(Dictionary<int, string> tab1, Dictionary<int, string> tab2)
		{
			//GD.Print($"Table 1 has {tab1.Count} doors, Table 2 has {tab2.Count} doors");
			//GD.Print($"Total rooms scanned: {roomCount}");
		}
		
		public void TeleportToDoor(int doorID)
		{
			string targetRoom = "";
			
			if (tab1.ContainsKey(doorID) && tab1[doorID] != currentRoomName)
			{
				targetRoom = tab1[doorID];
			}
			else if (tab2.ContainsKey(doorID) && tab2[doorID] != currentRoomName)
			{
				targetRoom = tab2[doorID];
			}
			
			
			
			if (targetRoom != "")
			{
				room_init(targetRoom);
			}
			else
			{
				GD.PrintErr($"Door {doorID} not found or has no destination");
			}
		}
}

		

		

		

		

		

