using Godot;
using System;
public partial class playerMain : RigidBody2D
{
    float SPEED = 20f;
    const float STEPIMPULSE = 5f; //Impulse for each step
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
    AnimatedSprite2D sprite;
    Timer stepTimer;
    public override void _Ready()
    {
        stepTimer = GetNode<Timer>("StepTimer");
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        ChangedSpriteScale += ChangeSpriteScale;
        ChangedSpriteIndex += ChangeSpriteIndex;
    }
    public void Movement()
    {
        direction = Godot.Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (direction != Vector2.Zero && stepTimer.IsStopped() == true)
        {
            isMoving = true;
            ApplyCentralImpulse(direction * SPEED * STEPIMPULSE);
            stepTimer.Start();
        }
    }
    //Using signals to learn how to use them, not really necessary in this case
    [Signal]
    public delegate void ChangedSpriteScaleEventHandler(float num);
    private void ChangeSpriteScale(float num)
    {
        sprite.Scale = new Vector2(num, 1);
    }
    [Signal]
    public delegate void ChangedSpriteIndexEventHandler(int num);
    private void ChangeSpriteIndex(int num)
    { //function only used when the player is not doing any animation (standing)
        sprite.Stop();
        sprite.Animation = "standing";
        sprite.Frame = num;
    }
    public void SetFacingDirection()
    {
        Vector2 MousePos = GetGlobalMousePosition();
        int scalingFactor = Godot.Mathf.RoundToInt((MousePos - Position).Normalized().X);
        if (horizontalFacingDirection != scalingFactor && scalingFactor != 0 && direction == Vector2.Zero)
        {
            horizontalFacingDirection = scalingFactor;
            EmitSignal(SignalName.ChangedSpriteScale, scalingFactor);
        }
        float indexChanged = 2 + Godot.Mathf.Round(2*(MousePos - Position).Normalized().Y);
        if (direction.X != 0)
        {
            EmitSignal(SignalName.ChangedSpriteScale, Godot.Mathf.RoundToInt(direction.X));
        }
        if (direction != Vector2.Zero)
        {
            sprite.Play();
            int animationMovIndex;
            if (direction == new Vector2(0, 1))
            {
                animationMovIndex = 4;
            }
            else
            {
                animationMovIndex = Godot.Mathf.RoundToInt(Godot.Mathf.Abs(direction.X) + 1) + Godot.Mathf.RoundToInt(direction.Y);
            }
            sprite.Animation = animationNames[animationMovIndex];
            return;
        }
        //There's a necessity of rounding the X and Y values of the direction vector becau-
        //se they go to a "normalized" value, guessing it's for diagonal movement which makes sense -
        //but it makes quirkiness happen.

        //From here the function assumes that the player is not moving.
        if (isMoving)
        {
            isMoving = false;
            EmitSignal(SignalName.ChangedSpriteIndex, animationIndex);
        }
        if (animationIndex != indexChanged)
        {
            animationIndex = (int)indexChanged;
            EmitSignal(SignalName.ChangedSpriteIndex, animationIndex);
        }
    }
    public override void _Process(double delta)
    {
        Movement();
        SetFacingDirection();
    }
    public void _StepTimeout()
    {
    }

}
