using Godot;
using System;

public partial class GameFriend : Node3D
{
    public static GameFriend gameinstance; 
    public InventoryFriend inventoryfriend; 
    public SaveFriend savefriend;
    public RoomFriend roomfriend;
    public Raven raven;
    public PlayerManager player;
    public CameraFriend camera;
    public DebugFriend debugfriend;


    private const string CAMERA_SCENE_PATH = "res://Camera/Camera.tscn";
    private const string PLAYER_SCENE_PATH = "res://Player/Player.tscn";
    private const string ROOMFRIEND_SCENE_PATH = "res://RoomFriend/RoomFriend.tscn";

    private const string DEBUGFRIEND_SCENE_PATH = "res://DebugFriend/DebugFriend.tscn";
    public override void _Ready()
    {
        gameinstance = this;
        init_player("Player", PLAYER_SCENE_PATH, "Player");
        init_camera("Camera", CAMERA_SCENE_PATH, "Camera");
        init_inventoryfriend();
        init_savefriend();
        init_roomfriend("RoomFriend", ROOMFRIEND_SCENE_PATH, "RoomFriend", "res://Environment/Levels/Dungeon_WallJumpUnlock/wj6_WJUnlock.tscn", "Environment/Levels/Dungeon_WallJumpUnlock", "wj");
        init_debugfriend("DebugFriend", DEBUGFRIEND_SCENE_PATH, "DebugFriend");
    }

    private void init_scene(string name, string path)
    {
        var instantiator = GD.Load<PackedScene>(path).Instantiate();
        if (instantiator == null) GD.PrintErr("[GameFriend]Cant find path for: " + name);
        instantiator.Name = name;
        AddChild(instantiator);
    }

   private void init_debugfriend(string name, string path, string rootname)
    {
        init_scene(name, path);
        debugfriend = GetNode<DebugFriend>(rootname);
        debugfriend.init_debugfriend();
    }

    private void init_roomfriend(string name, string path, string rootname, string initroom, string roomfolder, string roomprefix)
    {
        init_scene(name, path);
        roomfriend = GetNode<RoomFriend>(rootname);
        var instantiator = GD.Load<PackedScene>(initroom);
        roomfriend.init_roomfriend(roomfolder, roomprefix, instantiator);
    }


    private void init_camera(string name, string path, string rootname)
    {
        init_scene(name, path);
        camera = GetNode<CameraFriend>(rootname);
        camera.init_camera();
    }

    private void init_player(string name, string path, string rootname)
    {
          init_scene(name, path);
          player = GetNode<PlayerManager>(rootname);
          raven = player.GetNode<Raven>("Raven");
          GD.Print (raven.Name);
          raven.init_raven();

    
    }

    private void init_savefriend()
    {
       savefriend = new SaveFriend();
    }

    private void init_inventoryfriend()
    {
        inventoryfriend = new InventoryFriend();
    }


    public override void _Process(double delta)
    {

        if (raven != null)
        {
            if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("ravenSlash"))
            {
                raven.ProcessMode = ProcessModeEnum.Inherit;
                raven.Visible = true;
            }
            else
            {
                raven.ProcessMode = ProcessModeEnum.Disabled;
                raven.Visible = false;
            }
        }

        
    }
}
