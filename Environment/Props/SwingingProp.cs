using Godot;
using System.Threading.Tasks;

public partial class SwingingProp : Node3D
{
	[Export] Node3D obj;
	[Export] float swingSpeed = 5f;
	[Export] float swingDistance = 15f;
	float timer = 0;

	public override void _Ready()
	{
		if (swingDistance <= 0) return;
		RotateObject();
	}

	private async void RotateObject()
	{
		await Task.Delay(100);
		timer += .01f;
		if (Mathf.Abs(swingDistance - obj.RotationDegrees.Z ) <= .5f)
		{
			swingDistance *= -1;
			timer = 0;
		}
		float newZRot = (float)Mathf.Lerp(obj.RotationDegrees.Z, swingDistance, timer * Mathf.Abs(swingSpeed));
		obj.RotationDegrees = new Vector3(0, 0, Mathf.Ceil( newZRot * 10)/10);
		RotateObject();
	}
}
