#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System;
#if (!EIGHT_PLAYER)
  using TowerFall.Editor;
#endif

namespace TowerFall
{
  public class patch_RoundLogic : RoundLogic, GhostDeathInterface
  {
    public patch_RoundLogic(Session session, bool canHaveMiasma) : base(session, canHaveMiasma)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(Session session, bool canHaveMiasma);
    [MonoModConstructor]
    public void ctor(Session session, bool canHaveMiasma)
    {
      orig_ctor(session, canHaveMiasma);

      if (((patch_MatchVariants)(session.MatchSettings.Variants)).CalvinFall)
      {
        session.MatchSettings.Variants.Randomize();
      }
    }

    public static RoundLogic GetMyRoundLogic(Session session)
    {
      // myLogic = false; // Does this do anything?
      switch (session.MatchSettings.Mode)
      {
        case Modes.Trials:
          return new TrialsRoundLogic(session);
        case Modes.Quest:
          return new QuestRoundLogic(session);
        case Modes.DarkWorld:
          return new DarkWorldRoundLogic(session);
        case Modes.LastManStanding:
          return new LastManStandingRoundLogic(session);
        case Modes.HeadHunters:
          return new HeadhuntersRoundLogic(session);
        case Modes.TeamDeathmatch:
          return new TeamDeathmatchRoundLogic(session);
        case Modes.Warlord:
          return new WarlordRoundLogic(session);
        case Modes.LevelTest:
          return new LevelTestRoundLogic(session);
        #if (!EIGHT_PLAYER)
          case Modes.EditorTest:
            return new EditorTestRoundLogic (session);
          case Modes.EditorPreview:
            return new SubmitPreviewRoundLogic (session);
        #endif
        case RespawnRoundLogic.Mode:
          return new RespawnRoundLogic(session);
        case MobRoundLogic.Mode:
          return new MobRoundLogic(session);
        default:
          throw new Exception("No defined Round Logic for that mode!");
      }
    }

    public override void OnLevelLoadFinish()
    {
      if (!this.Session.MatchSettings.SoloMode)
      {
        SaveData.Instance.Stats.RoundsPlayed++;
        SessionStats.RoundsPlayed++;
        ((patch_Session)this.Session).RoundsPlayedThisMatch++;
      }
    }

    public extern bool orig_FFACheckForAllButOneDead();
    public bool patch_FFACheckForAllButOneDead()
    {
      if (((patch_MatchVariants)(this.Session.MatchSettings.Variants)).GottaBustGhosts)
      {
        if (this.Session.CurrentLevel.LivingPlayers == 0)
        {
          return true;
        }

        // Round not over if ghost spawning
        List<Entity> players = this.Session.CurrentLevel[GameTags.Player];
        for (int i = 0; i < players.Count; i++)
        {
          patch_Player player = (patch_Player)players[i];
          if (player.spawningGhost)
          {
            return false;
          }
        }

        List<Entity> playerCorpses = this.Session.CurrentLevel[GameTags.Corpse];
        for (int i = 0; i < playerCorpses.Count; i++)
        {
          patch_PlayerCorpse playerCorpse = (patch_PlayerCorpse)playerCorpses[i];
          if (playerCorpse.spawningGhost)
          {
            return false;
          }
        }

        // Round not over if ghosts alive
        List<Entity> playerGhosts = this.Session.CurrentLevel[GameTags.PlayerGhost];
        int livingGhostCount = 0;
        for (int i = 0; i < playerGhosts.Count; i++)
        {
          patch_PlayerGhost ghost = (patch_PlayerGhost)playerGhosts[i];
          if (ghost.State != 3)
          {
            livingGhostCount += 1;
          }
        }
        return livingGhostCount == 0 && this.Session.CurrentLevel.LivingPlayers <= 1;
      }
      else
      {
        return orig_FFACheckForAllButOneDead();
      }
    }

    public bool patch_TeamCheckForRoundOver(out Allegiance surviving)
    {
      if (this.Session.CurrentLevel.LivingPlayers == 0)
      {
        surviving = Allegiance.Neutral;
        return true;
      }
      bool[] array = new bool[2];
      bool gottaBustGhosts = ((patch_MatchVariants)(this.Session.MatchSettings.Variants)).GottaBustGhosts;
      List<Entity> players = this.Session.CurrentLevel[GameTags.Player];
      for (int i = 0; i < players.Count; i++)
      {
        patch_Player player = (patch_Player)players[i];
        if (!player.Dead || (gottaBustGhosts && player.spawningGhost))
        {
          array[(int)player.Allegiance] = true;
        }
      }

      List<Entity> playerCorpses = this.Session.CurrentLevel[GameTags.Corpse];
      for (int i = 0; i < playerCorpses.Count; i++)
      {
        patch_PlayerCorpse playerCorpse = (patch_PlayerCorpse)playerCorpses[i];
        if (playerCorpse.Revived || (gottaBustGhosts && playerCorpse.spawningGhost))
        {
          array[(int)playerCorpse.Allegiance] = true;
        }
      }

      if (gottaBustGhosts && players.Count >= 1)
      {
        List<Entity> playerGhosts = this.Session.CurrentLevel[GameTags.PlayerGhost];
        for (int i = 0; i < playerGhosts.Count; i++)
        {
          PlayerGhost playerGhost = (PlayerGhost)playerGhosts[i];
          if (playerGhost.State != 3)
          { // Ghost not dead
            array[(int)playerGhost.Allegiance] = true;
          }
        }
      }

      if (array[0] == array[1])
      {
        surviving = Allegiance.Neutral;
      }
      else if (array[0])
      {
        surviving = Allegiance.Blue;
      }
      else
      {
        surviving = Allegiance.Red;
      }
      return !array[0] || !array[1];
    }

    public void OnPlayerGhostDeath(PlayerGhost ghost, PlayerCorpse corpse)
    {
      if (
        this.miasma &&
        this.FFACheckForAllButOneDead()
      )
      {
        this.miasma.Dissipate();
      }
    }
  }
}
