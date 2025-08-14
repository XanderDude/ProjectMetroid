// Simple pause handler - put this on your MAIN SCENE (not in subviewport)
using Godot;

public partial class GameManager : Node
{
	[Export] public PlayerManager player;
	[Export] public PackedScene pauseMenuScene;
	
	private bool isPaused = false;
	private CanvasLayer pauseLayer;
	private Node pauseMenu;
	
	public override void _Ready()
	{
		// Create pause layer
		pauseLayer = new CanvasLayer();
		pauseLayer.ProcessMode = Node.ProcessModeEnum.Always;
		pauseLayer.Visible = false;
		AddChild(pauseLayer);
		
		// Ensure this can process when paused
		ProcessMode = Node.ProcessModeEnum.Always;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			GD.Print($"ESC pressed! Paused: {isPaused}");
			
			if (isPaused)
			{
				Unpause();
			}
			else
			{
				Pause();
			}
			
			GetViewport().SetInputAsHandled();
		}
	}
	
	private void Pause()
	{
		GD.Print("PAUSING");
		
		// Show pause menu
		if (pauseMenuScene != null && pauseMenu == null)
		{
			pauseMenu = pauseMenuScene.Instantiate();
			pauseMenu.ProcessMode = Node.ProcessModeEnum.Always;
		}
		
		if (pauseMenu != null && pauseMenu.GetParent() == null)
		{
			pauseLayer.AddChild(pauseMenu);
		}
		
		// Show inventory
		if (player != null)
		{
			var inventory = player.GetInventory();
			if (inventory != null && inventory.GetParent() == null)
			{
				pauseLayer.AddChild(inventory);
			}
		}
		
		pauseLayer.Visible = true;
		
		// Pause the main tree
		GetTree().Paused = true;
		
		// Find and pause the specific SubViewport structure
		var subViewportContainer = GetTree().GetFirstNodeInGroup("world")?.FindChild("SubViewportContainer", true, false);
		if (subViewportContainer == null)
		{
			// Search everywhere for SubViewportContainer
			subViewportContainer = FindNodeByName("SubViewportContainer");
		}
		
		if (subViewportContainer != null)
		{
			var subViewport = subViewportContainer.FindChild("SubViewport", false, false) as SubViewport;
			if (subViewport != null)
			{
				subViewport.ProcessMode = Node.ProcessModeEnum.Disabled;
				GD.Print("Paused SubViewport in SubViewportContainer");
			}
			else
			{
				GD.Print("SubViewport not found in SubViewportContainer");
			}
		}
		else
		{
			GD.Print("SubViewportContainer not found");
		}
		
		isPaused = true;
		GD.Print("PAUSED!");
	}
	
	private void Unpause()
	{
		GD.Print("UNPAUSING");
		
		// Sync player inventory before hiding it
		if (player != null)
		{
			GD.Print("Calling player.SyncWithInventory()");
			player.SyncWithInventory();
		}
		else
		{
			GD.Print("Player is null - cannot sync");
		}
		
		// Hide inventory
		if (player != null)
		{
			var inventory = player.GetInventory();
			if (inventory != null && inventory.GetParent() != null)
			{
				inventory.GetParent().RemoveChild(inventory);
			}
		}
		
		// Hide pause menu
		if (pauseMenu != null && pauseMenu.GetParent() != null)
		{
			pauseLayer.RemoveChild(pauseMenu);
		}
		
		pauseLayer.Visible = false;
		
		// Unpause the main tree
		GetTree().Paused = false;
		
		// Find and unpause the specific SubViewport structure
		var subViewportContainer = GetTree().GetFirstNodeInGroup("world")?.FindChild("SubViewportContainer", true, false);
		if (subViewportContainer == null)
		{
			// Search everywhere for SubViewportContainer
			subViewportContainer = FindNodeByName("SubViewportContainer");
		}
		
		if (subViewportContainer != null)
		{
			var subViewport = subViewportContainer.FindChild("SubViewport", false, false) as SubViewport;
			if (subViewport != null)
			{
				subViewport.ProcessMode = Node.ProcessModeEnum.Always;
				GD.Print("Unpaused SubViewport in SubViewportContainer");
			}
		}
		
		isPaused = false;
		GD.Print("UNPAUSED!");
	}
	
	private Node FindNodeByName(string name)
	{
		return FindNodeByNameRecursive(GetTree().CurrentScene, name);
	}
	
	private Node FindNodeByNameRecursive(Node node, string name)
	{
		if (node.Name == name)
			return node;
			
		foreach (Node child in node.GetChildren())
		{
			var found = FindNodeByNameRecursive(child, name);
			if (found != null)
				return found;
		}
		
		return null;
	}
}
