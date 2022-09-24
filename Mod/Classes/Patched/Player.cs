#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Microsoft.Xna.Framework;
using Patcher;
using Monocle;

namespace TowerFall
{
  public class patch_Player : Player
  {
    private string lastHatState = "UNSET";
    public bool spawningGhost;
    public bool diedFromPrism = false;

    // MyChaliceGhost summonedChaliceGhost; // uncomment later

    public patch_Player(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatStates hatState, bool frozen, bool flash, bool indicator)
      : base(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen, flash, indicator)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_Added();
    public void patch_Added()
    {
      orig_Added();
      this.spawningGhost = false;
      this.diedFromPrism = false;
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).VarietyPack[this.PlayerIndex]) {
        this.Arrows.Clear();
        this.Arrows.SetMaxArrows(10);
        var arrows = new ArrowTypes[] {
          ArrowTypes.Bomb,
          ArrowTypes.SuperBomb,
          ArrowTypes.Laser,
          ArrowTypes.Bramble,
          ArrowTypes.Drill,
          ArrowTypes.Bolt,
          ArrowTypes.Toy,
          ArrowTypes.Feather,
          ArrowTypes.Trigger,
          ArrowTypes.Prism
        };

        // Randomize. Couldn't get static method to work.
        Random rand = new Random();
        // For each spot in the array, pick
        // a random item to swap into that spot.
        for (int i = 0; i < arrows.Length - 1; i++)
        {
          int j = rand.Next(i, arrows.Length);
          ArrowTypes temp = arrows[i];
          arrows[i] = arrows[j];
          arrows[j] = temp;
        }
        this.Arrows.AddArrows(arrows);
      }
    }

    public extern bool orig_CanGrabLedge(int a, int b);
    public bool patch_CanGrabLedge(int a, int b)
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).NoLedgeGrab[this.PlayerIndex]) {
        return false;
      }
      return orig_CanGrabLedge(a, b);
    }

    public extern int orig_GetDodgeExitState();
    public int patch_GetDodgeExitState()
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).NoDodgeCooldowns[this.PlayerIndex]) {
        this.DodgeCooldown();
      }
      return orig_GetDodgeExitState();
    }

    public extern void orig_ShootArrow();
    public void patch_ShootArrow()
    {
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).InfiniteArrows[this.PlayerIndex]) {
        var arrow = this.Arrows.Arrows[0];
        orig_ShootArrow();
        this.Arrows.AddArrows(arrow);
      } else {
        orig_ShootArrow();
      }
    }

    public extern void orig_HurtBouncedOn(int bouncerIndex);
    public void patch_HurtBouncedOn(int bouncerIndex)
    {
      if (!((patch_MatchVariants)Level.Session.MatchSettings.Variants).NoHeadBounce[this.PlayerIndex]) {
        orig_HurtBouncedOn(bouncerIndex);
      }
    }

    // Identical to parent
    // public void Die (Arrow arrow)
    // {
    //     Vector2 value = Calc.SafeNormalize (arrow.Speed);
    //     int ledge = (int)((this.state.PreviousState == 1 && Vector2.Dot (Vector2.UnitX * (float)this.Facing, value) > 0.8f) ? this.Facing : ((Facing)0));
    //     int playerIndex = arrow.PlayerIndex;
    //     if (playerIndex == this.PlayerIndex && arrow is LaserArrow) {
    //         base.Level.Session.MatchStats [this.PlayerIndex].SelfLaserKills += 1u;
    //     }
    //     if (arrow.State == Arrow.ArrowStates.Falling && arrow.PlayerIndex != -1 && arrow.PlayerIndex != this.PlayerIndex) {
    //         base.Level.Session.MatchStats [arrow.PlayerIndex].DroppedArrowKills += 1u;
    //     }
    //     if (arrow.FromHyper) {
    //         if (playerIndex == this.PlayerIndex) {
    //             base.Level.Session.MatchStats [this.PlayerIndex].HyperSelfKills += 1u;
    //         } else {
    //             base.Level.Session.MatchStats [arrow.PlayerIndex].HyperArrowKills += 1u;
    //         }
    //     }
    //     this.diedFromPrism = arrow is PrismArrow;

    //     this.Die (DeathCause.Arrow, playerIndex, arrow is BrambleArrow, arrow is LaserArrow).DieByArrow (arrow, ledge);
    // }

    public extern PlayerCorpse orig_Die(DeathCause deathCause, int killerIndex, bool brambled = false, bool laser = false);
    public PlayerCorpse patch_Die (DeathCause deathCause, int killerIndex, bool brambled = false, bool laser = false)
    {
        // Will figure out chalice ghost stuff later
        // if (summonedChaliceGhost) {
        //     summonedChaliceGhost.Vanish();
        //     summonedChaliceGhost = null;
        // }

        if (Level.Session.MatchSettings.Variants.ReturnAsGhosts[this.PlayerIndex] && !this.diedFromPrism)
        {
          this.spawningGhost = true;
        }

        return orig_Die(deathCause, killerIndex, brambled, laser);
    }

    public extern int orig_DodgingUpdate();
    public int patch_DodgingUpdate()
    {
      if (this.input.DodgePressed) {
        this.canHyper = true;
        if ((bool)this.dodgeCatchCounter && this.Speed.LengthSquared () >= 3.6f) {
          ((patch_Session)(Level.Session)).MyMatchStats[this.PlayerIndex].MiracleCatches += 1u;
        }
      }
      return orig_DodgingUpdate();
    }

    // Commented out for now. I'll figure out chalice ghost stuff later
    // public void Update()
    // {
    //   base.Update();
    //   if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).CrownSummonsChaliceGhost) {
    //     if (lastHatState == "UNSET") {
    //       lastHatState = HatState.ToString();
    //     } else if (lastHatState != HatState.ToString()) {
    //       if (lastHatState != "Crown" && HatState.ToString() == "Crown") {
    //         MyChalicePad chalicePad = new MyChalicePad(ActualPosition, 4);
    //         MyChalice chalice = new MyChalice(chalicePad);
    //         summonedChaliceGhost = new MyChaliceGhost(
    //           PlayerIndex,
    //           chalice,
    //           ((patch_MatchVariants)Level.Session.MatchSettings.Variants).ChaliceGhostsHuntGhosts
    //         );
    //         Level.Layers[summonedChaliceGhost.LayerIndex].Add(summonedChaliceGhost, false);
    //       } else if (summonedChaliceGhost && lastHatState == "Crown" && HatState.ToString() != "Crown") {
    //         // Ghost vanishes when player loses the crown
    //         summonedChaliceGhost.Vanish();
    //         summonedChaliceGhost = null;
    //       }
    //       lastHatState = HatState.ToString();
    //     }
    //   }
    // }
  }
}
