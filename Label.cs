using Godot;
using System;

public partial class Label : Godot.Label
{
	public override void _Ready()
	{
	    Text = "Hello, World";
	}

	public override void _Process(double delta)
	{
		
	}
}
