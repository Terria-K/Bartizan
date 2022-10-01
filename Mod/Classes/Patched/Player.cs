#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Microsoft.Xna.Framework;
using MonoMod;
using Monocle;

namespace TowerFall
{
  public class patch_Player : Player
  {
    private string lastHatState;
    public bool spawningGhost;
    public bool diedFromPrism = false;

    ChaliceGhost summonedChaliceGhost;

    public patch_Player(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatStates hatState, bool frozen, bool flash, bool indicator)
      : base(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen, flash, indicator)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatStates hatState, bool frozen, bool flash, bool indicator);
    [MonoModConstructor]
    public void ctor(int playerIndex, Vector2 position, Allegiance allegiance, Allegiance teamColor, PlayerInventory inventory, Player.HatStates hatState, bool frozen, bool flash, bool indicator)
    {
      orig_ctor(playerIndex, position, allegiance, teamColor, inventory, hatState, frozen, flash, indicator);
      lastHatState = "UNSET";
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

    public extern void orig_Die(Arrow arrow);
    public void patch_Die(Arrow arrow)
    {
      this.diedFromPrism = arrow is PrismArrow;
      orig_Die(arrow);
    }

    public extern PlayerCorpse orig_Die(DeathCause deathCause, int killerIndex, bool brambled = false, bool laser = false);
    public PlayerCorpse patch_Die (DeathCause deathCause, int killerIndex, bool brambled = false, bool laser = false)
    {
      if (summonedChaliceGhost) {
          summonedChaliceGhost.Vanish();
          summonedChaliceGhost = null;
      }

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

    public extern void orig_Update();
    public void patch_Update()
    {
      orig_Update();
      if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).CrownSummonsChaliceGhost) {
        if (lastHatState == "UNSET") {
          lastHatState = HatState.ToString();
        } else if (lastHatState != HatState.ToString()) {
          if (lastHatState != "Crown" && HatState.ToString() == "Crown") {
            ChalicePad chalicePad = new ChalicePad(ActualPosition, 40);
            Chalice chalice = new Chalice(chalicePad);
            summonedChaliceGhost = new patch_ChaliceGhost(
              PlayerIndex,
              chalice,
              ((patch_MatchVariants)Level.Session.MatchSettings.Variants).ChaliceGhostsHuntGhosts
            );
            Level.Layers[summonedChaliceGhost.LayerIndex].Add(summonedChaliceGhost, false);
          } else if (summonedChaliceGhost && lastHatState == "Crown" && HatState.ToString() != "Crown") {
            // Ghost vanishes when player loses the crown
            summonedChaliceGhost.Vanish();
            summonedChaliceGhost = null;
          }
          lastHatState = HatState.ToString();
        }
      }
    }
  }
}
