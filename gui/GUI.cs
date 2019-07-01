using Godot;
using System;

public class GUI: MarginContainer {

  [Signal]
  public delegate void StartGame();

  private Options.Builder _options = Options.GetBuilder();

  private HBoxContainer _startScreen;
  private Label _messageLabel;
  private Button _1PlayerButton;
  private Button _2PlayerButton;

  public override void _Ready() {
    _startScreen = GetNode<HBoxContainer>("StartScreen");
    _1PlayerButton = GetNode<Button>("StartScreen/VBoxContainer/PlayerOptions/1PlayerButton");
    _2PlayerButton = GetNode<Button>("StartScreen/VBoxContainer/PlayerOptions/2PlayerButton");

    _1PlayerButton.GrabFocus();
    _messageLabel = GetNode<Label>("MessageLabelContainer/MessageLabel");
    _messageLabel.Hide();

    _1PlayerButton.Connect(
        "pressed",
        this,
        nameof(OnStartButtonPressed),
        new Godot.Collections.Array() {1});

    _2PlayerButton.Connect(
        "pressed",
        this,
        nameof(OnStartButtonPressed),
        new Godot.Collections.Array() {2});

    GetNode<Timer>("MessageTimer").Connect("timeout", this, nameof(OnMessageTimer));
  }

  public void ShowMessage(string text) {
    _messageLabel.SetText(text);
    _messageLabel.Show();

    GetNode<Timer>("MessageTimer").Start();
  }

  async public void ShowGameOver() {
    ShowMessage("Game Over");
    Timer messageTimer = GetNode<Timer>("MessageTimer");
    await ToSignal(messageTimer, "timeout");
    _startScreen.Show();
    _1PlayerButton.GrabFocus();
  }

  public void OnStartButtonPressed(int numberOfPlayers) {
    _options.SetNumberOfPlayers(numberOfPlayers);
    _startScreen.Hide();
    EmitSignal("StartGame");
  }

  public void OnMessageTimer() {
    _messageLabel.Hide();
    _startScreen.Hide();
  }

  public Options GetOptions() {
    return _options.Build();
  }

}
