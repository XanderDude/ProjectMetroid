using Godot;

public partial class ArrowHud : Control
{
	[Export] public Inventory inventory; // Drag your inventory node here
	[Export] public int arrowItemID = 1; // Set this to your arrow's ID
	
	private Label arrowCountLabel;
	private TextureRect arrowIconRect;
	private Timer updateTimer;
	
	public override void _Ready()
	{
		// Create the UI elements
		CreateHUD();
		
		// Set up update timer
		updateTimer = new Timer();
		updateTimer.WaitTime = 0.1f; // Update every 100ms
		updateTimer.Timeout += UpdateArrowDisplay;
		updateTimer.Autostart = true;
		AddChild(updateTimer);
		
		// Initial update
		UpdateArrowDisplay();
	}
	
	private void CreateHUD()
	{
		// Position in top-right corner
		SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopRight);
		Position = new Vector2(-100, 10);
		Size = new Vector2(90, 40);
		
		// Create horizontal container
		var hbox = new HBoxContainer();
		hbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		AddChild(hbox);
		
		// Arrow icon
		arrowIconRect = new TextureRect();
		arrowIconRect.CustomMinimumSize = new Vector2(32, 32);
		arrowIconRect.ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional;
		arrowIconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		hbox.AddChild(arrowIconRect);
		
		// Arrow count label
		arrowCountLabel = new Label();
		arrowCountLabel.Text = "0";
		arrowCountLabel.AddThemeColorOverride("font_color", Colors.White);
		arrowCountLabel.AddThemeFontSizeOverride("font_size", 18);
		arrowCountLabel.VerticalAlignment = VerticalAlignment.Center;
		hbox.AddChild(arrowCountLabel);
	}
	
	private void UpdateArrowDisplay()
	{
		if (inventory == null)
		{
			Visible = false;
			return;
		}
		
		// Find the first arrow item to steal icon and get total count
		Texture2D arrowIcon = null;
		int totalArrows = 0;
		
		for (int i = 0; i < inventory.inventorySize; i++)
		{
			var item = inventory.GetInventoryItem(i);
			if (item != null && item.ID == arrowItemID)
			{
				// Steal the icon from the first arrow we find
				if (arrowIcon == null)
				{
					arrowIcon = item.Icon;
				}
				
				// Add to total count
				totalArrows += item.Qty;
			}
		}
		
		// Update display
		if (totalArrows > 0 && arrowIcon != null)
		{
			arrowIconRect.Texture = arrowIcon;
			arrowCountLabel.Text = totalArrows.ToString();
			Visible = true;
		}
		else
		{
			Visible = false;
		}
	}
}
