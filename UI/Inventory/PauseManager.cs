using Godot;

public partial class PauseManager : CanvasLayer {
	[Export] public PackedScene inventoryScene;
	
	private bool _isPaused = false;
	private Node _inventory;
	
	public override void _Ready() {
		ProcessMode = Node.ProcessModeEnum.Always;
		Visible = false;
	}
	
	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("ui_cancel")) {
			if (_isPaused) {
				// Remove inventory and unpause
				if (_inventory != null && _inventory.GetParent() != null) {
					RemoveChild(_inventory);
				}
				Visible = false;
				GetTree().Paused = false;
				_isPaused = false;
			} else {
				// Create/add inventory and pause
				if (inventoryScene != null) {
					if (_inventory == null) {
						_inventory = inventoryScene.Instantiate();
						_inventory.ProcessMode = Node.ProcessModeEnum.Always;
					}
					AddChild(_inventory);
				}
				Visible = true;
				GetTree().Paused = true;
				_isPaused = true;
			}
		}
	}
}
