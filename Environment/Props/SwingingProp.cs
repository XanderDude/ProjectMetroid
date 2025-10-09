using Godot;
using System.Threading.Tasks;

public partial class SwingingProp : Node3D
{
	[Export] Node3D obj;
	[Export] float swingSpeed = 5f;
	[Export] float swingDistance = 15f;
	[Export] float rotateUpdateSpeed = .5f; //seconds
	float timer = 0;

	public override void _PhysicsProcess(double delta)
	{
		if (swingSpeed == 0) return;
		timer += (float)delta;
		if (timer >= rotateUpdateSpeed)
		{
			timer = 0;
			float newZRot = (float)Mathf.Lerp(obj.RotationDegrees.Z, swingDistance, swingSpeed);
			obj.RotationDegrees = new Vector3(0, 0, newZRot);
		}
	}

	private void RotateObject()
	{
		if (Mathf.Abs(swingDistance - obj.RotationDegrees.Z ) <= .5f)
		{
			swingDistance *= -1;
		}
		float newZRot = (float)Mathf.Lerp(obj.RotationDegrees.Z, swingDistance, swingSpeed);
		obj.RotationDegrees = new Vector3(0, 0, newZRot);
		//obj.RotationDegrees = new Vector3(0, 0, Mathf.Ceil( newZRot * 100)/100);
	}
}
