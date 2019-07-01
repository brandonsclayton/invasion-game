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
  private Dictionary<PlayerShip, Area2D> _playerShips = new Dictionary<PlayerShip, Area2D>();

  private static int _playerNumber = 0;
  private static readonly float _ROTATION_OFFSET = -Mathf.Deg2Rad(90);
  private static readonly string _SPRITE = "Sprite";
  private static readonly string _COLLISION = "Collision";

  public override void _Ready() {
    _playerShip.SetCollisionLayerBit(1, true);
    _playerShip.SetCollisionMaskBit(3, true);
    _screenSize = GetViewport().GetSize();
    _playerSprite = _playerShip.GetNode<Sprite>(_SPRITE);
    _playerShip.Connect("body_entered", this, nameof(OnHit));

    Hide();
  }

  public void SetPlayer(PlayerOptions playerOptions) {
    SetPlayerShips();
    _controls = new Controls(++_playerNumber);
    _playerShip = _playerShips[playerOptions.playerShip];
  }

  public Area2D GetPlayerShip() {
    return _playerShip;
  }

  public override void _Process(float delta) {
    Vector2 velocity = movePlayer(_playerSprite);

    if (velocity.Length() > 0) {
      velocity = velocity.Normalized() * Speed;
    }

    updatePosition(velocity, delta);
  }

  public override void _Input(InputEvent inputEvent) {
    if (inputEvent.IsActionPressed(_controls.FIRE)) {
      OnFire();
    }
  }

  private void OnFire() {
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

  public void Start(Vector2 pos) {
    Position = pos;
    Show();
    _playerShip.Show();
    _playerShip.GetNode<CollisionPolygon2D>(_COLLISION).Disabled = false;
  }

  public void Destroy() {
    _playerShip.Hide();
    _playerShip.GetNode<CollisionPolygon2D>(_COLLISION).SetDeferred("disabled", true);
  }

  void OnHit(PhysicsBody2D body) {
    EmitSignal("Hit");

    if (body is Enemy enemy) {
      enemy.Destroy();
    }
  }

  private void updatePosition(Vector2 velocity, float delta) {
    Position += velocity * delta;
    Position = new Vector2(
      x: Mathf.Clamp(Position.x, 0, _screenSize.x),
      y: Mathf.Clamp(Position.y, 0, _screenSize.y));
  }

  private Vector2 movePlayer(Sprite sprite) {
    Vector2 velocity = new Vector2();

    if (Input.IsActionPressed(_controls.RIGHT)) {
      velocity.x += 1;
    }

    if (Input.IsActionPressed(_controls.LEFT)) {
      velocity.x -= 1;
    }

    if (Input.IsActionPressed(_controls.DOWN)) {
      velocity.y += 1;
    }

    if (Input.IsActionPressed(_controls.UP)) {
      velocity.y -= 1;
    }

    RotatePlayer(sprite);

    return velocity;
  }

  void RotatePlayer(Sprite sprite) {
    Vector2 rotate = new Vector2();

    if (Input.IsActionPressed(_controls.ROTATE_RIGHT)) {
      rotate.x = 1;
    }

    if (Input.IsActionPressed(_controls.ROTATE_LEFT)) {
      rotate.x = -1;
    }

    if (Input.IsActionPressed(_controls.ROTATE_DOWN)) {
      rotate.y = 1;
    }

    if (Input.IsActionPressed(_controls.ROTATE_UP)) {
      rotate.y = -1;
    }

    if (Input.IsActionPressed(_controls.ROTATE_DOWN) ||
        Input.IsActionPressed(_controls.ROTATE_LEFT) ||
        Input.IsActionPressed(_controls.ROTATE_RIGHT) ||
        Input.IsActionPressed(_controls.ROTATE_UP)) {
      sprite.SetRotation(rotate.Angle() - _ROTATION_OFFSET);
    }


  }

  private void SetPlayerShips() {
    _playerShips.Add(PLAYER_SHIP_GREEN, GetNode<Area2D>("PlayerShipGreen"));
    _playerShips.Add(PLAYER_SHIP_BLUE, GetNode<Area2D>("PlayerShipBlue"));

    foreach(Area2D ship in _playerShips.Values) {
      ship.Hide();
    }
  }

  class Controls {
    public string UP;
    public string DOWN;
    public string RIGHT;
    public string LEFT;

    public string ROTATE_UP;
    public string ROTATE_DOWN;
    public string ROTATE_RIGHT;
    public string ROTATE_LEFT;

    public string FIRE;

    public Controls(int player) {
      string num = player.ToString();

      UP = $"player{num}_up";
      DOWN = $"player{num}_down";
      LEFT = $"player{num}_left";
      RIGHT = $"player{num}_right";

      ROTATE_UP = $"player{num}_rotate_up";
      ROTATE_DOWN = $"player{num}_rotate_down";
      ROTATE_LEFT = $"player{num}_rotate_left";
      ROTATE_RIGHT = $"player{num}_rotate_right";

      FIRE = $"player{num}_fire";
    }

  }

}
