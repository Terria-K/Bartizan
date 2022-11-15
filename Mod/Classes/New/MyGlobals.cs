#if (STAT_TRACKING)
  using Newtonsoft.Json.Linq;
#endif

namespace Mod
{
  public class MyGlobals
  {
    public static PlayerNames playerNames;

    public enum GameTags {}

    public enum ArrowTypes {
      Ghost = 11
    }

    public enum Pickups {
      GhostArrows = 21
    }

    #if (STAT_TRACKING)
      public static JArray roster;
    #endif

    #if (EIGHT_PLAYER)
      public const int MAX_PLAYERS = 8;
    #else
      public const int MAX_PLAYERS = 4;
    #endif

    public static bool IsAntiGrav = false;
  }
}
