using Godot;
using System;
public partial class Room1 : State
{

    public override void Enter()
    {
       rc.InstantiateRoomScene("Room1");
       GD.Print("Entered Room 1!");}

    public override void Exit()
    {
        if (rc.currentDoorNumber == 2)
            {
                //Position player at door 2 location
                rosm.TransitionTo("Room2");
            }
           
            
    }





}
