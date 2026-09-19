using Godot;
using System;

public partial class DebugFriend : Node3D
{

	private Label labelbombarrow => GetNode<Label>("CanvasLayer/BombArrowCount");
	private OptionButton roomteleporter => GetNode<OptionButton>("CanvasLayer/RoomTeleporter");
	private OptionButton doorid => GetNode<OptionButton>("CanvasLayer/DoorID");
	private OptionButton upgradeselectorbutton => GetNode<OptionButton>("CanvasLayer/Upgrades");
	private Godot.Button addupgradebutton => GetNode<Godot.Button>("CanvasLayer/Upgrades/AddUpgrade");

	private Godot.Button addhealthbutton => GetNode<Godot.Button>("CanvasLayer/Health/AddHealth");

	private Godot.Button goteleportbutton => GetNode<Godot.Button>("CanvasLayer/GoTeleport");

	private Godot.Button removeupgradebutton => GetNode<Godot.Button>("CanvasLayer/Upgrades/RemUpgrade");
	private Godot.Button addbombarrowbutton => GetNode<Godot.Button>("CanvasLayer/BombArrowCount/AddBombArrow");

	private Godot.Button resetbutton => GetNode<Godot.Button>("CanvasLayer/RESET");

	private Label healthlabel => GetNode<Label>("CanvasLayer/Health");
	
	private ItemDrop.Upgrade selected;

	private string selectedRoom;

	private int id;


	//Editor upgrades
	[ExportGroup("Upgrades")]
	[Export] private bool WallJump = false;
	[Export] private bool RavenSlash = false;
	[Export] private bool Mantling = false;
	[Export] private bool ChargeShot = false;
	[Export] private bool SlideBoost = false;
	[Export] private bool BombArrows = false;
	[Export] private bool RavenTeleport = false;


	public override void _Ready()
	{
		//have to use this because otherwise roomfriend loads before this 
		CallDeferred(nameof(init));
	}


