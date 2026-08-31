using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading.Tasks;



public partial class RoomFriend : Node3D
{
	//Need these to use Room Friend
	public string roomfolder = "";
	public int currentDoorNumber = 0;

	//Random pointers / checks 
	public bool isTransitioning = false;

	public double transitionCooldown = 0.0;
	private const double COOLDOWN_TIME = 0.5;
	
	public Node3D currentRoomScene; 
	public string currentRoomName = "";

	public Dictionary<int, string> tab1 = new Dictionary<int, string>();
	public Dictionary<int, string> tab2 = new Dictionary<int, string>();

	public int roomCount = 0;
	
	//Fade transition
	[Export] public float fadeDuration = 0.35f;
	[Export] public float doorEntryOffset = 1.25f;
	[Export] private Godot.TextureRect fadeRect;

	public void init_roomfriend(string initRoomPath)
	{
		roomfolder = initRoomPath.GetBaseDir();
		room_table_init(tab1, tab2);
		room_init(initRoomPath.GetFile().GetBaseName());
	
	}

	public override void _Process(double delta)
	{
		if (!isTransitioning && transitionCooldown > 0)
		{
			transitionCooldown -= delta;
			isTransitioning = false;
		}
		
		if (isTransitioning && transitionCooldown <= 0)
		{
			TeleportToDoor(currentDoorNumber);

			GD.Print("Transitioning to door number: " + currentDoorNumber);
			GD.Print("Current room: " + currentRoomName);

			transitionCooldown = COOLDOWN_TIME; // set cooldown
		}
	}
		
	public void room_init(string name)
	{
		if (currentRoomScene != null) 
		{
			room_del();
			obj_del();
		}
		currentRoomName = name;
		currentRoomScene = ResourceLoader.Load<PackedScene>(roomfolder + "/" + name + ".tscn").Instantiate() as Node3D;
		AddChild(currentRoomScene);
		GameFriend.gameinstance.player.GlobalPosition = currentRoomScene.GlobalPosition;
		if (GameFriend.gameinstance.raven != null)
		{
			GameFriend.gameinstance.raven.GlobalPosition = GameFriend.gameinstance.player.GlobalPosition + new Godot.Vector3(0, 1.5f, 0);
		}
	}

	public void room_del()
	{
		currentRoomScene.Free();
	}

	public void obj_del()
	{
		var children = GameFriend.gameinstance.svp.GetChildren();
		foreach (Node child in children)
		{
			if (child is NormalArrow arrow)
			{
				arrow.QueueFree();
			}
		}
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
			if (!dir.CurrentIsDir() && fileName.EndsWith(".tscn"))
			{
				roomFiles.Add(fileName.TrimSuffix(".tscn"));
				roomCount++;
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();

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

	private List<Node3D> GetAllDoorsInRoom(Node3D room)
	{
		var doors = new List<Node3D>();
		FindDoorsRecursive(room, doors);
		return doors;
	}

	private void FindDoorsRecursive(Node node, List<Node3D> doors)
	{
		
		var variant = node.Get("DoorNumber");
		if (variant.VariantType != Variant.Type.Nil && variant.ToString() != "0")
		{
			doors.Add((Node3D)node);
		}
		foreach (Node child in node.GetChildren())
		{
			
			FindDoorsRecursive(child, doors);
		}
	}
	
	public async Task TeleportToDoor(int doorID)
	{

		var player = GameFriend.gameinstance.player;
		player.Velocity = new Godot.Vector3(0,0,0);
		player.LockMovement(true);

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
			await FadeToBlack();
			Engine.TimeScale = 0;
			room_init(targetRoom);
		}
		else
		{
			GD.PrintErr("cant load the room");
			isTransitioning = false;
			return;
		}

		var doors = GetAllDoorsInRoom(currentRoomScene);
		foreach (var door in doors)
		{
			int dID = (int)door.Get("DoorNumber");
			if (dID == doorID)
			{
				player.GlobalPosition = door.GlobalPosition;
				if (GameFriend.gameinstance.raven != null) 
					GameFriend.gameinstance.raven.GlobalPosition = GameFriend.gameinstance.player.GlobalPosition + new Godot.Vector3(0, 1.5f, 0);;
				break;	
			}
		}

		Engine.TimeScale = 1;
		await Task.Delay(500);
		await FadeFromBlack();
		player.LockMovement(false);
		isTransitioning = false;
	}

	private void AddFadeLayer()
	{
		if (fadeRect != null) return;
		CreateFadeLayer();
		AddChild(fadeRect);
	}
	private void CreateFadeLayer()
	{
		fadeRect = new Godot.TextureRect();
		fadeRect.Modulate = new Color(Colors.Black, 0);
	}
	private async Task FadeToBlack()
	{
		if (fadeRect == null) return;
		var tween = CreateTween().SetIgnoreTimeScale();
		tween.TweenProperty(fadeRect, "modulate:a", 1f, fadeDuration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private async Task FadeFromBlack()
	{
		if (fadeRect == null) return;
		var tween = CreateTween().SetIgnoreTimeScale().SetEase(Tween.EaseType.In);
		tween.TweenProperty(fadeRect, "modulate:a", 0f, fadeDuration*.75);
		await ToSignal(tween, Tween.SignalName.Finished);
	}	
}

		

		

		

		

		

