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
    int next = _random.Next(0, _mobTypes.Length);
    GetNode<AnimatedSprite>("AnimatedSprite").Animation = _mobTypes[next];
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

}
