using Godot;
using System;

public partial class MainMenuUi : CanvasLayer
{
[Export] Godot.AudioStreamPlayer titleMP3;
AnimationPlayer fogAnimation;

public override void _Ready() {
	titleMP3 = GetNode<Godot.AudioStreamPlayer>("%titleMusic");
	fogAnimation = GetNode<AnimationPlayer>("%titleFog");
	fogAnimation.Play("fog_animation");
	titleMP3.Play();
}
	
public void _on_play_pressed() {
	GetTree().ChangeSceneToFile("res://Environment/Scenes/TexturesAndLighting.tscn");
}

public void _on_options_pressed() {
	
}

public void _on_quit_pressed() {
	GetTree().Quit();
	
}
}
