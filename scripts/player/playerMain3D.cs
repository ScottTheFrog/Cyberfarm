using Godot;

public partial class playerMain3D : RigidBody3D
{
     float SPEED = 2f;
     Vector2 direction = Vector2.Zero;
     int horizontalFacingDirection = 1;
     int animationIndex = 0;
     bool isMoving;

     string[] animationNames = new string[]
    {"walking_back",
     "walking_back_right",
     "walking_right",
     "walking_forward_right",
     "walking_forward"};

     AnimatedSprite3D sprite;
     Camera3D camera;

    public override void _Ready()
    {
        sprite = GetNode<AnimatedSprite3D>("AnimatedSprite3D");
        camera = GetNode<Camera3D>("Camera3D");
    }

    public void Input() => direction = Godot.Input.GetVector("move_left", "move_right", "move_up", "move_down");

    public void Movement()
    {
        direction = direction.Rotated(Mathf.DegToRad(-45));
        Vector3 planeDirection = new Vector3(direction.X, 0, direction.Y);
        if (direction != Vector2.Zero)
        {
            isMoving = true;
            ApplyCentralImpulse(planeDirection * SPEED);
        }
    }

    public void SetFacingDirection()
    {
        if (direction == Vector2.Zero)
        { // if the player is not moving then:
            PhysicsDirectSpaceState3D spaceState3D = GetWorld3D().DirectSpaceState;
            Vector2 mousePos = GetViewport().GetMousePosition();
            Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
            Vector3 rayEnd = camera.ProjectRayNormal(mousePos) * 2000;
            Godot.Collections.Array<Rid> excludeArray = new Godot.Collections.Array<Rid>();
            excludeArray.Add(GetRid());
            PhysicsRayQueryParameters3D parameters3D = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
            parameters3D.Exclude = excludeArray;
            Godot.Collections.Dictionary rayArray = spaceState3D.IntersectRay(parameters3D);
            Vector3 positionLooked = Vector3.Zero;
            if (rayArray.ContainsKey("position"))
            {
                positionLooked = (Vector3)rayArray["position"];
            }
            Vector2 rotatedVector = new Vector2(GlobalPosition.X - positionLooked.X, GlobalPosition.Z- positionLooked.Z);
            rotatedVector = rotatedVector.Rotated(Mathf.DegToRad(-45));
            float angleToMouse = Mathf.RadToDeg(Vector2.Zero.AngleToPoint(rotatedVector));
            if (angleToMouse - Mathf.Abs(angleToMouse) != 0)
                sprite.FlipH = true;
            else
                sprite.FlipH = false;
            angleToMouse = Mathf.Abs(angleToMouse);
            switch (angleToMouse)
            {
                case < 22.5f:
                    setFrameStanding(0);
                    break;

                case < 67.5f:
                    setFrameStanding(1);
                    break;

                case < 112.5f:
                    setFrameStanding(2);
                    break;

                case < 157.5f:
                    setFrameStanding(3);
                    break;

                case < 180f:
                    setFrameStanding(4);
                    break;
            }
            return;
        }
        if (direction.X < 0)
            sprite.FlipH = true;
        else if (direction.X > 0)
            sprite.FlipH = false;

        sprite.Play();
        int animationMovIndex;
        if (direction == new Vector2(0, 1))
        {
            animationMovIndex = 4;
        }
        else
        {
            animationMovIndex = Mathf.RoundToInt(Mathf.Abs(direction.X) + 1) + Mathf.RoundToInt(direction.Y);
        }
        sprite.Animation = animationNames[animationMovIndex];
        return;
    }

     void setFrameStanding(int frame)
    {
        sprite.Stop();
        sprite.Animation = "standing";
        sprite.Frame = frame;
    }

    public override void _Process(double delta)
    {
        Input();
        SetFacingDirection();
        Movement();
    }
}