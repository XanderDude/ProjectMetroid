using Godot;
using System;
using System.Collections;

public partial class GameFriend : Node3D
{
	public static GameFriend gameinstance;
	public SaveFriend savefriend;
	public DebugFriend debugfriend;

	public SubViewportContainer svc;
	public SubViewport svp;

	[Export] public bool initdebugfriend = false;

	private const string DEBUGFRIEND_SCENE_PATH = "res://Friends/DebugFriend/DebugFriend.tscn";

	public override void _Ready()
	{
		gameinstance = this;
		init_filter();
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

	private void init_savefriend()
	{
	   savefriend = new SaveFriend();
	}
}
