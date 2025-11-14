using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class aggroState : State
{
    private float outOfRangeTimer = 0f;
	private float outOfRangeDelay = 2f;
	private float jumpForce = 0f;
	private float edgeTurnCooldown = 0f;  
    private const float EDGE_TURN_DELAY = 3.0f; 
    public override void Enter()
    {
        GD.Print("Entered Aggro State");
        ec.speed = ec.runspeed;
        outOfRangeTimer = 0f;

		if (ec.ray != null)
		{
			ec.ray.Enabled = true;

		}
		
    }
    
    public override void Exit()
    {
        if (ec.ray != null)
        {
            ec.ray.Enabled = false;
        }
        outOfRangeTimer = 0f;
    }
    
    public override void Update(float delta)
    {
        // Check if player is out of range
        if (!ec.isPlayerInRange(ec.detectionrange))
        {
            outOfRangeTimer += delta;
            
            if (outOfRangeTimer >= outOfRangeDelay)
            {
                
                esm.TransitionTo("patrolState");
            }
        }
        else
        {
            outOfRangeTimer = 0f;
        }
    }
    
   public override void PhysicsUpdate(float delta)
    {
        if (ec.player != null)
        {
            UpdateRaycastDirection();
            
            float directionToPlayer = Mathf.Sign(ec.player.GlobalPosition.X - ec.GlobalPosition.X);
            bool shouldJump = ShouldJumpOverObstacle();
            float verticalVelocity = ec.Velocity.Y + ec.gravity * delta;
            
            // Decrease cooldown timer
            if (edgeTurnCooldown > 0)
            {
                edgeTurnCooldown -= delta;
            }

			// JUMP CASE
			if (ec.ray.IsColliding() && shouldJump && ec.IsOnFloor() && !isUnderObstacle(ec.ray))
			{
				verticalVelocity = jumpForce;

				ec.Velocity = new Godot.Vector3(
					directionToPlayer * ec.speed,
					verticalVelocity,
					0
				);

			
				edgeTurnCooldown = 0f;  // Reset cooldown
			}
			// UNDER OBSTACLE CASE
			else if (ec.ray.IsColliding() && isUnderObstacle(ec.ray) && ec.IsOnFloor())
			{
				bool tryingToGoRight = isUnderObstacleLeadingDirection(ec.ray);

				// If moving LEFT and hit edge, turn around (only if cooldown expired)
				if (!tryingToGoRight && isHitEdge(ec.edgeray) && edgeTurnCooldown <= 0)
				{
					// Turn around and go right
					ec.Velocity = new Godot.Vector3(
						Mathf.Abs(ec.speed),
						verticalVelocity,
						0
					);
					edgeTurnCooldown = EDGE_TURN_DELAY;  // Start cooldown
					
				}
				else if (tryingToGoRight)
				{
					// Move right
					ec.Velocity = new Godot.Vector3(
						Mathf.Abs(ec.speed),
						verticalVelocity,
						0
					);
				}
				else
				{
					// Move left (but only if not in cooldown)
					if (edgeTurnCooldown <= 0)
					{
						ec.Velocity = new Godot.Vector3(
							-Mathf.Abs(ec.speed),
							verticalVelocity,
							0
						);
					}
					else
					{
						// Still in cooldown - keep moving right
						ec.Velocity = new Godot.Vector3(
							Mathf.Abs(ec.speed),
							verticalVelocity,
							0
						);
					}
				}
			}
			//JUMP ACROSS GAP CASE
			else if (isHitEdge(ec.edgeray) && ec.IsOnFloor())
			{
				verticalVelocity = Mathf.Sqrt(2 * Mathf.Abs(ec.gravity) * ec.jumpmaxheight);

				ec.Velocity = new Godot.Vector3(
					directionToPlayer * ec.speed,
					verticalVelocity,
					0
				);

				
				edgeTurnCooldown = 0f;  // Reset cooldown
			}
			// NORMAL CASE
			else
			{
				ec.Velocity = new Godot.Vector3(
					directionToPlayer * ec.speed,
					verticalVelocity,
					0
				);

				edgeTurnCooldown = 0f;  // Reset when not under obstacle
			}
        }
    }

	private bool ShouldJumpOverObstacle()
	{

		ec.ray.ForceRaycastUpdate();

		bool isColliding = ec.ray.IsColliding();

		if (isColliding)
		{
			var collider = ec.ray.GetCollider() as Node3D;
			var boxShape = (BoxShape3D)collider.GetChild<CollisionShape3D>(0).Shape;

			// Don't jump if we hit the player or wall (layer 2)
			if (collider == ec.player || (collider is PhysicsBody3D body && body.CollisionLayer == 2))
			{
				return false;
			}

			float obstacleHeight = collider.GlobalPosition.Y + (boxShape.Size.Y / 2) - ec.GlobalPosition.Y;
			if (ec.jumpmaxheight > obstacleHeight && ec.player.GlobalPosition.Y - ec.GlobalPosition.Y > 0.0f)
			{
				
				jumpForce = Mathf.Sqrt(2 * Mathf.Abs(ec.gravity) * (obstacleHeight + 5.0f));
				return true;
			}
		}

		return false;
	}

	private bool isUnderObstacle(RayCast3D ray)
{
    // Check if ray exists
    if (ray == null)
    {
        return false;
    }

    // Check if ray is colliding
    if (!ray.IsColliding())
    {
        return false;
    }

    // Check if collider exists
    var collider = ray.GetCollider() as Node3D;
    if (collider == null)
    {
        return false;
    }

    // Check if collider has children
    if (collider.GetChildCount() == 0)
    {
        return false;
    }

    // Check if first child is a CollisionShape3D
    var collisionShape = collider.GetChild<CollisionShape3D>(0);
    if (collisionShape == null)
    {
        return false;
    }

    // Check if shape exists and is a BoxShape3D
    if (collisionShape.Shape == null || collisionShape.Shape is not BoxShape3D)
    {
        return false;
    }

    var boxShape = (BoxShape3D)collisionShape.Shape;

    float obstacleRight = collider.GlobalPosition.X + (boxShape.Size.X / 2);
    float obstacleLeft = collider.GlobalPosition.X - (boxShape.Size.X / 2);

    if (ec.GlobalPosition.X <= obstacleRight + 1.0f && ec.GlobalPosition.X >= obstacleLeft - 1.0f)
    {
        float obstacleTop = collider.GlobalPosition.Y + (boxShape.Size.Y / 2);
        if (ec.GlobalPosition.Y < obstacleTop)
        {
            return true;
        }
    }
    
    return false;
}

	private bool isUnderObstacleLeadingDirection(RayCast3D ray)
	{
		var collider = ec.ray.GetCollider() as Node3D;
		var boxShape = (BoxShape3D)collider.GetChild<CollisionShape3D>(0).Shape;

		float obstacleRight = collider.GlobalPosition.X + (boxShape.Size.X / 2);
		float obstacleLeft = collider.GlobalPosition.X - (boxShape.Size.X / 2);

		if (ec.player.GlobalPosition.X <= obstacleRight + 1.0f && ec.player.GlobalPosition.X >= collider.GlobalPosition.X)
		{
			return true; //right side
		}
		else if (ec.player.GlobalPosition.X >= obstacleLeft - 1.0f && ec.player.GlobalPosition.X <= collider.GlobalPosition.X)
		{
			return false; //left side
		}
		return false;
	}
	
	private bool isHitEdge(RayCast3D ray)
	{
		if (ray == null)
		{
		
			return false;
		}
		
		bool hitEdge = !ray.IsColliding();
	
		return hitEdge;
	}
    private void UpdateRaycastDirection()
    {
       
        Godot.Vector3 rayStart = ec.ray.GlobalPosition;
        Godot.Vector3 playerPos = ec.player.GlobalPosition;
        
        
        Godot.Vector3 worldDirection = (playerPos - rayStart).Normalized();
        
       
        Godot.Vector3 localDirection = ec.ray.GlobalTransform.Basis.Inverse() * worldDirection;
        
        
        float distance = rayStart.DistanceTo(playerPos);
        
        ec.ray.TargetPosition = localDirection * distance;
        
       
    }
}