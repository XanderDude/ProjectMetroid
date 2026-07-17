using Godot;
using System;
using System.Diagnostics;

public partial class Door : Node3D
{
  public enum DoorOrientation { Left, Right, Up, Down }
  [Export] public DoorOrientation orientation = DoorOrientation.Up;
  [Export] public int DoorNumber = 0;

  // Direction to nudge an arriving player so they land clear of this door's trigger area
  // instead of standing inside it (which would immediately re-fire the transition, causing
  // an infinite reload loop). Most doors in existing rooms never had orientation configured
  // (it was unused before this) and every door that IS configured across the whole dungeon
  // is Left or Right - Up/Down are never intentionally authored, just the enum default. So
  // Up must never push down into the floor, but it also can't be a zero offset (that leaves
  // the player standing in the trigger). Default to a horizontal nudge instead.
  public Vector3 GetEntryOffset(float distance)
  {
    return orientation switch
    {
      DoorOrientation.Left => new Vector3(-distance, 0, 0),
      DoorOrientation.Down => new Vector3(0, distance, 0),
      _ => new Vector3(distance, 0, 0), // Right, Up (default/unconfigured), and anything else
    };
  }

  public void _on_door_transport_body_entered(Node3D body)
  {
    if (body is not PlayerManager) return;

    if (RoomFriend.instance.isTransitioning) return;

    RoomFriend.instance.TeleportToDoor(DoorNumber);
  }

  public void _on_door_transport_body_exited(Node3D body)
  {

  }

}