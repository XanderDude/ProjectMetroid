using Godot;
using System;
using System.Diagnostics;
public partial class Door : Node3D
{
  enum DoorOrientation { Left, Right, Up, Down }
  [Export] DoorOrientation orientation = DoorOrientation.Up;
  [Export] public int DoorNumber = 0; 
  


  public override void _Ready()
    {
  

    }

  public override void _PhysicsProcess(double delta)
    {

      
    }
  

    public void _on_door_transport_body_entered(Node3D body)
    {
        var rc = GetTree().GetCurrentScene().GetNode<RoomFriend>("RoomFriend");
       
          rc.currentDoorNumber = DoorNumber;
          rc.isTransitioning = true; 
          //GD.Print("Player entered door number: " + DoorNumber);
         // GD.Print("RC current door number set to: " + rc.currentDoorNumber);
    }

    public void _on_door_transport_body_exited(Node3D body)
    {
        var rc = GetTree().GetCurrentScene().GetNode<RoomFriend>("RoomFriend");
        
            rc.isTransitioning = false; 
            //GD.Print("Player exited door number: " + DoorNumber);
    }


}
