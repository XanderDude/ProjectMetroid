
using Godot;

public partial class GameManager : Node
{
	[Export] public PlayerManager player;
	
	private bool isPaused = false;
	
	public override void _Ready()
	{
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
		
		// Show player's inventory
		if (player != null)
		{
			player.ShowInventory();
			GD.Print("Player inventory shown");
		}
		
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
			
			// Hide player's inventory
			player.HideInventory();
			GD.Print("Player inventory hidden");
		}
		else
		{
			GD.Print("Player is null - cannot sync");
		}
		
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
