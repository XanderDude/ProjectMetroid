using Godot;
using System;

public partial class MainMenuUi : CanvasLayer
{
	
public void _on_play_pressed() {
	GetTree().ChangeSceneToFile("res://Environment/Scenes/TexturesAndLighting.tscn");
}

public void _on_options_pressed() {
	
}

public void _on_quit_pressed() {
	GetTree().Quit();
	
}
}
