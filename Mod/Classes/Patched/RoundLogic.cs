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
    // public static bool myLogic; // can't tell what this is meant to do. Commenting out to see if I need it
    // public static RoundLogic myRoundLogic; // commenting out to see if I need it

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
        // Uncomment later
        // case RespawnRoundLogic.Mode:
        //   return new RespawnRoundLogic(session);
        // case MobRoundLogic.Mode:
        //   return new MobRoundLogic(session);
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

    // It doesn't look like I actually changed anything here
    // public override void OnPlayerDeath(Player player, PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex)
    // {
    //   if (this.Session.CurrentLevel.KingIntro)
    //   {
    //     this.Session.CurrentLevel.KingIntro.Laugh();
    //   }
    //   if (this.Session.MatchSettings.Variants.GunnStyle)
    //   {
    //     GunnStyle gunnStyle = new GunnStyle(corpse);
    //     this.Session.CurrentLevel.Layers[gunnStyle.LayerIndex].Add(gunnStyle, false);
    //   }
    //   if (
    //     this.miasma && killerIndex != -1 &&
    //     this.FFACheckForAllButOneDead()
    //   )
    //   {
    //     this.miasma.Dissipate();
    //   }
    //   if (killerIndex != -1 && killerIndex != playerIndex)
    //   {
    //     this.Kills[killerIndex]++;
    //   }
    //   if (killerIndex != -1 && !this.Session.CurrentLevel.IsPlayerAlive(killerIndex))
    //   {
    //     MatchStats[] expr_F8_cp_0 = this.Session.MatchStats;
    //     expr_F8_cp_0[killerIndex].KillsWhileDead = expr_F8_cp_0[killerIndex].KillsWhileDead + 1u;
    //   }
    //   if (killerIndex != -1 && killerIndex != playerIndex)
    //   {
    //     this.Session.MatchStats[killerIndex].RegisterFastestKill(this.Time);
    //   }
    //   DeathType deathType = DeathType.Normal;
    //   if (killerIndex == playerIndex)
    //   {
    //     deathType = DeathType.Self;
    //   }
    //   else if (killerIndex != -1 && this.Session.MatchSettings.TeamMode && this.Session.MatchSettings.GetPlayerAllegiance(playerIndex) == this.Session.MatchSettings.GetPlayerAllegiance(killerIndex))
    //   {
    //     deathType = DeathType.Team;
    //   }
    //   if (killerIndex != -1) {
    //     if (deathType == DeathType.Normal && this.Session.WasWinningAtStartOfRound (playerIndex)) {
    //       this.Session.MatchStats [killerIndex].WinnerKills += 1u;
    //     }
    //     if (!this.Session.MatchSettings.SoloMode) {
    //       this.Session.MatchStats [killerIndex].Kills.Add (deathType, cause, TFGame.Characters [playerIndex]);
    //       SaveData.Instance.Stats.Kills.Add (deathType, cause, TFGame.Characters [killerIndex]);
    //       SaveData.Instance.Stats.TotalVersusKills++;
    //       SaveData.Instance.Stats.RegisterVersusKill (killerIndex);
    //     }
    //   }
    //   this.Session.MatchStats [playerIndex].Deaths.Add (deathType, cause, (killerIndex == -1) ? (-1) : TFGame.Characters [killerIndex]);
    //   SaveData.Instance.Stats.Deaths.Add (deathType, cause, TFGame.Characters [playerIndex]);
    //   if (!this.Session.MatchSettings.SoloMode) {
    //     SessionStats.RegisterVersusKill (killerIndex, playerIndex, deathType == DeathType.Team);
    //   }
    // }

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

    // Doesn't look like I changed anything
    // public new void FinalKillTeams (PlayerCorpse corpse, Allegiance otherSpotlightTeam)
    // {
    //   List<LevelEntity> list = new List<LevelEntity> ();
    //   for (int i = 0; i < MyGlobals.MAX_PLAYERS; i++) {
    //     if (TFGame.Players [i] && this.Session.MatchSettings.Teams [i] == otherSpotlightTeam) {
    //       this.Session.MatchStats [i].GotWin = true;
    //       LevelEntity playerOrCorpse = this.Session.CurrentLevel.GetPlayerOrCorpse (i);
    //       if (playerOrCorpse != null && playerOrCorpse != corpse) {
    //         list.Add (playerOrCorpse);
    //       }
    //     }
    //   }
    //   this.Session.CurrentLevel.LightingLayer.SetSpotlight (list.ToArray ());
    //   this.FinalKillNoSpotlight ();
    // }

    // Nothing changed
    // public override void FinalKillNoSpotlight ()
    // {
    //   this.Session.CurrentLevel.OrbLogic.DoSlowMoKill ();
    //   this.Session.MatchSettings.LevelSystem.StopVersusMusic ();
    //   Sounds.sfx_finalKill.Play (160f, 1f);
    // }
  }
}
