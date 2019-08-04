using Godot;
using System;
using System.Collections.Generic;
using static PlayerOptions.PlayerShip;
using static PlayerOptions.LaserSFX;
using static PlayerOptions;

public class Player : Node2D {

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

  private Sprite _playerSprite;
  private Area2D _playerShip;
  private Controls _controls;
  private Timer _laserTimer;
  private Dictionary<PlayerShip, Area2D> _playerShips = new Dictionary<PlayerShip, Area2D>();
  private PlayerOptions _playerOptions;

  private static readonly float _FIRE_TIMEOUT = 0.2f;
  private static readonly float _ROTATION_OFFSET = -Mathf.Deg2Rad(90);
  private static readonly string _SPRITE = "Sprite";
  private static readonly string _COLLISION = "Collision";
  private static readonly float _JOYPAD_DEADZONE = 0.35f;

  public override void _Ready() {
    _playerShip.SetCollisionLayerBit(1, true);
    _playerShip.SetCollisionMaskBit(3, true);
    _screenSize = GetViewport().GetSize();
    _laserTimer = GetNode<Timer>("LaserTimer");
    _laserTimer.SetOneShot(true);
    _laserTimer.SetWaitTime(_FIRE_TIMEOUT);
    _playerSprite = _playerShip.GetNode<Sprite>(_SPRITE);
    _playerShip.Connect("body_entered", this, nameof(OnHit));

    _laserTimer.Start();
    Hide();
  }

  public override void _Process(float delta) {
    MovePlayer(delta);
    RotatePlayer(_playerSprite);

    if (Input.IsActionPressed(_controls.FIRE) && _laserTimer.GetTimeLeft() == 0.0) {
      OnFire();
      _laserTimer.Stop();
      _laserTimer.Start();
    }
  }

  public void SetPlayer(PlayerOptions playerOptions) {
    _playerOptions = playerOptions;
    SetPlayerShips();
    _controls = new Controls(_playerOptions);
    _playerShip = _playerShips[_playerOptions.playerShip];
  }

  public void Start(Vector2 pos) {
    Position = pos;
    Show();
    _playerShip.Show();
    _playerShip.GetNode<CollisionPolygon2D>(_COLLISION).Disabled = false;
  }

  public void Destroy() {
    QueueFree();
    _playerShip.Hide();
    _playerShip.GetNode<CollisionPolygon2D>(_COLLISION).SetDeferred("disabled", true);
  }

  void OnFire() {
    float rotation = _playerSprite.GetRotation();

    Laser laser =(Laser)Laser.Instance();
    AddChild(laser);
    laser.PlaySFX();
    laser.SetPosition(_playerSprite.GetPosition());
    Vector2 playerBounds = _playerSprite.GetRect().Size;
    laser.Translate(new Vector2(
        playerBounds.y * Mathf.Cos(rotation + _ROTATION_OFFSET),
        playerBounds.y * Mathf.Sin(rotation + _ROTATION_OFFSET)));
    laser.SetRotation(rotation);
    Vector2 laserVelocity = new Vector2(800f, 0).Rotated(rotation + _ROTATION_OFFSET);
    laser.SetLinearVelocity(laserVelocity);
  }

  void OnHit(PhysicsBody2D body) {
    EmitSignal("Hit");

    if (body is Enemy enemy) {
      enemy.Destroy();
    }
  }

  void UpdatePosition(Vector2 velocity, float delta) {
    Position += velocity * delta;
    Position = new Vector2(
      x: Mathf.Clamp(Position.x, 0, _screenSize.x),
      y: Mathf.Clamp(Position.y, 0, _screenSize.y));
  }

  void MovePlayer(float delta) {
    Vector2 movement = new Vector2(
        Input.GetJoyAxis(_playerOptions.playerNumber - 1, 0),
        Input.GetJoyAxis(_playerOptions.playerNumber - 1, 1));

    if (movement.Length() < _JOYPAD_DEADZONE) {
      movement = new Vector2(0, 0);
    } else {
      movement = movement.Normalized() *
          ((movement.Length() - _JOYPAD_DEADZONE) / (1 - _JOYPAD_DEADZONE));
    }

    if (movement.Length() > 0) {
      movement = movement.Normalized() * Speed;
    }

    UpdatePosition(movement, delta);
  }

  void RotatePlayer(Sprite sprite) {
    Vector2 rotate = new Vector2(
        Input.GetJoyAxis(_playerOptions.playerNumber - 1, 2),
        Input.GetJoyAxis(_playerOptions.playerNumber - 1, 3));

    if (rotate.Length() < _JOYPAD_DEADZONE) {
      rotate = new Vector2(0, 0);
    } else {
      rotate = rotate.Normalized() *
          ((rotate.Length() - _JOYPAD_DEADZONE) / (1 - _JOYPAD_DEADZONE));
    }

    if (rotate.Length() > 0) {
      sprite.SetRotation(rotate.Angle() - _ROTATION_OFFSET);
    }
  }

  void SetPlayerShips() {
    _playerShips.Add(PLAYER_SHIP_GREEN, GetNode<Area2D>("PlayerShipGreen"));
    _playerShips.Add(PLAYER_SHIP_BLUE, GetNode<Area2D>("PlayerShipBlue"));

    foreach(Area2D ship in _playerShips.Values) {
      ship.Hide();
    }
  }

  class Controls {
    public string FIRE;

    public Controls(PlayerOptions playerOptions) {
      FIRE = $"player{playerOptions.playerNumber}_fire";
    }

  }

}
