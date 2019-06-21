using Godot;
using System;

public class Laser: RigidBody2D {

  public override void _Ready() {
    SetContactMonitor(true);
    SetMaxContactsReported(1);

    SetGravityScale(0);
    SetCollisionMaskBit(1, false);
    SetCollisionLayerBit(1, false);

    SetCollisionLayerBit(2, true);
    SetCollisionMaskBit(3, true);
    Connect("body_entered", this, nameof(OnLaserBodyEntered));
  }

  void OnLaserBodyEntered(PhysicsBody2D body) {
    Hide();
    QueueFree();
  }
}
