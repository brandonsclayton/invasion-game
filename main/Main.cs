using Godot;
using System;
using System.Collections.Generic;

public class Main : Node {

  [Export]
  public PackedScene Enemy;

  [Export]
  public PackedScene Player;

  private int _score;
  private int _level;
  private Random _random = new Random();
  private float _waitTime = 0.5f;
  private float _difficultyModified = 1.25f;

  /* Nodes */
  private GUI _gui;
  private HUD _hud;
  private List<Player> _players = new List<Player>();
  private Timer _startTimer;
  private Timer _enemyTimer;
  private Timer _scoreTimer;
  private Timer _difficultyTimer;
  private Position2D _player1StartPosition;
  private Position2D _player2StartPosition;
  private Position2D _singlePlayerStartPosition;
  private AudioStreamPlayer _gamePlayAudio;
  private AudioStreamPlayer _gameOverAudio;

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
    _difficultyTimer = GetNode<Timer>("DifficultyTimer");
    _singlePlayerStartPosition = GetNode<Position2D>("StartPositions/SinglePlayerStart");
    _player1StartPosition = GetNode<Position2D>("StartPositions/Player1Start");
    _player2StartPosition = GetNode<Position2D>("StartPositions/Player2Start");
    _gamePlayAudio = GetNode<AudioStreamPlayer>("Music");
    _gameOverAudio = GetNode<AudioStreamPlayer>("DeathSound");

    _startTimer.Connect("timeout", this, nameof(OnStartTimerTimeout));
    _enemyTimer.Connect("timeout", this, nameof(OnEnemyTimerTimeout));
    _scoreTimer.Connect("timeout", this, nameof(OnScoreTimerTimeout));
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

    _enemyTimer.SetWaitTime(_waitTime);
    _gamePlayAudio.Play();
    _score = 0;
    _level = 1;

    _hud.UpdateScore(_score);
    _hud.UpdateLevel(_level);
    _startTimer.Start();
    _gui.ShowMessage("Get Ready!");
  }

  void GameOver() {
    foreach (Player player in _players) {
      player.Hide();
    }

    _gamePlayAudio.Stop();
    _enemyTimer.Stop();
    _scoreTimer.Stop();
    _gui.ShowGameOver();
    _gameOverAudio.Play();
  }

  void OnStartTimerTimeout() {
    _enemyTimer.Start();
    _scoreTimer.Start();
    _difficultyTimer.Start();
  }

  void OnScoreTimerTimeout() {
    _hud.UpdateScore(++_score);
  }

  void OnEnemyTimerTimeout() {
    /* Choose a random location on Path2D */
    PathFollow2D enemySpawnLocation = GetNode<PathFollow2D>("EnemyPath/EnemySpawnLocation");
    enemySpawnLocation.SetOffset(_random.Next());

    /* Create a Enemy instance and add it to the scene */
    Enemy enemy = (Enemy)Enemy.Instance();
    enemy.Connect("EnemyDestroyed", this, nameof(EnemyDestroyed));
    AddChild(enemy);

    /* Set enemy's direction orthogonal to the path direction */
    float direction = enemySpawnLocation.Rotation + Mathf.Pi / 2;

    /* Set the enemy's position to a random location */
    enemy.Position = enemySpawnLocation.Position;

    /* Add some randomness to the direction */
    direction += RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
    enemy.Rotation = direction;

    Vector2 enemyVelocity = new Vector2(
        RandRange(enemy.MinSpeed, enemy.MaxSpeed), 0).Rotated(direction);

    enemy.SetLinearVelocity(enemyVelocity);

    _gui.Connect("StartGame", enemy, "OnStartGame");
  }

  void OnDifficultyTimerTimeout() {
    AddLevel(_LEVEL_UP);
    _hud.LifeBarChange(_LEVEL_UP_HEALTH);
    float timeOut = _enemyTimer.WaitTime;
    _enemyTimer.SetWaitTime(timeOut / _difficultyModified);
  }

  void EnemyDestroyed() {
    AddScore(_DESTROY_ENEMY_SCORE);
  }

  void OnPlayerHit(Player player) {
    _hud.LifeBarChange(_DAMAGE);

    if (_hud.GetLifeBarValue() == _hud.MIN_LIFE_VALUE) {
      player.Destroy();
      GameOver();
    }
  }

  private float RandRange(float min, float max) {
    return (float)_random.NextDouble() * (max - min) + min;
  }

}
