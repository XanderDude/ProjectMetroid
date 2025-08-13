using Godot;

public partial class HudManager : CanvasLayer
{
	[Export] public PauseManager pauseManager; // Reference to pause manager
	
	private Label arrowCountLabel;
	private Timer updateTimer;
	
	public override void _Ready()
	{
		
		// Create arrow count display
		arrowCountLabel = new Label();
		arrowCountLabel.Text = "0";
		arrowCountLabel.Position = new Vector2(20, 30);
		arrowCountLabel.AddThemeColorOverride("font_color", Colors.White);
		arrowCountLabel.AddThemeColorOverride("font_shadow_color", Colors.Black);
		arrowCountLabel.AddThemeConstantOverride("shadow_offset_x", 1);
		arrowCountLabel.AddThemeConstantOverride("shadow_offset_y", 1);
		arrowCountLabel.AddThemeFontSizeOverride("font_size", 18);
		AddChild(arrowCountLabel);
		
		// Update every 100ms
		updateTimer = new Timer();
		updateTimer.WaitTime = 0.1f;
		updateTimer.Timeout += UpdateArrowCount;
		updateTimer.Autostart = true;
		AddChild(updateTimer);
	}
	
	private void UpdateArrowCount()
	{
		if (pauseManager == null) return;
		
		int totalArrows = GetArrowCount();
		arrowCountLabel.Text = $"{totalArrows}";
	}
	
	private int GetArrowCount()
	{
		// Get the arrow inventory from pause manager
		if (pauseManager._inventory == null) return 0;
		
		var arrowInventory = pauseManager._inventory.FindChild("Arrows", true, false) as Inventory;
		if (arrowInventory == null) return 0;
		
		int total = 0;
		for (int i = 0; i < arrowInventory.inventorySize; i++)
		{
			var item = arrowInventory.GetInventoryItem(i);
			if (item != null)
			{
				total += item.Qty;
			}
		}
		
		return total;
	}
}
