using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Mod;

namespace TowerFall
{
  public class patch_HeadhuntersRoundLogic : HeadhuntersRoundLogic
  {
    public patch_HeadhuntersRoundLogic(Session session) : base(session)
    {
      // no-op. MonoMod ignores this
    }

    public bool patch_OtherPlayerCouldWin (int playerIndex)
    {
      if (base.Session.Scores [playerIndex] < base.Session.MatchSettings.GoalScore || base.Session.GetScoreLead (playerIndex) <= 0) {
        return true;
      }
      int num = base.Session.GetHighestScore () - (base.Session.CurrentLevel.LivingPlayers - 1);
      for (int i = 0; i < MyGlobals.MAX_PLAYERS; i++) {
        if (TFGame.Players [i] && i != playerIndex) {
          Player player = base.Session.CurrentLevel.GetPlayer (i);
          if (((patch_MatchVariants)base.Session.MatchSettings.Variants).GottaBustGhosts)
          {
            if (player == null)
            {
              List<Entity> corpses = base.Session.CurrentLevel[GameTags.Corpse];
              for (int j = 0; j < corpses.Count; j++)
              {
                patch_PlayerCorpse corpse = (patch_PlayerCorpse)corpses[j];
                if (corpse.PlayerIndex == i && (corpse.hasGhost || corpse.spawningGhost) && base.Session.Scores[i] >= num)
                {
                  return true;
                }
              }
            }
            if (player != null && (!player.Dead || ((patch_Player)player).spawningGhost) && base.Session.Scores[i] >= num)
            {
              return true;
            }
          }
          else
          {
            if (player != null && !player.Dead && base.Session.Scores[i] >= num)
            {
              return true;
            }
          }
        }
      }
      return false;
    }

    public void OnPlayerGhostDeath(PlayerGhost ghost, PlayerCorpse corpse)
    {
      ((patch_RoundLogic)base.Session.RoundLogic).OnPlayerGhostDeath(ghost, corpse);
      int winner = base.Session.GetWinner ();
      if (((patch_RoundLogic)base.Session.RoundLogic).FFACheckForAllButOneDead ()) {
        base.Session.CurrentLevel.Ending = true;
        if (winner != -1 && !this.wasFinalKill) {
          this.wasFinalKill = true;
          base.FinalKill (corpse, winner);
        }
      } else if (!this.wasFinalKill && winner != -1 && !this.OtherPlayerCouldWin (winner)) {
        base.Session.CurrentLevel.Ending = true;
        this.wasFinalKill = true;
        base.FinalKill (corpse, winner);
      }
    }
  }
}
