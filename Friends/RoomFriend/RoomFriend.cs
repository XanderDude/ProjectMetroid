using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

// Autoloaded singleton - see project.godot [autoload]. Lives at the scene tree root, not
// inside any particular GameFriend's SubViewport, so anything it shows (fade overlay, room
// content) is explicitly parented into GameFriend.gameinstance.svp rather than into itself.
public partial class RoomFriend : Node3D
{
	public static RoomFriend instance;
	private const string LEVEL_FOLDER = "res://Environment/Levels/Dungeon_WallJumpUnlock";

	// Nodes GameFriend actually manages under the viewport; anything else (leftover static
	// content authored directly into a test scene, e.g. navtesting.tscn's nav-mesh sandbox)
	// gets trashed on the first real room load so it can't sit overlapping the dungeon.
	private static readonly HashSet<string> ManagedViewportNodeNames = new() { "Raven","Player", "DebugFriend" , "Camera3D", "WorldEnvironment", "DirectionalLight_Exterior_DayBreak" };

	//Fade transition
	[Export] public float fadeDuration = 0.35f;
	[Export] public float doorEntryOffset = 1.25f;
	[Export] private Godot.TextureRect fadeRect;

	//Random pointers / checks
	public bool isTransitioning = false;
	private bool levelStarted = false;

	public Node3D currentRoomScene;
	public string currentRoomName = "";
	private string currentRoomPath = "";

	// Values are full res:// paths (rooms can now live in subfolders of LEVEL_FOLDER).
	public Dictionary<int, string> tab1 = new Dictionary<int, string>();
	public Dictionary<int, string> tab2 = new Dictionary<int, string>();

	private List<string> allRoomPaths = new();

	public override void _Ready()
	{
		instance = this;
	}

	// Deliberately does nothing at launch - no table building, no trashing, no room loading.
	// Deferred to the first real room load (a door, or a debug click) via EnsureLevelStarted().
	public void EnsureLevelStarted()
	{
		if (levelStarted) return;
		levelStarted = true;

		room_table_init();
		TrashUnmanagedViewportContent();
		EnsureFadeLayerInViewport();

		if (fadeRect == null) GD.PushError("RoomFriend.fadeRect is not assigned (CanvasLayer/TextureRect NodePath) - fades will be skipped.");
	}

	public void TrashUnmanagedViewportContent()
	{
		foreach (Node child in GameFriend.gameinstance.svp.GetChildren())
		{
			if (!ManagedViewportNodeNames.Contains(child.Name)) child.QueueFree();
		}
	}

	// RoomFriend itself lives outside the SubViewport (it's an autoload), but its fade
	// overlay needs to render inside it like everything else the player sees.
	private void EnsureFadeLayerInViewport()
	{
		if (fadeRect == null) return;
		Node canvasLayer = fadeRect.GetParent();
		if (canvasLayer == null || canvasLayer.GetParent() == GameFriend.gameinstance.svp) return;

		canvasLayer.GetParent()?.RemoveChild(canvasLayer);
		GameFriend.gameinstance.svp.AddChild(canvasLayer);
	}

	public async void TeleportToDoor(int doorID)
	{
		if (isTransitioning) return;
		isTransitioning = true;
		EnsureLevelStarted();

		var pm = GameFriend.gameinstance.player;
		pm.Velocity = Vector3.Zero;
		pm.LockMovement(true);

		string targetRoom = ResolveTargetRoom(doorID);
		if (targetRoom == "")
		{
			GD.PrintErr($"[RoomFriend] No connecting room found for door {doorID}");
			pm.LockMovement(false);
			isTransitioning = false;
			return;
		}

		await FadeToBlack();
		LoadRoom(targetRoom);


		Door matchedDoor = FindDoorByNumber(currentRoomScene, doorID);
		if (matchedDoor != null)
		{
			pm.GlobalPosition = matchedDoor.GlobalPosition + matchedDoor.GetEntryOffset(doorEntryOffset);
			pm.StateMachine.TransitionTo("knockbackState");
			GameFriend.gameinstance.EmitSignal(GameFriend.SignalName.ChangingRooms);
		}
		

		await FadeFromBlack();

		pm.LockMovement(false);
		isTransitioning = false;
	}

	private string ResolveTargetRoom(int doorID)
	{
		
		if (tab1.ContainsKey(doorID) && tab1[doorID] != currentRoomPath) return tab1[doorID];
		if (tab2.ContainsKey(doorID) && tab2[doorID] != currentRoomPath) return tab2[doorID];
		return "";
	}

	private Door FindDoorByNumber(Node3D room, int doorID)
	{
		foreach (var doorNode in GetAllDoorsInRoom(room))
		{
			if (doorNode is Door door && door.DoorNumber == doorID) return door;
		}
		return null;
	}

	private Door FindAnyDoor(Node3D room)
	{
		foreach (var doorNode in GetAllDoorsInRoom(room))
		{
			if (doorNode is Door door) 
			{
				
				return door;
			}
		}
		return null;
	}

	// Debug-only jump: instant, no fade, drops the player near a door instead of the room's
	// local origin (which isn't necessarily clear space and can land you in the floor).
	// Takes a bare room name (e.g. from a save file) and finds it anywhere under
	// DUNGEON_FOLDER, including subfolders.
	public void DebugTeleportToRoom(string name)
	{
		EnsureLevelStarted();
		string path = allRoomPaths.Find(p => System.IO.Path.GetFileNameWithoutExtension(p) == name);
		if (path == null)
		{
			GD.PrintErr($"[RoomFriend] No room named '{name}' found under {LEVEL_FOLDER}");
			return;
		}
		DebugLoadRoom(path);
	}

