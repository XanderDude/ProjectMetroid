using Godot;

public partial class PauseManager : CanvasLayer 
{
	[Export] public PackedScene pauseMenuScene; // The pause menu/UI scene
	[Export] public PlayerManager player; // Reference to player for inventory access
	
	public bool _isPaused = false;
	private Node _pauseMenu;
	private Node _currentInventoryDisplay;
	
	public override void _Ready() 
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		Visible = false;
	}
	
	public override void _Input(InputEvent @event) 
	{
		if (@event.IsActionPressed("ui_cancel")) 
		{
			if (_isPaused) 
			{
				UnpauseGame();
			} 
			else 
			{
				PauseGame();
			}
		}
	}
	
	private void PauseGame()
	{
		// Instantiate pause menu if needed
		if (pauseMenuScene != null)
		{
			if (_pauseMenu == null)
			{
				_pauseMenu = pauseMenuScene.Instantiate();
				_pauseMenu.ProcessMode = Node.ProcessModeEnum.Always;
			}
			AddChild(_pauseMenu);
		}
		
		// Add player inventory to the pause menu for display
		if (player != null)
		{
			var inventory = player.GetInventory();
			if (inventory != null)
			{
				// Remove from any previous parent
				if (inventory.GetParent() != null)
				{
					inventory.GetParent().RemoveChild(inventory);
				}
				
				// Try to find an inventory container in the pause menu, or add directly
				Node inventoryContainer = _pauseMenu?.FindChild("InventoryContainer", true, false);
				if (inventoryContainer != null)
				{
					inventoryContainer.AddChild(inventory);
				}
				else
				{
					// Fallback: add directly to pause manager
					AddChild(inventory);
				}
				_currentInventoryDisplay = inventory;
			}
		}
		
		Visible = true;
		GetTree().Paused = true;
		_isPaused = true;
	}
	
	private void UnpauseGame()
	{
		// Remove inventory from display but DON'T remove from scene tree completely
		if (_currentInventoryDisplay != null && _currentInventoryDisplay.GetParent() != null)
		{
			_currentInventoryDisplay.GetParent().RemoveChild(_currentInventoryDisplay);
			_currentInventoryDisplay = null;
		}
		
		// Remove pause menu from display but keep it in memory
		if (_pauseMenu != null && _pauseMenu.GetParent() != null)
		{
			RemoveChild(_pauseMenu);
			// Don't set _pauseMenu to null - keep it for reuse
		}
		
		Visible = false;
		GetTree().Paused = false;
		_isPaused = false;
	}
	
	// Public getter for other systems that need inventory access
	public Node GetCurrentInventory()
	{
		return player?.GetInventory();
	}
}
