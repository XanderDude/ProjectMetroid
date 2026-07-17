using Godot;
using System;
public partial class Water : Node3D
{



    public void _on_area_3d_body_entered(Node3D body)
    {
       PlayerManager.instance.GetNode<Node>("MovementStateMachine").GetNode<groundedState>("groundedState").groundMaxSpeed -= 2.0f;

       
    }

     public void _on_area_3d_body_exited(Node3D body)
    {
       PlayerManager.instance.GetNode<Node>("MovementStateMachine").GetNode<groundedState>("groundedState").groundMaxSpeed += 2.0f;
     
    }

}