	public void DebugLoadRoom(string resPath)
	{
		EnsureLevelStarted();
		LoadRoom(resPath);
		var pm = GameFriend.gameinstance.player;
		Door anyDoor = FindAnyDoor(currentRoomScene);
		pm.GlobalPosition = anyDoor != null ? anyDoor.GlobalPosition + anyDoor.GetEntryOffset(doorEntryOffset) : Vector3.Zero;
	}

	public void LoadRoom(string resPath)
	{
		var instantiated = ResourceLoader.Load<PackedScene>(resPath).Instantiate() as Node3D;
		instantiated = ExtractRoomContent(instantiated);
		GameFriend.gameinstance.svp.AddChild(instantiated);

		if (currentRoomScene != null)
		{
			currentRoomScene.QueueFree();
			obj_del();
		}
		currentRoomScene = instantiated;
		currentRoomPath = resPath;
		currentRoomName = System.IO.Path.GetFileNameWithoutExtension(resPath);

	}

	// Some scenes (e.g. abandonedsewer_blockout) are authored as standalone levels with their
	// own GameFriend/SubViewportContainer/SubViewport, not as bare rooms. Adding one of those
	// as-is nests a second SubViewport inside the real one - nothing renders where expected and
	// the player ends up floating with no geometry under it. If we find an inner SubViewport,
	// pull its children out into a plain wrapper and use that instead of the whole scene.
	private Node3D ExtractRoomContent(Node3D instantiated)
	{
		SubViewport innerViewport = FindDescendantSubViewport(instantiated);
		if (innerViewport == null) return instantiated;

		var content = new Node3D { Name = instantiated.Name };
		foreach (Node child in innerViewport.GetChildren())
		{
			innerViewport.RemoveChild(child);
			content.AddChild(child);
		}
		instantiated.Free();
		return content;
	}

	private SubViewport FindDescendantSubViewport(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is SubViewport sv) return sv;
			SubViewport found = FindDescendantSubViewport(child);
			if (found != null) return found;
		}
		return null;
	}

	// wj0-wj7 (and similar) rooms carry their own Player (which now brings its own Camera
	// along as a child) for standalone testing; strip it here, before the room ever enters
	// the tree, so real gameplay never ends up with duplicates of either. Free() (not
	// QueueFree) matters: this runs before the room is added to the tree, so a deferred free
	// would still let the test Camera enter the tree and steal "current" for a frame with
	// nothing to hand it back to.

	private async Task FadeToBlack()
	{
		if (fadeRect == null) return;
		var tween = CreateTween();
		tween.TweenProperty(fadeRect, "modulate:a", 1f, fadeDuration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private async Task FadeFromBlack()
	{
		if (fadeRect == null) return;
		var tween = CreateTween();
		tween.TweenProperty(fadeRect, "modulate:a", 0f, fadeDuration);
		await ToSignal(tween, Tween.SignalName.Finished);
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

	// ------------------------------------------------------------------------------- [Table Functions]
	// Builds tab1/tab2: for each door ID, which room(s) it appears in. A door ID normally
	// shows up in exactly two rooms (the two sides of the connection) - tab1 holds the
	// first room seen for that ID, tab2 the second.

	public void room_table_init()
	{
		allRoomPaths.Clear();
		ScanRoomFolderRecursive(LEVEL_FOLDER, allRoomPaths);

		foreach (var roomPath in allRoomPaths)
		{
			ProcessRoomDoors(roomPath);
		}
	/*
		//Print WHole Table
		GD.Print("[RoomFriend] Door table built:");
		foreach (var kvp in tab1)
		{
			string room1 = kvp.Value;
			string room2 = tab2.ContainsKey(kvp.Key) ? tab2[kvp.Key] : "(none)";
			GD.Print($"  Door {kvp.Key}: {room1} <-> {room2}");
		}
		*/
	}

	// Every .tscn under DUNGEON_FOLDER counts as a room, including ones nested in subfolders.
	private void ScanRoomFolderRecursive(string path, List<string> results)
	{
		var dir = DirAccess.Open(path);
		if (dir == null) return;

		dir.ListDirBegin();
		var entry = dir.GetNext();
		while (entry != "")
		{

			string fullPath = $"{path}/{entry}";
			if (dir.CurrentIsDir())
			{
				ScanRoomFolderRecursive(fullPath, results);
			}
			else if (entry.EndsWith(".tscn"))
			{
				results.Add(fullPath);
			}
			
			entry = dir.GetNext();
		}
		dir.ListDirEnd();
	}

	private void ProcessRoomDoors(string roomPath)
	{
		var roomScene = ResourceLoader.Load<PackedScene>(roomPath);
		if (roomScene == null)
		{
			return;
		}

		var roomInstance = roomScene.Instantiate() as Node3D;

		foreach (var door in GetAllDoorsInRoom(roomInstance))
		{
			int doorID = (int)door.Get("DoorNumber");
			AddDoorToTable(doorID, roomPath);
		}

		roomInstance.Free();
	}

	private void AddDoorToTable(int doorID, string roomPath)
	{
		if (!tab1.ContainsKey(doorID))
		{
			tab1[doorID] = roomPath;
		}
		else if (!tab2.ContainsKey(doorID))
		{
			tab2[doorID] = roomPath;
		}
		else
		{
			GD.PushError($"[RoomFriend] Door {doorID} appears in more than two rooms: {tab1[doorID]}, {tab2[doorID]}, {roomPath}");
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
}