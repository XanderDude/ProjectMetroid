using Godot;

public partial class HudManager : CanvasLayer
{
	[Export] public PlayerManager player; // Direct reference to player
	
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
		
		updateTimer.Autostart = true;
		AddChild(updateTimer);
	}
	
	
}
