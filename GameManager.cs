
using Godot;

public partial class GameManager : Node
{
	[Export] public PlayerManager player;
	
	private bool isPaused = false;
	
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
	}

	public override void _PhysicsProcess(double delta)
	{
		
    }

	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("Pause"))
		{

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
	
		
	
		if (player != null)
		{
			player.ShowInventory();
			//GD.Print("Player inventory shown");
		}
		
	
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
				
			}
			else
			{
			
			}
		}
		else
		{
		
		}
		
		isPaused = true;
		
	}
	
	private void Unpause()
	{
		
		
		
		if (player != null)
		{
			
			player.SyncWithInventory();
			
			player.HideInventory();
			
		}
		else
		{
			//GD.Print("Player is null - cannot sync");
		}
		
	
		GetTree().Paused = false;
		
	
		var subViewportContainer = GetTree().GetFirstNodeInGroup("world")?.FindChild("SubViewportContainer", true, false);
		if (subViewportContainer == null)
		{
		
			subViewportContainer = FindNodeByName("SubViewportContainer");
		}
		
		if (subViewportContainer != null)
		{
			var subViewport = subViewportContainer.FindChild("SubViewport", false, false) as SubViewport;
			if (subViewport != null)
			{
				subViewport.ProcessMode = Node.ProcessModeEnum.Always;
				//GD.Print("Unpaused SubViewport in SubViewportContainer");
			}
		}
		
		isPaused = false;
		//GD.Print("UNPAUSED!");
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
