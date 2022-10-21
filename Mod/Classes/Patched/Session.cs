#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using Mod;
using System;
using Microsoft.Xna.Framework;

namespace TowerFall
{
  public class patch_Session : Session, GhostDeathInterface
  {
    public int RoundsPlayedThisMatch;
    public MyMatchStats[] MyMatchStats;

    public patch_Session(MatchSettings settings) : base(settings)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(MatchSettings settings);
    [MonoModConstructor]
    public void ctor(MatchSettings settings)
    {
      orig_ctor(settings);

      RoundsPlayedThisMatch = 0;
      this.MyMatchStats = new MyMatchStats[MyGlobals.MAX_PLAYERS];
    }

    public void patch_LevelLoadStart(Level level)
    {
      this.CurrentLevel = level;
      this.RoundLogic = patch_RoundLogic.GetMyRoundLogic(this);
      if (this.TreasureSpawner != null) {
        this.RoundRandomArrowType = this.TreasureSpawner.GetRandomArrowType (true);
      }
    }

    public void OnPlayerGhostDeath(PlayerGhost ghost, PlayerCorpse corpse)
    {
      String logicName = this.RoundLogic.GetType().Name;
      switch (logicName)
      {
        case "TeamDeathmatchRoundLogic":
          ((patch_TeamDeathmatchRoundLogic)this.RoundLogic).OnPlayerGhostDeath(ghost, corpse);
          break;
        case "HeadhuntersRoundLogic":
          ((patch_HeadhuntersRoundLogic)this.RoundLogic).OnPlayerGhostDeath(ghost, corpse);
          break;
        case "LastManStandingRoundLogic":
          ((patch_LastManStandingRoundLogic)this.RoundLogic).OnPlayerGhostDeath(ghost, corpse);
          break;
      }
    }

    public void OnTeamRevive(Player player)
    {
      String logicName = this.RoundLogic.GetType().Name;
      switch (logicName)
      {
        case "TeamDeathmatchRoundLogic":
          ((patch_TeamDeathmatchRoundLogic)this.RoundLogic).OnTeamRevive(player);
          break;
      }
    }
  }
}
