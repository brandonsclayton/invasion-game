using Godot;
using System;
using System.Collections.Generic;
using static Bonus;
using static Bonus.BonusItem;

public class Main : Node {

  [Export]
  public PackedScene Enemy;

  [Export]
  public PackedScene Player;

  [Export]
  public PackedScene Asteriod;

  private int _score;
  private int _level;
  private Random _random = new Random();
  private float _enemyWaitTime = 0.5f;
  private float _difficultyModified = 1.25f;

  /* Nodes */
  private GUI _gui;
  private HUD _hud;
  private List<Player> _players = new List<Player>();
  private Timer _startTimer;
  private Timer _enemyTimer;
  private Timer _scoreTimer;
  private Timer _difficultyTimer;
  private Timer _asteriodTimer;
  private Position2D _player1StartPosition;
  private Position2D _player2StartPosition;
  private Position2D _singlePlayerStartPosition;
  private AudioStreamPlayer _gamePlayAudio;
  private AudioStreamPlayer _gameOverAudio;
  private PathFollow2D _spawnLocation;

  private static readonly float _DAMAGE = -10f;
  private static readonly int _DESTROY_ENEMY_SCORE = 5;
  private static readonly float _LEVEL_UP_HEALTH = 10f;
  private static readonly int _LEVEL_UP = 1;

  public override void _Ready() {
    _gui = GetNode<GUI>("GUI");
    _hud = GetNode<HUD>("HUD");
    _startTimer = GetNode<Timer>("StartTimer");
    _enemyTimer = GetNode<Timer>("EnemyTimer");
    _scoreTimer = GetNode<Timer>("ScoreTimer");
    _asteriodTimer = GetNode<Timer>("AsteriodTimer");
    _difficultyTimer = GetNode<Timer>("DifficultyTimer");
    _spawnLocation = GetNode<PathFollow2D>("SpawnPath/SpawnLocation");
    _singlePlayerStartPosition = GetNode<Position2D>("StartPositions/SinglePlayerStart");
    _player1StartPosition = GetNode<Position2D>("StartPositions/Player1Start");
    _player2StartPosition = GetNode<Position2D>("StartPositions/Player2Start");
    _gamePlayAudio = GetNode<AudioStreamPlayer>("Music");
    _gameOverAudio = GetNode<AudioStreamPlayer>("DeathSound");

    _startTimer.Connect("timeout", this, nameof(OnStartTimerTimeout));
    _enemyTimer.Connect("timeout", this, nameof(OnEnemyTimerTimeout));
    _scoreTimer.Connect("timeout", this, nameof(OnScoreTimerTimeout));
    _asteriodTimer.Connect("timeout", this, nameof(OnAsteriodTimerTimeout));
    _difficultyTimer.Connect("timeout", this, nameof(OnDifficultyTimerTimeout));

    _gui.Connect("StartGame", this, nameof(NewGame));
  }

  void AddScore(int score) {
    _score += score;
    _hud.UpdateScore(_score);
  }

  void AddLevel(int level) {
    _level += level;
    _hud.UpdateLevel(_level);
  }

  void NewGame() {
    _hud.NewGame();
    Options options = _gui.GetOptions();

    for (int index = 0; index < options.numberOfPlayers; index++) {
      Player player = (Player)Player.Instance();
      player.SetPlayer(options.playerOptions[index]);
      AddChild(player);
      _players.Add(player);
      player.Connect(
          "Hit",
          this,
          nameof(OnPlayerHit),
          new Godot.Collections.Array() {player});
    }

    if (options.numberOfPlayers == 1) {
      _players[0].Start(_singlePlayerStartPosition.GetPosition());
    }

    if (options.numberOfPlayers == 2) {
      _players[0].Start(_player1StartPosition.GetPosition());
      _players[1].Start(_player2StartPosition.GetPosition());
    }

    _enemyTimer.SetWaitTime(_enemyWaitTime);
    _gamePlayAudio.Play();
    _score = 0;
    _level = 1;

    _hud.UpdateScore(_score);
    _hud.UpdateLevel(_level);
    _startTimer.Start();
    _gui.ShowMessage("Get Ready!");
  }

  void GameOver() {
    _players.ForEach(player => player.Destroy());
    GetEnemies().ForEach(enemy => enemy.Destroy());
    GetAsteriods().ForEach(asteriod => asteriod.Destroy());

    _gamePlayAudio.Stop();
    _enemyTimer.Stop();
    _scoreTimer.Stop();
    _asteriodTimer.Stop();
    _startTimer.Stop();
    _difficultyTimer.Stop();
    _gui.ShowGameOver();
    _gameOverAudio.Play();
  }

