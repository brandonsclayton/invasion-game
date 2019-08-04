using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public sealed class PlayerOptions {
  public readonly PlayerShip playerShip;
  public readonly LaserSFX laserSFX;
  public readonly int playerNumber;

  public PlayerOptions(PlayerShip playerShip, LaserSFX laserSFX, int playerNumber) {
    this.playerShip = playerShip;
    this.laserSFX = laserSFX;
    this.playerNumber = playerNumber;
  }

  public static Builder GetBuilder() {
    return new Builder();
  }

  public class Builder {
    List<PlayerOptions> playerOptions = new List<PlayerOptions>();
    private static int _playerNumber = 0;

    public Builder Add(PlayerShip playerShip, LaserSFX laserSFX) {
      playerOptions.Add(new PlayerOptions(playerShip, laserSFX, ++_playerNumber));
      return this;
    }

    public List<PlayerOptions> Build() {
      return playerOptions;
    }
  }

  public enum PlayerShip {
    PLAYER_SHIP_GREEN,
    PLAYER_SHIP_BLUE
  }

  public enum LaserSFX {
    LASER_SFX1
  }
}


