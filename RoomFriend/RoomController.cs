using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;



	public partial class RoomController : Node3D
	{
		RoomStateMachine rosm;	

		public Node3D currentRoomScene;

		[Export] public string roomfolder = "";

		public byte currentDoorNumber = 0;

		public CharacterBody3D player;

		[Export] public NodePath playerpath;

		public bool isTransitioning = false;
		
		


		public override async void _Ready()
		{

			rosm = GetNode<RoomStateMachine>("RoomStateMachine");
			if (rosm == null)
			{
				GD.Print("RoomStateMachine not found!");
			}
			player = GetNode<PlayerManager>(playerpath);
			if (player == null)
			{
				GD.Print("PlayerManager not found!");
			}
			
		}

		

		public override void _Process(double delta)
		{
			if (isTransitioning)
        	{
				GD.Print("Transitioning to room for door number: " + currentDoorNumber);
				rosm._currentState.Exit();
			}
}


		public void InstantiateRoomScene(string name)
    	{
			if (currentRoomScene != null) 
			{
				FreeRoomScene();
				PositionPlayerAtDoor();
			}
			currentRoomScene = ResourceLoader.Load<PackedScene>(roomfolder + "/" + name + ".tscn").Instantiate() as Node3D;
			//add child if godot isnt busy setting up other children
			CallDeferred("add_child", currentRoomScene);
			
			
			
    	}

		public void FreeRoomScene()
    	{
			currentRoomScene.Free();
    	}

		public void PositionPlayerAtDoor()
		{
			player.GlobalPosition = new Godot.Vector3(0, 0, 0); // Default position if door not found
		}

		

		

		

		


	}
