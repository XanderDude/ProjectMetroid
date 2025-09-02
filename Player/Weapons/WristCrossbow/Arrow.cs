using Godot;
using System;

public partial class Arrow : Node
{
    [Export] public float speed;
    [Export] public AudioStream shootSound;
    [Export] public AudioStream hitSound;
    [Export] public float damage;
    [Export] public PackedScene arrowScene;
	

}
