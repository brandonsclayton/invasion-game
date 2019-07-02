using System;

public class Bonus {

  private static Random _rand = new Random();
  private static readonly BonusItem[] _items = (BonusItem[])Enum.GetValues(typeof(BonusItem));

  internal static BonusItem GetBonusItem() {
    return _items[_rand.Next(0, _items.Length)];
  }

  public enum BonusItem {
    DESTROY_ALL_ENEMIES,
    DESTROY_HALF_ENEMIES,
    PLUS_5_HEALTH,
    PLUS_10_HEALTH,
    PLUS_20_HEALTH,
    PLUS_100_SCORE,
    PLUS_500_SCORE
  }
}
