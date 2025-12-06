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
    var rc = GetTree().GetCurrentScene().GetNode<RoomFriend>("RoomFriend");
    rc.isTransitioning = false;
  }
  
  public override void _PhysicsProcess(double delta)
    {
      
    }
  
    public void _on_door_transport_body_entered(Node3D body)
    {
        var rc = GetTree().GetCurrentScene().GetNode<RoomFriend>("RoomFriend");
              rc.currentDoorNumber = DoorNumber;
              if (rc.transitionCooldown <= 0) rc.isTransitioning = true; 
              

    }
    
    public void _on_door_transport_body_exited(Node3D body)
    {
        
    }
    
}