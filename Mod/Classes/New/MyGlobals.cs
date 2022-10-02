#if (STAT_TRACKING)
  using Newtonsoft.Json.Linq;
#endif

namespace Mod
{
  public class MyGlobals
  {
    public static PlayerNames playerNames;

    #if (STAT_TRACKING)
      public static JArray roster;
    #endif

    #if (EIGHT_PLAYER)
      public const int MAX_PLAYERS = 8;
    #else
      public const int MAX_PLAYERS = 4;
    #endif
  }
}
