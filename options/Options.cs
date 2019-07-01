using System;
using System.Collections.Generic;
using static PlayerOptions.PlayerShip;
using static PlayerOptions.LaserSFX;

public sealed class Options {

  public int numberOfPlayers;
  public List<PlayerOptions> playerOptions;

  Options(Builder builder) {
    numberOfPlayers = builder._numberOfPlayers;
    playerOptions = builder._playerOptions;
  }

  public static Builder GetBuilder() {
    return new Builder();
  }

  public class Builder {
    public int _numberOfPlayers = 1;
    public List<PlayerOptions> _playerOptions;

    public Builder() {
      _playerOptions = PlayerOptions.GetBuilder()
        .Add(PLAYER_SHIP_GREEN, LASER_SFX1)
        .Add(PLAYER_SHIP_BLUE, LASER_SFX1)
        .Build();
    }

    public Builder SetNumberOfPlayers(int numberOfPlayers) {
      _numberOfPlayers = numberOfPlayers;
      return this;
    }

    public Builder SetPlayerOptions(List<PlayerOptions> playerOptions) {
      _playerOptions = playerOptions;
      return this;
    }

    public Options Build() {
      return new Options(this);
    }
  }
}
