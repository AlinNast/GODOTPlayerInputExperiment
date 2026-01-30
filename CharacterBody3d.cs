using Godot;
using System;

public partial class CharacterBody3d : CharacterBody3D
{
	int movementType = 0;

	public float Speed = 10.0f;
    public float MaxSpeed = 10.0f;
    public float gravityMutiplier = 2.0f;

    public const float JumpVelocity = 9.0f;

    public float Acceleration = 50.0f;
    public float Deceleration = 7.0f;
    float inputPressedTime = 0;
    bool canJump = true;
    float coyoteTime = 0;
    bool shouldDoCoyoteTime = true;
    float coyoteTimeThreshold = 0.3f;

    bool isCrouched = false;

    float jumpChargeTime = 0;
    bool isWaitingToJump = false;


    [Export]
	MeshInstance3D mesh;

    [Export]
    CollisionShape3D collisionObject;

    private static Vector3 standingScale = new Vector3(1.0f, 1.0f, 1.0f);
    private static Vector3 crouchedScale = new Vector3(1.0f, 0.7f, 1.0f);

    public override void _Ready()
    {
        base._Ready();

        
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

		if (Input.IsActionPressed("switch_input_mode_1", true))
		{
            movementType = 0;
        }
        if (Input.IsActionPressed("switch_input_mode_2", true))
        {
            movementType = 1;
        }
        if (Input.IsActionPressed("switch_input_mode_3", true))
        {
            movementType = 2;
        }
    }

    private void PerformJump()
    {
        canJump = false;
        shouldDoCoyoteTime = false;
        mesh.Scale = standingScale;
        jumpChargeTime = 0;
        isWaitingToJump = false;
        ((CapsuleShape3D)collisionObject.Shape).Height = 2.0f;

    }

  

    public override void _PhysicsProcess(double delta)
	{
		
		Vector3 velocity = Velocity;

        if (Input.IsActionJustPressed("crouch"))
        {
            isCrouched = (isCrouched) ? false : true;

            if (!isCrouched) 
            {
                Acceleration = 4.0f;
                Deceleration = 7.0f;
                Speed = 5.0f;
                MaxSpeed = 15.0f;
                mesh.Scale = standingScale;
                ((CapsuleShape3D)collisionObject.Shape).Height = 2.0f;
            }
            if (isCrouched)
            {
                Acceleration = 2.0f;
                Deceleration = 7.0f;
                Speed = 3.0f;
                MaxSpeed = 7.0f;
                shouldDoCoyoteTime = false;
                mesh.Scale = crouchedScale;
                ((CapsuleShape3D)collisionObject.Shape).Height = 1.4f;
            }
        }

        // Add the gravity.
        if (!IsOnFloor())
        {
            coyoteTime += (float)delta;
            if (coyoteTime > coyoteTimeThreshold)
            {
                shouldDoCoyoteTime = false;
            }

            if (velocity.Y > -0.1f)
            {
                velocity.Y += GetGravity().Y * gravityMutiplier * (float)delta;
            }

            if (!shouldDoCoyoteTime)
            {
                canJump = false;
                
                if (!Input.IsActionPressed("ui_accept"))
                {
                    velocity.Y += GetGravity().Y* gravityMutiplier * (float)delta;
                }
            }
        }
        else 
        {
            coyoteTime = 0;
            if (velocity.Y <= 0.01f)
            {
                canJump = true;

            }
            if (!isCrouched)
            {
                shouldDoCoyoteTime = true;

            }
        }

            // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && canJump && !isWaitingToJump)
        {
            isWaitingToJump = true;

            mesh.ScaleObjectLocal(crouchedScale);
            ((CapsuleShape3D)collisionObject.Shape).Height = 1.4f;
        }

        if (isWaitingToJump)
        {
            jumpChargeTime += (float)delta;

            if (jumpChargeTime > 0.5f)
            {
                
                PerformJump();
                velocity.Y = JumpVelocity;

            }
        }



		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (movementType == 0)
		{
			// Instant Acceleration to speed
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;

				mesh.LookAt(mesh.GlobalPosition + direction, Vector3.Up);
			}
			// Instant deceleration to zero
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
				velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
			}

		}


		if (movementType == 1)
		{
			// Smooth Acceleration
			if (direction != Vector3.Zero)
			{
				velocity.X += direction.X * Acceleration * (float)delta;
				velocity.Z += direction.Z * Acceleration * (float)delta;
            }
            // Rapid Deceleration
            else
            {
                velocity.X -= velocity.X * Deceleration * (float)delta;
                velocity.Z -= velocity.Z * Deceleration * (float)delta;
            }
            velocity.X = Math.Clamp(velocity.X, -MaxSpeed, MaxSpeed);
            velocity.Z = Math.Clamp(velocity.Z, -MaxSpeed, MaxSpeed);

        }


        if (movementType == 2)
        {
            // Ease-in ease-out acceleration
            if (direction != Vector3.Zero)
            {
                GD.Print(direction);
                inputPressedTime += (float)delta;
				velocity.X += direction.X * (Acceleration * ((float)delta * (float)delta * (3.0f - 2.0f * (float)delta))) * inputPressedTime;
                velocity.Z += direction.Z * (Acceleration * ((float)delta * (float)delta * (3.0f - 2.0f * (float)delta))) * inputPressedTime;
                // mesh.LookAt(mesh.GlobalPosition + direction, Vector3.Up);
            }
            // Instant deceleration to zero
            else
            {
                inputPressedTime = 0;

                velocity.X -= velocity.X * Deceleration * (float)delta;
                velocity.Z -= velocity.Z * Deceleration * (float)delta;
            }

            velocity.X = Math.Clamp(velocity.X, -MaxSpeed, MaxSpeed);
            velocity.Z = Math.Clamp(velocity.Z, -MaxSpeed, MaxSpeed);

        }
 
        if ((velocity.X != 0 || velocity.Z != 0) && IsOnFloor()) { mesh.LookAt(mesh.GlobalPosition + new Vector3(velocity.X,0.0f,velocity.Z), Vector3.Up); }

        Velocity = velocity;
		MoveAndSlide();
	}
}
