using Godot;
using System;

public class Main : Node {

  [Export]
  public PackedScene Mob;

  private int _score;

  private Random _random = new Random();

  private float _waitTime = 0.5f;

  private float _difficultyModified = 1.25f;

  public override void _Ready() {
    GetNode<Player>("Player").Connect("Hit", this, nameof(GameOver));
    GetNode<Timer>("StartTimer").Connect("timeout", this, nameof(OnStartTimerTimeout));
    GetNode<Timer>("MobTimer").Connect("timeout", this, nameof(OnMobTimerTimeout));
    GetNode<Timer>("ScoreTimer").Connect("timeout", this, nameof(OnScoreTimerTimeout));
    GetNode<Timer>("DifficultyTimer").Connect("timeout", this, nameof(OnDifficultyTimerTimeout));
    GetNode<HUD>("HUD").Connect("StartGame", this, nameof(NewGame));
  }

  public void NewGame() {
    GetNode<Timer>("MobTimer").SetWaitTime(_waitTime);
    GetNode<AudioStreamPlayer>("Music").Play();
    _score = 0;

    Player player = GetNode<Player>("Player");
    Position2D startposition = GetNode<Position2D>("StartPosition");
    player.Start(startposition.Position);

    GetNode<Timer>("StartTimer").Start();

    HUD hud = GetNode<HUD>("HUD");
    hud.UpdateScore(_score);
    hud.ShowMessage("Get Ready!");
  }

  public void GameOver() {
    GetNode<AudioStreamPlayer>("Music").Stop();
    GetNode<Timer>("MobTimer").Stop();
    GetNode<Timer>("ScoreTimer").Stop();
    GetNode<HUD>("HUD").ShowGameOver();
    GetNode<AudioStreamPlayer>("DeathSound").Play();
  }

  public void OnStartTimerTimeout() {
    GetNode<Timer>("MobTimer").Start();
    GetNode<Timer>("ScoreTimer").Start();
    GetNode<Timer>("DifficultyTimer").Start();
  }

  public void OnScoreTimerTimeout() {
    _score++;
    GetNode<HUD>("HUD").UpdateScore(_score);
  }

  public void OnMobTimerTimeout() {
    /* Choose a random location on Path2D */
    PathFollow2D mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");
    float a = _random.Next();
    mobSpawnLocation.SetOffset(a);

    /* Create a Mob instance and add it to the scene */
    RigidBody2D mob = (RigidBody2D)Mob.Instance();
    AddChild(mob);

    /* Set mob's direction orthogonal to the path direction */
    float direction = mobSpawnLocation.Rotation + Mathf.Pi / 2;

    /* Set the mob's position to a random location */
    mob.Position = mobSpawnLocation.Position;

    /* Add some randomness to the direction */
    direction += RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
    mob.Rotation = direction;

    /* Choose velocity */
    mob.SetLinearVelocity(new Vector2(RandRange(150f, 250f), 0).Rotated(direction));

    GetNode<HUD>("HUD").Connect("StartGame", mob, "OnStartGame");
  }

  public void OnDifficultyTimerTimeout() {
    Timer mobTimer = GetNode<Timer>("MobTimer");
    float timeOut = mobTimer.WaitTime;
    mobTimer.SetWaitTime(timeOut / _difficultyModified);
  }

  private float RandRange(float min, float max) {
    return (float)_random.NextDouble() * (max - min) + min;
  }

}
