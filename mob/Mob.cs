using Godot;
using System;

public class Mob : RigidBody2D {

  /* Min speed */
  [Export]
  public int MinSpeed = 150;

  /* Max speed */
  [Export]
  public int MaxSpeed = 250;

  private String[] _mobTypes = { "walk", "swim", "fly" };

  static private Random _random = new Random();

  public override void _Ready() {
    SetContactMonitor(true);
    SetMaxContactsReported(1);

    SetCollisionMaskBit(1, false);
    SetCollisionLayerBit(1, false);

    SetCollisionLayerBit(3, true);
    SetCollisionMaskBit(2, true);
    int next = _random.Next(0, _mobTypes.Length);
    GetNode<AnimatedSprite>("AnimatedSprite").Animation = _mobTypes[next];
    Connect("body_entered", this, nameof(OnMobBodyEntered));
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

  void OnMobBodyEntered(PhysicsBody2D body) {
    Hide();
    QueueFree();
  }

}
