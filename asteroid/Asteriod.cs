using Godot;
using System;

public class Asteriod : RigidBody2D {

  [Export]
  public readonly int MIN_SPEED = 150;

  [Export]
  public readonly int MAX_SPEED = 250;

  [Export]
  public readonly float MIN_SPAWN_TIME = 5.0f;

  [Export]
  public readonly float MAX_SPAWN_TIME = 10.0f;

  [Signal]
  public delegate void AsteriodDestroyed();

  private CollisionPolygon2D _collisionShape;

  public override void _Ready() {
    SetContactMonitor(true);
    SetMaxContactsReported(1);

    SetCollisionMaskBit(1, false);
    SetCollisionLayerBit(1, false);

    SetCollisionLayerBit(3, true);
    SetCollisionMaskBit(2, true);

    _collisionShape = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
    Connect("body_entered", this, nameof(OnAsteriodBodyEntered));
    GetNode<VisibilityNotifier2D>("Visibility").Connect(
        "screen_exited",
        this,
        nameof(OnVisibilityScreenExited));
  }

  void OnVisibilityScreenExited() {
    QueueFree();
  }

  public void Destroy() {
    Hide();
    _collisionShape.SetDeferred("disabled", true);
    QueueFree();
  }

  void OnAsteriodBodyEntered(PhysicsBody2D body) {
    Destroy();

    if (body is Laser) {
      EmitSignal(nameof(AsteriodDestroyed), Bonus.GetBonusItem());
    }
  }
}
