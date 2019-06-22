using Godot;
using System;

public class GUI: MarginContainer {

  [Export]
  public float MIN_LIFE_VALUE;

  [Export]
  public float MAX_LIFE_VALUE;

  private TextureProgress _lifeBarGauge;
  private Label _lifeBarValue;
  private Label _score;
  private Label _level;

  public override void _Ready() {
    Hide();
    _lifeBarGauge = GetNode<TextureProgress>("LifeBar/Gauge");
    _lifeBarValue = GetNode<Label>("LifeBar/Count/Background/Number");
    _score = GetNode<Label>("Counters/Score/Background/Number");
    _level = GetNode<Label>("Counters/Level/Background/Number");

    MIN_LIFE_VALUE = _lifeBarGauge.MinValue;
    MAX_LIFE_VALUE = _lifeBarGauge.MaxValue;
    _lifeBarGauge.Connect("value_changed", this, nameof(OnLifeBarValueChanged));
    _lifeBarGauge.SetValue(MAX_LIFE_VALUE);
  }

  public void NewGame() {
    _lifeBarGauge.SetValue(MAX_LIFE_VALUE);
    Show();
  }

  public void UpdateScore(int score) {
    _score.SetText(score.ToString());
  }

  public void UpdateLevel(int level) {
    _level.SetText(level.ToString());
  }

  public void LifeBarChange(float value) {
    _lifeBarGauge.SetValue(GetLifeBarValue() + value);
  }

  public float GetLifeBarValue() {
    return _lifeBarGauge.GetValue();
  }

  void OnLifeBarValueChanged(float value) {
    _lifeBarValue.SetText(value.ToString());
  }

}
