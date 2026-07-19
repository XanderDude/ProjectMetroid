using Godot;
using System;
using System.Diagnostics;

public partial class Door : Node3D
{
  public enum DoorOrientation { Left, Right, UpRight, UpLeft, Down }

  [Signal] public delegate void DoorOpenedEventHandler();
  [Export] public DoorOrientation player_orientation = DoorOrientation.UpRight;
  [Export] public int DoorNumber = 0;

  [Export] Node3D key;

  public enum DoorType { Normal, Locked, Open};

  [Export] public DoorType doorType = DoorType.Normal;

  public Area3D doorTransportArea => GetNode<Area3D>("DoorTransport");

  // Direction to nudge an arriving player so they land clear of this door's trigger area
  // instead of standing inside it (which would immediately re-fire the transition, causing
  // an infinite reload loop). Most doors in existing rooms never had orientation configured
  // (it was unused before this) and every door that IS configured across the whole dungeon
  // is Left or Right - Up/Down are never intentionally authored, just the enum default. So
  // Up must never push down into the floor, but it also can't be a zero offset (that leaves
  // the player standing in the trigger). Default to a horizontal nudge instead.
  public Vector3 GetEntryOffset(float distance)
  {
    return player_orientation switch
    {
      DoorOrientation.Left => new Vector3(-distance, 0, 0),
      DoorOrientation.Down => new Vector3(0, -distance * 2f, 0),
      DoorOrientation.Right => new Vector3(distance, 0, 0),
      DoorOrientation.UpLeft => new Vector3(-distance, distance * 2f, 0),
      DoorOrientation.UpRight => new Vector3(distance * 2, 0, 0),
      _ => new Vector3(distance, 0, 0)
    };
  }

  public override void _Ready()
  {
    if (doorType == DoorType.Open) CallDeferred("DoorEnabled", true);
    else CallDeferred("DoorEnabled", false);
  }

  public void DoorEnabled(bool enabled)
  {
    doorTransportArea.Monitoring = enabled;
    doorTransportArea.Monitorable = enabled;
  }

  public void _on_door_transport_body_entered(Node3D body)
  {
    if (body is not PlayerManager) return;

    if (RoomFriend.instance.isTransitioning) return;

    if (body is PlayerManager) RoomFriend.instance.TeleportToDoor(DoorNumber);
    
  }

  public void _on_physical_door_area_3d_body_entered(Node3D body)
  {
      EmitSignal(SignalName.DoorOpened);
      if (key == body || key == null) CallDeferred("DoorEnabled", true);
  }
  public void _on_door_transport_body_exited(Node3D body)
  {

  }

}