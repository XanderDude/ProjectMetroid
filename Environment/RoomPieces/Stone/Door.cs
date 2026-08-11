using Godot;
using System;
using System.Diagnostics;

public partial class Door : Node3D
{
  public enum DoorOrientation { Left, Right, UpRight, UpLeft, Down }

  public enum DoorType { Normal, Open }

  public enum DoorKey { None, BombArrow, RavenSlash, RavenTeleport }
  [Signal] public delegate void DoorOpenedEventHandler();
  [Export] public DoorOrientation player_orientation = DoorOrientation.UpRight;
  [Export] public int DoorNumber = 0;

  [Export] public DoorKey keyType = DoorKey.BombArrow;


  [Export] public DoorType doorType = DoorType.Normal;


  public Area3D doorTransportArea => GetNode<Area3D>("DoorTransport");
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
       bool correctKey = keyType switch
    {
        DoorKey.BombArrow => body is BombArrowProjectile,
        DoorKey.None => body is NormalArrow,
        _ => false
    };
    if (correctKey)
    {
        EmitSignal(SignalName.DoorOpened);
        CallDeferred(MethodName.DoorEnabled, true);
    }
  }
  public void _on_physical_door_area_3d_area_entered(Area3D area)
  {
      bool correctKey = keyType switch
      {
          DoorKey.RavenSlash => area.GetParent() is RavenSlash,
          _ => false
      };
      GD.Print($"Area: {area}");
      if (correctKey)
      {
          EmitSignal(SignalName.DoorOpened);
          CallDeferred(MethodName.DoorEnabled, true);
      }
  }

  

}