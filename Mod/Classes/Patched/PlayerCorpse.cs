#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System;

namespace TowerFall
{
  public class patch_PlayerCorpse : PlayerCorpse
  {
    public bool reviverAdded;
    public bool spawningGhost;
    public bool hasGhost;

    public patch_PlayerCorpse (PlayerCorpse.EnemyCorpses enemyCorpse, Vector2 position, Facing facing, int killerIndex) : base (enemyCorpse.ToString (), Allegiance.Neutral, position, facing, -1, killerIndex)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_Update();
    public void patch_Update ()
    {
      orig_Update();
      if (this.reviverAdded == true) {
        this.reviverAdded = false;
        List<Entity> teamRevivers = base.Level[GameTags.TeamReviver];
        for (int i = 0; i < teamRevivers.Count; i++) {
          TeamReviver teamReviver = (TeamReviver)(teamRevivers[i]);
          if (teamReviver.Corpse.PlayerIndex == this.PlayerIndex) {
            base.Level.Layers[teamReviver.LayerIndex].Remove(teamReviver);
            patch_TeamReviver myTeamReviver = new patch_TeamReviver(
              this,
              TeamReviver.Modes.TeamDeathmatch,
              ((patch_TeamDeathmatchRoundLogic)(base.Level.Session.RoundLogic)).GetRoundEndCounter(),
              ((patch_MatchVariants)(base.Level.Session.MatchSettings.Variants)).GhostRevives
            );
            base.Level.Layers[myTeamReviver.LayerIndex].Add(myTeamReviver, false);
          }
        }
      }
    }

    public extern void orig_Added();
    public void patch_Added ()
    {
      this.spawningGhost = true;
      this.hasGhost = false;
      orig_Added();

      if (this.PlayerIndex != -1) {
        if (base.Level.Session.MatchSettings.Mode == Modes.TeamDeathmatch && base.Level.Session.MatchSettings.Variants.TeamRevive) {
          reviverAdded = true;
        }
      }
    }

    public extern void orig_DieByArrow(Arrow arrow, int ledge);
    public void patch_DieByArrow (Arrow arrow, int ledge)
    {
      if (this.CanDoPrismHit (arrow)) {
        this.spawningGhost = false;
      }
      orig_DieByArrow(arrow, ledge);
    }
  }
}
