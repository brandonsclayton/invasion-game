using Godot;
using System;

public class Enemy : RigidBody2D {

  /* Min speed */
  [Export]
  public int MinSpeed = 150;

  /* Max speed */
  [Export]
  public int MaxSpeed = 250;

  [Signal]
  public delegate void EnemyDestroyed();

  private String[] _enemyTypes = { "walk", "swim", "fly" };

  static private Random _random = new Random();

  public override void _Ready() {
    SetContactMonitor(true);
    SetMaxContactsReported(1);

    SetCollisionMaskBit(1, false);
    SetCollisionLayerBit(1, false);

    SetCollisionLayerBit(3, true);
    SetCollisionMaskBit(2, true);
    int next = _random.Next(0, _enemyTypes.Length);
    GetNode<AnimatedSprite>("AnimatedSprite").Animation = _enemyTypes[next];
    Connect("body_entered", this, nameof(OnEnemyBodyEntered));
    GetNode<VisibilityNotifier2D>("Visibility").Connect(
        "screen_exited",
        this,
        nameof(OnVisibilityScreenExited));
  }

  public void OnVisibilityScreenExited() {
    QueueFree();
  }

  public void OnStartGame() {
    QueueFree();
  }

  public void Destroy() {
    Hide();
    GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
    QueueFree();
  }

  void OnEnemyBodyEntered(PhysicsBody2D body) {
    Destroy();

    if (body is Laser) {
      EmitSignal(nameof(EnemyDestroyed));
    }
  }

}