  void OnStartTimerTimeout() {
    _enemyTimer.Start();
    _scoreTimer.Start();
    _difficultyTimer.Start();
    _asteriodTimer.Start();
  }

  void OnScoreTimerTimeout() {
    _hud.UpdateScore(++_score);
  }

  void OnAsteriodTimerTimeout() {
    Asteriod asteriod = (Asteriod)Asteriod.Instance();
    SpawnItem(asteriod, asteriod.MIN_SPEED, asteriod.MAX_SPEED);
    asteriod.Connect("AsteriodDestroyed", this, nameof(OnAsteriodDestroyed));
    _asteriodTimer.SetWaitTime(RandRange(
        asteriod.MIN_SPAWN_TIME / (_difficultyModified * _level),
        asteriod.MAX_SPAWN_TIME / (_difficultyModified * _level)));
  }

  void OnEnemyTimerTimeout() {
    Enemy enemy = (Enemy)Enemy.Instance();
    SpawnItem(enemy, enemy.MinSpeed, enemy.MaxSpeed);
    enemy.Connect("EnemyDestroyed", this, nameof(EnemyDestroyed));
    _gui.Connect("StartGame", enemy, "OnStartGame");
  }

  void SpawnItem(RigidBody2D body, float minSpeed, float maxSpeed) {
    _spawnLocation.SetOffset(_random.Next());
    AddChild(body);

    /* Set direction orthogonal to the path direction */
    float direction = _spawnLocation.Rotation + Mathf.Pi / 2;

    /* Set position to a random location */
    body.Position = _spawnLocation.Position;

    /* Add some randomness to the direction */
    direction += RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
    body.Rotation = direction;

    Vector2 enemyVelocity = new Vector2(
        RandRange(minSpeed, maxSpeed), 0).Rotated(direction);

    body.SetLinearVelocity(enemyVelocity);
  }

  void OnDifficultyTimerTimeout() {
    AddLevel(_LEVEL_UP);
    _hud.LifeBarChange(_LEVEL_UP_HEALTH);
    float timeOut = _enemyTimer.WaitTime;
    _enemyTimer.SetWaitTime(timeOut / _difficultyModified);
    GetEnemies().ForEach(enemy =>  enemy.Destroy());
    GetAsteriods().ForEach(asteriod => asteriod.Destroy());
    _gui.ShowMessage("Level: " + _level);
  }

  void EnemyDestroyed() {
    AddScore(_DESTROY_ENEMY_SCORE);
  }

  void OnAsteriodDestroyed(BonusItem bonusItem) {
    switch(bonusItem) {
      case DESTROY_ALL_ENEMIES:
        GetEnemies().ForEach(enemy => enemy.Destroy());
        break;
      case DESTROY_HALF_ENEMIES:
        List<Enemy> enemies = GetEnemies();
        int half = (int)Math.Ceiling(enemies.Count / 2.0);
        enemies.GetRange(0, half).ForEach(enemy => enemy.Destroy());
        break;
      case PLUS_5_HEALTH:
        _hud.LifeBarChange(5);
        break;
      case PLUS_10_HEALTH:
        _hud.LifeBarChange(10);
        break;
      case PLUS_20_HEALTH:
        _hud.LifeBarChange(20);
        break;
      case PLUS_100_SCORE:
        AddScore(100);
        break;
      case PLUS_500_SCORE:
        AddScore(500);
        break;
    }
  }

  List<Enemy> GetEnemies() {
    List<Enemy> enemies = new List<Enemy>();
    foreach (Node node in GetChildren()) {
      if (node is Enemy enemy) {
        enemies.Add(enemy);
      }
    }

    return enemies;
  }

  List<Asteriod> GetAsteriods() {
    List<Asteriod> asteriods = new List<Asteriod>();
    foreach (Node node in GetChildren()) {
      if (node is Asteriod asteriod) {
        asteriods.Add(asteriod);
      }
    }

    return asteriods;
  }

  void OnPlayerHit(Player player) {
    _hud.LifeBarChange(_DAMAGE);

    if (_hud.GetLifeBarValue() <= _hud.MIN_LIFE_VALUE) {
      player.Destroy();
      GameOver();
    }
  }

  private float RandRange(float min, float max) {
    return (float)_random.NextDouble() * (max - min) + min;
  }

}
