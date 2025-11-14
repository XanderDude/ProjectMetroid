using Godot;
using System;
using System.Collections;
public partial class Room2 : State
{

   
    public override void Enter()
    {
        GD.Print("Entered Room 2!");
        rc.InstantiateRoomScene("Room2");



    }
	public override void Exit()
    {

        GD.Print("Exiting Room 2");
            if (rc.currentDoorNumber == 2)
            {
                rosm.TransitionTo("Room1");
            }
        //rc.currentDoorNumber = 0;
        


    }



}
