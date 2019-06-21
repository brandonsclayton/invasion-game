using Godot;
using System;

public class HUD: CanvasLayer {

  [Signal]
  public delegate void StartGame();

  public override void _Ready() {
    GetNode<Timer>("MessageTimer").Connect("timeout", this, nameof(OnMessageTimer));
    GetNode<Button>("StartButton").Connect("pressed", this, nameof(OnStartButtonPressed));
  }

  public void ShowMessage(string text) {
    Label messageLabel = GetNode<Label>("MessageLabel");
    messageLabel.Text = text;
    messageLabel.Show();

    GetNode<Timer>("MessageTimer").Start();
  }

  async public void ShowGameOver() {
    ShowMessage("Game Over");
    Timer messageTimer = GetNode<Timer>("MessageTimer");
    await ToSignal(messageTimer, "timeout");

    Label messageLabel = GetNode<Label>("MessageLabel");
    messageLabel.Text = "Dodge the\nCreeps!";
    messageLabel.Show();

    GetNode<Button>("StartButton").Show();
  }

  public void UpdateScore(int score) {
    GetNode<Label>("ScoreLabel").Text = score.ToString();
  }

  public void OnStartButtonPressed() {
    GetNode<Button>("StartButton").Hide();
    EmitSignal("StartGame");
  }

  public void OnMessageTimer() {
    GetNode<Label>("MessageLabel").Hide();
  }

}
