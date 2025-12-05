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


    private const string CAMERA_SCENE_PATH = "res://Camera/Camera.tscn";
    public override void _Ready()
    {
        gameinstance = this;
        init_camera();
        init_player();
        raven = GetNode<Raven>("%Raven");
        inventoryfriend = new InventoryFriend();
        savefriend = new SaveFriend();
        roomfriend = GetNode<RoomFriend>("RoomFriend");
    }


    private void init_camera()
    {
        var instantiator = GD.Load<PackedScene>(CAMERA_SCENE_PATH);
        Node instance = instantiator.Instantiate();
        instance.Name = "Camera";
        AddChild(instance);
        camera = GetNode<CameraFriend>("Camera");
        camera.init_camera();
    }

    private void init_savefriend()
    {
        savefriend = GetNode<SaveFriend>("SaveFriend");
        if (savefriend == null) GD.PrintErr("SaveFriend null");
    }



    private void init_player()
    {
          player = GetNode<PlayerManager>("%Player");
          if (player == null) GD.PrintErr("Player null");

    
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
