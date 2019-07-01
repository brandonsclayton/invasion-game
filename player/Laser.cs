using Godot;
using System;

public class Laser: RigidBody2D {

  private AudioStreamPlayer _sfxLaser;

  public override void _Ready() {
    SetContactMonitor(true);
    SetMaxContactsReported(1);

    SetGravityScale(0);
    SetCollisionMaskBit(1, false);
    SetCollisionLayerBit(1, false);

    SetCollisionLayerBit(2, true);
    SetCollisionMaskBit(3, true);
    Connect("body_entered", this, nameof(OnLaserBodyEntered));
    _sfxLaser = GetNode<AudioStreamPlayer>("SFXLaser1");
  }

  public void PlaySFX() {
    _sfxLaser.Play();
  }

  void OnLaserBodyEntered(PhysicsBody2D body) {
    Hide();
    QueueFree();
  }
}
