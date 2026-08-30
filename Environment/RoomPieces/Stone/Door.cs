using Godot;
using System;
using System.Diagnostics;

public partial class Door : Node3D
{
  enum DoorOrientation { Left, Right, Up, Down }
  [Export] DoorOrientation orientation = DoorOrientation.Up;
  [Export] public int DoorNumber = 0; //0 = door should be interactable but does not cause a room change, e.g. Raven's cell door
                                      //-1 = no door logic should run due to missing dependencies or is placeholder
  
  [Export] Node3D key;

  public enum DoorType { Normal, Locked, Open}; //Normal=Opens on interaction, Locked=Requires key or lever, Open=Just a doorway that can cause room transition

  [Export] public DoorType doorType = DoorType.Open;

  public Area3D doorTransportArea => GetNode<Area3D>("DoorTransport");
  
  
  public override void _Ready()
  {
    if (DoorNumber == 0) NoRoomChange(); 
    else if (DoorNumber == -1) ProcessMode = ProcessModeEnum.Disabled;
    else
    {
      //var rc = GameFriend.gameinstance.roomfriend;
      //rc.isTransitioning = false;
    }
  }

  private void NoRoomChange() //disables colliders meant for room changing
  {
    if (doorTransportArea != null) doorTransportArea.ProcessMode = ProcessModeEnum.Disabled;
    //var backupCol = GetNode<StaticBody3D>("BackupCollider") ?? null;
    //if (backupCol != null) backupCol.ProcessMode = ProcessModeEnum.Disabled;
  }
  
  public void _on_door_transport_body_entered(Node3D body)
  {
    if (DoorNumber > 0 && GameFriend.gameinstance != null && GameFriend.gameinstance.roomfriend != null)
    {
      var rc = GameFriend.gameinstance.roomfriend;
      rc.currentDoorNumber = DoorNumber;
      if (rc.transitionCooldown <= 0) rc.isTransitioning = true; 
    }
  }
  
  public void _on_door_transport_body_exited(Node3D body)
  {
      
  }
    
}