using Godot;
using System;

public partial class Alex : Node
{

    public override void _Ready()
    {
        GD.Print("Alex is ready!");
    }

    public void Greet()
    {
        GD.Print("Hello from Alex!");
    }
}
