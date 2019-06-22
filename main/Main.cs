using Godot;
using System;

public class Main : Node {

  [Export]
  public PackedScene Mob;

  private int _score;
  private int _level;
  private Random _random = new Random();
  private float _waitTime = 0.5f;
  private float _difficultyModified = 1.25f;

  private Player _player;
  private GUI _gui;
  private HUD _hud;

  private static readonly float _DAMAGE = -15f;
  private static readonly int _DESTROY_ENEMY_SCORE = 5;
  private static readonly float _LEVEL_UP_HEALTH = 5f;
  private static readonly int _LEVEL_UP = 1;

  public override void _Ready() {
    _player = GetNode<Player>("Player");
    _gui = GetNode<GUI>("GUI");
    _hud = GetNode<HUD>("HUD");

    _player.Connect("Hit", this, nameof(OnPlayerHit));
    GetNode<Timer>("StartTimer").Connect("timeout", this, nameof(OnStartTimerTimeout));
    GetNode<Timer>("MobTimer").Connect("timeout", this, nameof(OnMobTimerTimeout));
    GetNode<Timer>("ScoreTimer").Connect("timeout", this, nameof(OnScoreTimerTimeout));
    GetNode<Timer>("DifficultyTimer").Connect("timeout", this, nameof(OnDifficultyTimerTimeout));
    _hud.Connect("StartGame", this, nameof(NewGame));
  }

  void AddScore(int score) {
    _score += score;
    _gui.UpdateScore(_score);
  }

  void AddLevel(int level) {
    _level += level;
    _gui.UpdateLevel(_level);
  }

  public void NewGame() {
    GetNode<Timer>("MobTimer").SetWaitTime(_waitTime);
    // GetNode<AudioStreamPlayer>("Music").Play();
    _score = 0;
    _level = 1;

    _gui.UpdateScore(_score);
    _gui.UpdateLevel(_level);

    Position2D startposition = GetNode<Position2D>("StartPosition");
    _player.Start(startposition.Position);

    GetNode<Timer>("StartTimer").Start();

    _hud.ShowMessage("Get Ready!");
  }

  public void GameOver() {
    GetNode<AudioStreamPlayer>("Music").Stop();
    GetNode<Timer>("MobTimer").Stop();
    GetNode<Timer>("ScoreTimer").Stop();
    GetNode<HUD>("HUD").ShowGameOver();
    // GetNode<AudioStreamPlayer>("DeathSound").Play();
  }

  public void OnStartTimerTimeout() {
    GetNode<Timer>("MobTimer").Start();
    GetNode<Timer>("ScoreTimer").Start();
    GetNode<Timer>("DifficultyTimer").Start();
    _gui.NewGame();
  }

  public void OnScoreTimerTimeout() {
    _gui.UpdateScore(++_score);
  }

  public void OnMobTimerTimeout() {
    /* Choose a random location on Path2D */
    PathFollow2D mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");
    mobSpawnLocation.SetOffset(_random.Next());

    /* Create a Mob instance and add it to the scene */
    Mob mob = (Mob)Mob.Instance();
    mob.Connect("MobDestroyed", this, nameof(EnemyDestroyed));
    AddChild(mob);

    /* Set mob's direction orthogonal to the path direction */
    float direction = mobSpawnLocation.Rotation + Mathf.Pi / 2;

    /* Set the mob's position to a random location */
    mob.Position = mobSpawnLocation.Position;

    /* Add some randomness to the direction */
    direction += RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
    mob.Rotation = direction;

    Vector2 mobVelocity = new Vector2(
        RandRange(mob.MinSpeed, mob.MaxSpeed), 0).Rotated(direction);

    mob.SetLinearVelocity(mobVelocity);

    _hud.Connect("StartGame", mob, "OnStartGame");
  }

  public void OnDifficultyTimerTimeout() {
    AddLevel(_LEVEL_UP);
    _gui.LifeBarChange(_LEVEL_UP_HEALTH);
    Timer mobTimer = GetNode<Timer>("MobTimer");
    float timeOut = mobTimer.WaitTime;
    mobTimer.SetWaitTime(timeOut / _difficultyModified);
  }

  void EnemyDestroyed() {
    AddScore(_DESTROY_ENEMY_SCORE);
  }

  void OnPlayerHit() {
    _gui.LifeBarChange(_DAMAGE);

    if (_gui.GetLifeBarValue() == _gui.MIN_LIFE_VALUE) {
      _player.Destroy();
      GameOver();
    }
  }

  private float RandRange(float min, float max) {
    return (float)_random.NextDouble() * (max - min) + min;
  }

}
