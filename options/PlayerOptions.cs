using System;
using System.Collections.Generic;

public sealed class PlayerOptions {
  public PlayerShip playerShip;
  public LaserSFX laserSFX;

  public PlayerOptions(PlayerShip playerShip, LaserSFX laserSFX) {
    this.playerShip = playerShip;
    this.laserSFX = laserSFX;
  }

  public static Builder GetBuilder() {
    return new Builder();
  }

  public class Builder {
    List<PlayerOptions> playerOptions = new List<PlayerOptions>();

    public Builder Add(PlayerShip playerShip, LaserSFX laserSFX) {
      playerOptions.Add(new PlayerOptions(playerShip, laserSFX));
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


