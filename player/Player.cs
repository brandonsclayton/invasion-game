using Godot;
using System;

public class Player : Area2D {

  /* Spped of player in pixels/sec */
  [Export]
  public int Speed = 400;

  /* Player hit signal emiiter */
  [Signal]
  public delegate void Hit();

  [Export]
  public PackedScene Laser;

  /* The size of the game window */
  private Vector2 _screenSize;

  public static readonly string FIRE = "fire";

  private static readonly float _ROTATION_OFFSET = -Mathf.Deg2Rad(90);
  private static readonly string _SPRITE = "Sprite";
  private static readonly string _LASER = "Laster";
  private static readonly string _COLLISION = "Collision";

  public override void _Ready() {
    SetCollisionLayerBit(1, true);
    SetCollisionMaskBit(3, true);
    _screenSize = GetViewport().GetSize();
    Connect("body_entered", this, nameof(OnPlayerBodyEntered));
    Hide();
  }

  public override void _Process(float delta) {
    Sprite sprite = GetNode<Sprite>(_SPRITE);
    Vector2 velocity = movePlayer(sprite);

    if (velocity.Length() > 0) {
      velocity = velocity.Normalized() * Speed;
    }

    updatePosition(velocity, delta);
  }

  public override void _Input(InputEvent inputEvent) {
    if (inputEvent.IsActionPressed(FIRE)) {
      OnFire();
    }
  }

  private void OnFire() {
    Sprite player = GetNode<Sprite>(_SPRITE);
    float rotation = player.GetRotation();

    RigidBody2D laser =(RigidBody2D)Laser.Instance();
    AddChild(laser);
    laser.SetPosition(player.GetPosition());
    Vector2 playerBounds = player.GetRect().Size;
    laser.Translate(new Vector2(
        playerBounds.y * Mathf.Cos(rotation + _ROTATION_OFFSET),
        playerBounds.y * Mathf.Sin(rotation + _ROTATION_OFFSET)));
    laser.SetRotation(rotation);
    Vector2 laserVelocity = new Vector2(800f, 0).Rotated(rotation + _ROTATION_OFFSET);
    laser.SetLinearVelocity(laserVelocity);
  }

  public void Start(Vector2 pos) {
    Position = pos;
    Show();
    GetNode<CollisionPolygon2D>(_COLLISION).Disabled = false;
  }

  public void OnPlayerBodyEntered(PhysicsBody2D body) {
    System.Diagnostics.Debug.Print("On player body entered");
    Hide();
    EmitSignal("Hit");
    GetNode<CollisionPolygon2D>(_COLLISION).SetDeferred("disabled", true);
  }

  private void updatePosition(Vector2 velocity, float delta) {
    Position += velocity * delta;
    Position = new Vector2(
      x: Mathf.Clamp(Position.x, 0, _screenSize.x),
      y: Mathf.Clamp(Position.y, 0, _screenSize.y));
  }

  private Vector2 movePlayer(Sprite sprite) {
    Vector2 velocity = new Vector2();

    if (Input.IsActionPressed(Keys.RIGHT)) {
      velocity.x += 1;
    }

    if (Input.IsActionPressed(Keys.LEFT)) {
      velocity.x -= 1;
    }

    if (Input.IsActionPressed(Keys.DOWN)) {
      velocity.y += 1;
    }

    if (Input.IsActionPressed(Keys.UP)) {
      velocity.y -= 1;
    }

    RotatePlayer(sprite);

    return velocity;
  }

  void RotatePlayer(Sprite sprite) {
    Vector2 rotate = new Vector2();

    if (Input.IsActionPressed(Keys.ROTATE_RIGHT)) {
      rotate.x = 1;
    }

    if (Input.IsActionPressed(Keys.ROTATE_LEFT)) {
      rotate.x = -1;
    }

    if (Input.IsActionPressed(Keys.ROTATE_DOWN)) {
      rotate.y = 1;
    }

    if (Input.IsActionPressed(Keys.ROTATE_UP)) {
      rotate.y = -1;
    }

    if (Input.IsActionPressed(Keys.ROTATE_DOWN) ||
        Input.IsActionPressed(Keys.ROTATE_LEFT) ||
        Input.IsActionPressed(Keys.ROTATE_RIGHT) ||
        Input.IsActionPressed(Keys.ROTATE_UP)) {
      sprite.SetRotation(rotate.Angle() - _ROTATION_OFFSET);
    }


  }

  static class Keys {
    public static readonly string UP = "ui_up";
    public static readonly string DOWN = "ui_down";
    public static readonly string RIGHT = "ui_right";
    public static readonly string LEFT = "ui_left";

    public static readonly string ROTATE_UP = "rotate_up";
    public static readonly string ROTATE_DOWN = "rotate_down";
    public static readonly string ROTATE_RIGHT = "rotate_right";
    public static readonly string ROTATE_LEFT = "rotate_left";
  }

}
