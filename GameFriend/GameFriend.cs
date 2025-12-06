using Godot;
using System;
using System.Collections;

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

    public SubViewportContainer svc;
    public SubViewport svp;

    [Export] public bool initroomfriend = false;
    [Export] public bool initdebugfriend = false;

    private const string CAMERA_SCENE_PATH = "res://Camera/Camera.tscn";
    private const string PLAYER_SCENE_PATH = "res://Player/Player.tscn";
    private const string ROOMFRIEND_SCENE_PATH = "res://RoomFriend/RoomFriend.tscn";
    private const string DEBUGFRIEND_SCENE_PATH = "res://DebugFriend/DebugFriend.tscn";

    private const string RAVEN_SCENE_PATH = "res://Raven/Raven.tscn";

    private const string FILTER_SCENE_PATH = "res://svgstuff.tscn";
    public override void _Ready()
    {
        gameinstance = this;
        init_filter("SubViewportContainer", FILTER_SCENE_PATH);
        init_player("Player", PLAYER_SCENE_PATH, "Player");
        init_camera("Camera", CAMERA_SCENE_PATH, "Camera");
        init_inventoryfriend();
        init_savefriend();
        if (initroomfriend == true) init_roomfriend("RoomFriend", ROOMFRIEND_SCENE_PATH, "RoomFriend", "res://Environment/Levels/Dungeon_WallJumpUnlock/wj6_WJUnlock.tscn", "Environment/Levels/Dungeon_WallJumpUnlock", "wj");
        if (initdebugfriend == true) init_debugfriend("DebugFriend", DEBUGFRIEND_SCENE_PATH, "DebugFriend");
    }

    
    private void init_filter(string name, string path)
    {
        
        var instantiator = GD.Load<PackedScene>(path).Instantiate();
        if (instantiator == null) GD.PrintErr("[GameFriend]Cant find path for: " + name);
        instantiator.Name = name;
        AddChild(instantiator); 
        
        svc = GetNode<SubViewportContainer>("SubViewportContainer");
        svp = svc.GetNode<SubViewport>("SubViewport");
    }
    private void init_scene(string name, string path)
    {
        var instantiator = GD.Load<PackedScene>(path).Instantiate();
        if (instantiator == null) GD.PrintErr("[GameFriend]Cant find path for: " + name);
        instantiator.Name = name;
        svp.AddChild(instantiator);
    }

   private void init_debugfriend(string name, string path, string rootname)
    {
        init_scene(name, path);
        debugfriend = svp.GetNode<DebugFriend>(rootname);
        debugfriend.init_debugfriend();
    }

    private void init_roomfriend(string name, string path, string rootname, string initroom, string roomfolder, string roomprefix)
    {
        init_scene(name, path);
        roomfriend = svp.GetNode<RoomFriend>(rootname);
        var instantiator = GD.Load<PackedScene>(initroom);
        roomfriend.init_roomfriend(roomfolder, roomprefix, instantiator);
    }


    private void init_camera(string name, string path, string rootname)
    {
        init_scene(name, path);
        camera = svp.GetNode<CameraFriend>(rootname);
        camera.init_camera();
    }

    private void init_player(string name, string path, string rootname)
    {
          init_scene(name, path);
          player = svp.GetNode<PlayerManager>(rootname);          

    
    }

    private void init_raven(string name, string path, string rootname)
    {
        var instantiator = GD.Load<PackedScene>(path).Instantiate();
        if (instantiator == null) GD.PrintErr("[GameFriend]Cant find path for: " + name);
        instantiator.Name = name;
        player.AddChild(instantiator);
        raven = player.GetNode<Raven>(rootname);
        raven.TopLevel = true;
        raven.init_raven();
        GD.Print (raven.Name);
    }

    private void init_savefriend()
    {
       savefriend = new SaveFriend();
    }

    private void init_inventoryfriend()
    {
        inventoryfriend = new InventoryFriend();
    }


    private bool ravenunlocked = false;
    public override void _Process(double delta)
    {

            
                if (GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("ravenSlash") && !ravenunlocked)
                {
                    init_raven("Raven", RAVEN_SCENE_PATH, "Raven");
                    ravenunlocked = true;
                }
                else if (!GameFriend.gameinstance.inventoryfriend.isUpgradeUnlocked("ravenSlash") && ravenunlocked)
                {
                    raven.QueueFree();
                    raven = null;
                    ravenunlocked = false;
                    
                }
            
        
    }
}
