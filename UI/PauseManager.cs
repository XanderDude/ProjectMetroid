using Godot;
using System;

public partial class PauseManager : CanvasLayer {
	// Primary Goal for this script: Do not hardcode anything and export as much as possible
	[Export] public string pauseButton { get; set; } = "ui_cancel";
	[Export] public Color overlayColor { get; set; } = new Color(0, 0, 0, 0.5f);
	[Export] public string pauseText = "";
	[Export] public int fontSize { get; set; } = 48;
	[Export] public Color textColor { get; set; } = Colors.White;
	[Export] public float fadeSpeed { get; set; } = 5.0f;
	
	private ColorRect _overlay;
	private Label _pauseLabel;
	private bool _isPaused = false;
	private Tween _fadeTween;
	
	public override void _Ready() {
		Layer = 100; // top layer
		GD.Print($"pauseText: {pauseText}");
		if (string.IsNullOrEmpty(pauseText)) {
			pauseText = "PAUSED";
		}
		_overlay = new ColorRect {
			Color = new Color(overlayColor.R, overlayColor.G, overlayColor.B, 0.0f),
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		_overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		AddChild(_overlay);
		
		_pauseLabel = new Label {
			Text = pauseText,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Modulate = new Color(textColor.R, textColor.G, textColor.B, 0.0f)
		};
		
		_pauseLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		_pauseLabel.AddThemeStyleboxOverride("normal", new StyleBoxEmpty());
		_pauseLabel.AddThemeFontSizeOverride("font_size", fontSize);
		_pauseLabel.Text = pauseText;
		
		AddChild(_pauseLabel);
		
		Visible = false;
		ProcessMode = Node.ProcessModeEnum.Always;
		
		SetAllNodesPausable();
	}
	
	public override void _Process(double delta) {
		_pauseLabel.Text = pauseText;
	}
	
	public void SetAllNodesPausable() {
		var sceneRoot = GetTree().CurrentScene;
		if (sceneRoot != null) {
			SetNodeAndChildrenPausable(sceneRoot);
		}
	}
	
	private void SetNodeAndChildrenPausable(Node node) {
		if (node == this || IsDescendantOf(node, this)) {
			return;
		}
		
		if (node.ProcessMode == Node.ProcessModeEnum.Inherit) {
			node.ProcessMode = Node.ProcessModeEnum.Pausable;
		}
		
		foreach (Node child in node.GetChildren()) {
			SetNodeAndChildrenPausable(child);
		}
	}
	
	private bool IsDescendantOf(Node node, Node ancestor) {
		Node current = node;
		while (current != null) {
			if (current == ancestor) {
				return true;
			}
			current = current.GetParent();
		}
		return false;
	}
	
	public override void _UnhandledInput(InputEvent @event) {
		if (@event.IsActionPressed(pauseButton)) {
			TogglePause();
			GetViewport().SetInputAsHandled();
		}
	}
	
	public void TestPause() {
		TogglePause();
	}
	
	public void TogglePause() {
		if (_isPaused) {
			UnpauseGame();
		} else {
			PauseGame();
		}
	}
	
	public void PauseGame() {
		if (_isPaused) return;
		
		_isPaused = true;
		GetTree().Paused = true;
		Visible = true;
		
		FadeIn();
		EmitSignal(SignalName.GamePaused);
	}
	
	public void UnpauseGame() {
		if (!_isPaused) return;
		
		_isPaused = false;
		
		FadeOut(() => {
			GetTree().Paused = false;
			Visible = false;
		});
		
		EmitSignal(SignalName.GameUnpaused);
	}
	
	private void FadeIn() {
		if (_fadeTween != null && _fadeTween.IsValid()) {
			_fadeTween.Kill();
		}
		
		_fadeTween = CreateTween();
		_fadeTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_fadeTween.SetParallel(true);
		
		_fadeTween.TweenProperty(_overlay, "color:a", overlayColor.A, 1.0f / fadeSpeed);
		_fadeTween.TweenProperty(_pauseLabel, "modulate:a", textColor.A, 1.0f / fadeSpeed);
	}
	
	private void FadeOut(System.Action onComplete = null) {
		if (_fadeTween != null && _fadeTween.IsValid()) {
			_fadeTween.Kill();
		}
		
		_fadeTween = CreateTween();
		_fadeTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_fadeTween.SetParallel(true);
		
		_fadeTween.TweenProperty(_overlay, "color:a", 0.0f, 1.0f / fadeSpeed);
		_fadeTween.TweenProperty(_pauseLabel, "modulate:a", 0.0f, 1.0f / fadeSpeed);
		
		if (onComplete != null) {
			_fadeTween.Finished += () => {
				onComplete();
			};
		}
	}
	
	public bool IsPaused => _isPaused;
	
	[Signal] public delegate void GamePausedEventHandler();
	[Signal] public delegate void GameUnpausedEventHandler();
	
	public void SetPauseText(string text) {
		pauseText = text;
		if (_pauseLabel != null)
			_pauseLabel.Text = text;
	}
	
	public void SetOverlayColor(Color color) {
		overlayColor = color;
		if (_overlay != null && !_isPaused)
			_overlay.Color = new Color(color.R, color.G, color.B, 0.0f);
	}
	
	public void SetTextColor(Color color) {
		textColor = color;
		if (_pauseLabel != null && !_isPaused)
			_pauseLabel.Modulate = new Color(color.R, color.G, color.B, 0.0f);
	}
}
