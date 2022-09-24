using System;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace TowerFall
{
  public class patch_TeamDeathmatchRoundLogic : TeamDeathmatchRoundLogic
  {
    public patch_TeamDeathmatchRoundLogic(Session session)
      : base(session)
    {
      // no-op. MonoMod ignores this
    }

    public RoundEndCounter GetRoundEndCounter()
    {
      return this.roundEndCounter;
    }

    public void OnPlayerGhostDeath(PlayerGhost ghost, PlayerCorpse corpse)
    {
      ((patch_RoundLogic)base.Session.RoundLogic).OnPlayerGhostDeath(ghost, corpse);
      Allegiance allegiance = default(Allegiance);
      if (this.wasFinalKill && base.Session.CurrentLevel.LivingPlayers == 0)
      {
        this.wasFinalKill = false;
        base.CancelFinalKill();
      }
      else if (((patch_RoundLogic)base.Session.RoundLogic).TeamCheckForRoundOver(out allegiance))
      {
        base.Session.CurrentLevel.Ending = true;
        if (allegiance != Allegiance.Neutral && base.Session.Scores[(int)allegiance] >= base.Session.MatchSettings.GoalScore - 1)
        {
          this.wasFinalKill = true;
          base.FinalKillTeams(corpse, allegiance);
        }
      }
    }

    public void OnTeamRevive(Player player)
    {
      Allegiance allegiance = default(Allegiance);
      if (!((patch_RoundLogic)base.Session.RoundLogic).TeamCheckForRoundOver(out allegiance))
      {
        base.Session.CurrentLevel.Ending = false;
      }
    }
  }
}
