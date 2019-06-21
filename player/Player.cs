using Godot;
using System;

public class Player : Area2D {

  /* Spped of player in pixels/sec */
  [Export]
  public int Speed = 400;

  /* Player hit signal emiiter */
  [Signal]
  public delegate void Hit();

  /* The size of the game window */
  private Vector2 _screenSize;

  public override void _Ready() {
    _screenSize = GetViewport().GetSize();
    Connect("body_entered", this, nameof(OnPlayerBodyEntered));
    Hide();
  }

  public override void _Process(float delta) {
    Vector2 velocity = movePlayer();
    AnimatedSprite sprite = GetNode<AnimatedSprite>("AnimatedSprite");

    if (velocity.Length() > 0) {
      velocity = velocity.Normalized() * Speed;
      sprite.Play();
    } else {
      sprite.Stop();
    }

    updatePosition(velocity, delta);
    updateAnimation(velocity, sprite);
  }

  public void Start(Vector2 pos) {
    Position = pos;
    Show();
    GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
  }

  public void OnPlayerBodyEntered(PhysicsBody2D body) {
    Hide();
    EmitSignal("Hit");
    GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
  }

  private void updateAnimation(Vector2 velocity, AnimatedSprite sprite) {
    if (velocity.x != 0) {
      sprite.Animation = "right";
      sprite.FlipH = velocity.x < 0;
      sprite.FlipV = false;
    } else if (velocity.y != 0) {
      sprite.Animation = "up";
      sprite.FlipV = velocity.y > 0;
    }
  }

  private void updatePosition(Vector2 velocity, float delta) {
    Position += velocity * delta;
    Position = new Vector2(
      x: Mathf.Clamp(Position.x, 0, _screenSize.x),
      y: Mathf.Clamp(Position.y, 0, _screenSize.y));
  }

  private Vector2 movePlayer() {
    /* Players movement vector */
    Vector2 velocity = new Vector2();

    if (Input.IsActionPressed(Keys.Right)) {
      velocity.x += 1;
    }

    if (Input.IsActionPressed(Keys.Left)) {
      velocity.x -= 1;
    }

    if (Input.IsActionPressed(Keys.Down)) {
      velocity.y += 1;
    }

    if (Input.IsActionPressed(Keys.Up)) {
      velocity.y -= 1;
    }

    return velocity;
  }

  static class Keys {
    public static readonly string Up = "ui_up";
    public static readonly string Down = "ui_down";
    public static readonly string Right = "ui_right";
    public static readonly string Left = "ui_left";
  }

}
