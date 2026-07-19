using Godot;
using System;
using System.Collections;

public partial class GameFriend : Node3D
{

    [Signal] public delegate void ChangingRoomsEventHandler();
    public static GameFriend gameinstance; 
    public InventoryFriend inventoryfriend; 
    public SaveFriend savefriend;

    public Raven raven;
    public PlayerManager player;
    public DebugFriend debugfriend;

    public SubViewportContainer svc;
    public SubViewport svp;
    [Export] public bool initdebugfriend = false;


    private const string PLAYER_SCENE_PATH = "res://Player/Player.tscn";
    [Export] private Node3D playerNode = null;
    private const string ROOMFRIEND_SCENE_PATH = "res://Friends/RoomFriend/RoomFriend.tscn";
    private const string DEBUGFRIEND_SCENE_PATH = "res://Friends/DebugFriend/DebugFriend.tscn";
    private const string RAVEN_SCENE_PATH = "res://Raven/Raven.tscn";

    public override void _Ready()
    {
        gameinstance = this;
        init_filter();
        init_player("Player", PLAYER_SCENE_PATH, "Player");

        init_inventoryfriend();
        init_savefriend();
        if (initdebugfriend == true) init_debugfriend("DebugFriend", DEBUGFRIEND_SCENE_PATH, "DebugFriend");
    }

    
    private void init_filter()
    {
        
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


    private void init_player(string name, string path, string rootname)
    {
        if (playerNode != null) player = playerNode as PlayerManager;
        else
        {
            init_scene(name, path);
            player = svp.GetNode<PlayerManager>(rootname);
        }
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