	public void init()
	{
		roomteleporter.AddItem("Select Room");
		if (GameFriend.gameinstance.roomfriend != null)
		{
			foreach (string name in GameFriend.gameinstance.roomfriend.tab1.Values)
			{
			if (!OptionHasText(roomteleporter, name))
				roomteleporter.AddItem(name);
			}
			foreach (string name in GameFriend.gameinstance.roomfriend.tab2.Values)
			{
			if (!OptionHasText(roomteleporter, name))
				roomteleporter.AddItem(name);
			}
			roomteleporter.ItemSelected += OnDropdownRoomSelected;
	}
		
		
		

		foreach (string name in Enum.GetNames(typeof(ItemDrop.Upgrade)))
		{
			upgradeselectorbutton.AddItem(name);
		}
		upgradeselectorbutton.ItemSelected += OnDropdownItemSelected;

		addupgradebutton.Pressed += OnAddUpgradePressed;
		removeupgradebutton.Pressed += OnRemoveUpgradePressed;
		addbombarrowbutton.Pressed += OnAddBombArrowPressed;
		addhealthbutton.Pressed += OnAddHealthPressed;
		goteleportbutton.Pressed += OnGoTeleportPressed;
		doorid.ItemSelected += OnDropdownDoorSelected;


	}
	private bool OptionHasText(OptionButton ob, string text)
	{
		if (string.IsNullOrEmpty(text)) return false;
		for (int i = 0; i < ob.GetItemCount(); i++)
		{
			if (string.Equals(ob.GetItemText(i), text, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public override void _Process(double delta)
	{
		healthlabel.Text = $"Health: {GameFriend.gameinstance.player.Health}";
		int bombCount = 0;
		var inventory = GameFriend.gameinstance.inventoryfriend;
		if (inventory?.consumables != null && inventory.consumables.TryGetValue("bombArrows", out var cnt))
			bombCount = cnt;
		labelbombarrow.Text = $"Bomb Arrows: {bombCount}";


		//Has to be another way to write this lol 
		if (WallJump) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade("wallJump");
		else if (!WallJump && !GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("wallJump")) GameFriend.gameinstance.inventoryfriend.RemoveUpgrade("wallJump");
		if (RavenSlash) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade("ravenSlash");
		else if (!RavenSlash && !GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("ravenSlash")) GameFriend.gameinstance.inventoryfriend.RemoveUpgrade("ravenSlash");
		if (Mantling) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade("mantling");
		else if (!Mantling && !GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("mantling")) GameFriend.gameinstance.inventoryfriend.RemoveUpgrade("mantling");
		if (ChargeShot) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade("chargeShot");
		else if (!ChargeShot && !GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("chargeShot")) GameFriend.gameinstance.inventoryfriend.RemoveUpgrade("chargeShot");
		if (SlideBoost) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade("slideBoost");
		else if (!SlideBoost && !GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("slideBoost")) GameFriend.gameinstance.inventoryfriend.RemoveUpgrade("slideBoost");
		if (BombArrows) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade("bombArrows");
		else if (!BombArrows && !GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("bombArrows")) GameFriend.gameinstance.inventoryfriend.RemoveUpgrade("bombArrows");
		if (RavenTeleport) GameFriend.gameinstance.inventoryfriend.UnlockUpgrade("ravenTeleport");
		else if (!RavenTeleport && !GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("ravenTeleport")) GameFriend.gameinstance.inventoryfriend.RemoveUpgrade("ravenTeleport");

	   
	}



	private void OnDropdownRoomSelected(long index)
	{
		doorid.Clear();
		doorid.AddItem(0.ToString());
		selectedRoom = roomteleporter.GetItemText((int)index);
		foreach (int id in GameFriend.gameinstance.roomfriend.tab1.Keys)
		{
			if (GameFriend.gameinstance.roomfriend.tab1[id] == selectedRoom)
			{
				if (!OptionHasText(doorid, id.ToString()))
					doorid.AddItem(id.ToString());
			}
		}
		foreach (int id in GameFriend.gameinstance.roomfriend.tab2.Keys)
		{
			if (GameFriend.gameinstance.roomfriend.tab2[id] == selectedRoom)
			{
				if (!OptionHasText(doorid, id.ToString()))
					doorid.AddItem(id.ToString());
			}
		}
		roomteleporter.ReleaseFocus();
		
	}

	private void OnDropdownDoorSelected(long index)
	{
		id = int.Parse(doorid.GetItemText((int)index));
		doorid.ReleaseFocus();
	}

	private void OnAddHealthPressed()
	{
		GameFriend.gameinstance.player.Health += 50;
	}

	private async void OnGoTeleportPressed()
	{
		if (GameFriend.gameinstance.roomfriend != null && selectedRoom != null && selectedRoom != "Select Room") 
		{
			GameFriend.gameinstance.roomfriend.room_init(selectedRoom);
			var doors = GameFriend.gameinstance.roomfriend.GetAllDoorsInRoom(GameFriend.gameinstance.roomfriend.currentRoomScene);
			foreach (var door in doors)
			{
				int dID = (int)door.Get("DoorNumber");
				if (dID == id)
				{
					GameFriend.gameinstance.roomfriend.teleported = true;
					GameFriend.gameinstance.player.GlobalPosition = door.GlobalPosition;
					GameFriend.gameinstance.camera.SetToDefaultBounds();
					GameFriend.gameinstance.camera.GlobalPosition = GameFriend.gameinstance.player.GlobalPosition;
					if (GameFriend.gameinstance.raven != null) 
						GameFriend.gameinstance.raven.GlobalPosition = GameFriend.gameinstance.player.GlobalPosition + new Godot.Vector3(0, 1.5f, 0);;
					break;	
				}

			}
		}
		goteleportbutton.ReleaseFocus();
	}

   private void OnAddBombArrowPressed()
	{
		//GD.Print("Adding 5 Bomb Arrows");
		GameFriend.gameinstance.inventoryfriend.AddConsumable("bombArrows", 5);

		
		
		upgradeselectorbutton.ReleaseFocus();
	}
	private void OnAddUpgradePressed()
	{
		//GD.Print($"Adding upgrade: {selected}");
		
		GameFriend.gameinstance.inventoryfriend.UnlockUpgrade(selected.ToString());
		upgradeselectorbutton.ReleaseFocus();
	}

	private void OnRemoveUpgradePressed()
	{
		//GD.Print($"Removing upgrade: {selected}");
		GameFriend.gameinstance.inventoryfriend.RemoveUpgrade(selected.ToString());
		upgradeselectorbutton.ReleaseFocus();
	}




	private void OnDropdownItemSelected(long index)
	{
		selected = (ItemDrop.Upgrade)index;
		//GD.Print($"Selected: {selected}");     
		upgradeselectorbutton.ReleaseFocus();
	}


   

}
