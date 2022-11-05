using TowerFall;
#if (STAT_TRACKING)
  using Newtonsoft.Json.Linq;
#endif
using System;

namespace Mod
{
  public class PlayerNames
  {
    public const string UNASSIGNED_NAME = "?";

    private string[] playerNames = {
      UNASSIGNED_NAME,
      UNASSIGNED_NAME,
      UNASSIGNED_NAME,
      UNASSIGNED_NAME,
      UNASSIGNED_NAME,
      UNASSIGNED_NAME,
      UNASSIGNED_NAME,
      UNASSIGNED_NAME,
    };

    #if (STAT_TRACKING)
      public PlayerNames(JObject playerNamesJObject) {
        for (int i = 0; i < MyGlobals.MAX_PLAYERS; i++) {
          if (TFGame.Players[i]) {
            string playerColor = ((ArcherColor)TFGame.Characters[i]).ToString();
            if (playerNamesJObject.ContainsKey(playerColor)) {
              playerNames[i] = playerNamesJObject.Value<string>(playerColor);
            }
          }
        }
      }
    #endif

    public string GetName(int playerIndex)
    {
      return playerNames[playerIndex];
    }
  }
}