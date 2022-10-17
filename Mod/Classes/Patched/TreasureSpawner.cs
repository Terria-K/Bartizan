#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace TowerFall
{
  public class patch_TreasureSpawner : TreasureSpawner
  {
    public static readonly float[] ModDefaultTreasureChances = new float[22] {
      1f,
      1f,
      1f,
      1f,
      1f,
      1f,
      1f,
      1f,
      1f,
      1f,
      0.5f,
      0.5f,
      0.5f,
      0.25f,
      0.15f,
      0.15f,
      0.15f,
      0.15f,
      0.001f,
      0.1f,
      0f, // Gems. Normally the array cuts off before this, but we need to add to it.
      1f  // Ghost Arrows
    };


    public patch_TreasureSpawner (Session session, VersusTowerData versusTowerData)
      : base (session, versusTowerData.TreasureMask, versusTowerData.SpecialArrowRate, versusTowerData.ArrowShuffle)
    {
      // no-op
    }

    public patch_TreasureSpawner (Session session, int[] mask, float arrowChance, bool arrowShuffle)
      : base (session, mask, arrowChance, arrowShuffle)
    {
      // no-op
    }

    public extern void orig_ctor(Session session, int[] mask, float arrowChance, bool arrowShuffle);
    [MonoModConstructor]
    public void ctor (Session session, int[] mask, float arrowChance, bool arrowShuffle)
    {
      int[] onlyGhostArrowsMask  = new int[22] {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1
      };

      mask = onlyGhostArrowsMask;

      // orig_ctor(session, onlyGhostArrowsMask, arrowChance, arrowShuffle);

      // Copied with some changes
      this.Session = session;
      this.Random = new Random();
      this.Exclusions = this.Session.MatchSettings.Variants.GetItemExclusions(this.Session.MatchSettings.LevelSystem.CustomTower);
      if (!GameData.DarkWorldDLC) {
        for (int i = 0; i < TreasureSpawner.DarkWorldTreasures.Length; i++) {
          if (TreasureSpawner.DarkWorldTreasures [i]) {
            this.Exclusions.Add((Pickups)i);
          }
        }
      }
      if ((bool)this.Session.MatchSettings.Variants.IgnoreTowerItemSet) {
        this.TreasureRates = (float[])patch_TreasureSpawner.ModDefaultTreasureChances.Clone();
      } else {
        this.TreasureRates = new float[patch_TreasureSpawner.ModDefaultTreasureChances.Length];
        for (int i = 0; i < this.TreasureRates.Length; i++) {
          this.TreasureRates[i] = (float)mask[i] * patch_TreasureSpawner.ModDefaultTreasureChances[i];
        }
      }
      List<Pickups>.Enumerator enumerator;
      if (arrowShuffle || (bool)this.Session.MatchSettings.Variants.ArrowShuffle) {
        for (int i = 1; i <= 9; i++) { // @TODO include ghost arrows in shuffle if they are enabled
          this.TreasureRates[i] = 0f;
        }
        List<Pickups> arrowShufflePickups = this.GetArrowShufflePickups ();
        enumerator = arrowShufflePickups.GetEnumerator ();
        try {
          while (enumerator.MoveNext ()) {
            Pickups current = enumerator.Current;
            this.TreasureRates [(int)current] = patch_TreasureSpawner.ModDefaultTreasureChances[(int)current];
          }
        } finally {
          ((IDisposable)enumerator).Dispose ();
        }
      }
      enumerator = this.Exclusions.GetEnumerator ();
      try {
        while (enumerator.MoveNext ()) {
          Pickups current2 = enumerator.Current;
          this.TreasureRates[(int)current2] = 0f;
        }
      } finally {
        ((IDisposable)enumerator).Dispose ();
      }
      float specialArrowChance = arrowChance;
      if ((bool)this.Session.MatchSettings.Variants.IgnoreTowerItemSet) {
        specialArrowChance = 0.6f;
      }
      TreasureSpawner.AdjustTreasureRatesForSpecialArrows (this.TreasureRates, specialArrowChance);
    }

    // Extended to fix asymmetrical treasure bug
    public override List<TreasureChest> GetChestSpawnsForLevel (List<Vector2> chestPositions, List<Vector2> bigChestPositions)
    {
      List<TreasureChest> list = new List<TreasureChest> ();

      Calc.Shuffle(chestPositions);
      Calc.Shuffle(bigChestPositions);

      if ((bool)this.Session.MatchSettings.Variants.NoTreasure) {
        return list;
      }
      if ((bool)this.Session.MatchSettings.Variants.BombChests) {
        int num = 0;
        while (chestPositions.Count > 0) {
          Vector2 vector = chestPositions [0];
          chestPositions.RemoveAt (0);
          num += Calc.Range (this.Random, 30, 90);
          list.Add (new TreasureChest (vector, TreasureChest.Types.AutoOpen, TreasureChest.AppearModes.Time, Pickups.Bomb, num));
          if ((bool)this.Session.MatchSettings.Variants.SymmetricalTreasure && vector.X != 160f) {
            Vector2 vector2 = WrapMath.Opposite (vector);
            if (chestPositions.Contains (vector2)) {
              chestPositions.Remove (vector2);
              list.Add (new TreasureChest (vector2, TreasureChest.Types.AutoOpen, TreasureChest.AppearModes.Time, Pickups.Bomb, num));
            }
          }
        }
        return list;
      }
      if (!(bool)this.Session.MatchSettings.Variants.MaxTreasure) {
        List<Pickups> list2 = new List<Pickups> ();
        if (!(bool)this.Session.MatchSettings.Variants.BottomlessTreasure) {
          list2.Add (Pickups.Bomb);
        }
        bool flag = (bool)this.Session.MatchSettings.Variants.AlwaysBigTreasure || (bool)this.Session.MatchSettings.Variants.BottomlessTreasure || Calc.Chance (this.Random, 0.03f);
        if (bigChestPositions.Count > 0 && flag && this.CanProvideTreasure (list2)) {
          List<Pickups> list3 = new List<Pickups> ();
          for (int i = 0; i < 3; i++) {
            if (!this.CanProvideTreasure (list2)) {
              break;
            }
            Pickups treasureSpawn = this.GetTreasureSpawn (list2);
            if (treasureSpawn >= Pickups.Shield) {
              list2.Add (treasureSpawn);
            }
            list3.Add (treasureSpawn);
          }
          Calc.Shuffle (bigChestPositions, this.Random);
          Vector2 position = bigChestPositions [0];
          int timer = Calc.Range (this.Random, 30, TFGame.PlayerAmount * 30);
          if ((bool)this.Session.MatchSettings.Variants.BottomlessTreasure) {
            list.Add (new TreasureChest (position, TreasureChest.Types.Bottomless, TreasureChest.AppearModes.Time, new Pickups[1] {
              list3 [0]
            }, timer));
          } else {
            list.Add (new TreasureChest (position, TreasureChest.Types.Large, TreasureChest.AppearModes.Time, list3.ToArray (), timer));
          }
          return list;
        }
      }
      if (chestPositions.Count > 0 && this.CanProvideTreasure (null)) {
        int num2 = 0;
        List<Pickups> list2 = new List<Pickups> ();
        int num3 = 0;
        int num4 = 0;
        int num5 = 0;
        while (chestPositions.Count > 0 && this.CanProvideTreasure (list2) && this.CanSpawnAnotherChest (num2)) {
          Vector2 vector = chestPositions [0];
          chestPositions.RemoveAt (0);
          num2++;
          int num = Calc.Range (Calc.Random, 10, TFGame.PlayerAmount * 60);
          Pickups treasureSpawn = this.GetTreasureSpawn (list2);
          list.Add (new TreasureChest (vector, TreasureChest.Types.Normal, TreasureChest.AppearModes.Time, treasureSpawn, num));
          // Here is the fix. Need to update the center now that the game is widescreen.
          #if (EIGHT_PLAYER)
            float centerX = 210f;
          #else
            float centerX = 160f;
          #endif
          if ((bool)this.Session.MatchSettings.Variants.SymmetricalTreasure && vector.X != centerX) {
            Vector2 vector2 = WrapMath.Opposite (vector);
            if (chestPositions.Contains (vector2)) {
              chestPositions.Remove (vector2);
              num2++;
              list.Add (new TreasureChest (vector2, TreasureChest.Types.Normal, TreasureChest.AppearModes.Time, treasureSpawn, num));
            }
          }
          int num6;
          switch (treasureSpawn) {
          case Pickups.Wings:
            num3++;
            if (num3 >= TFGame.PlayerAmount) {
              list2.Add (Pickups.Wings);
            }
            break;
          case Pickups.Mirror:
            num4++;
            if (num4 >= TFGame.PlayerAmount) {
              list2.Add (Pickups.Mirror);
            }
            break;
          case Pickups.SpeedBoots:
            num5++;
            if (num5 >= TFGame.PlayerAmount) {
              list2.Add (Pickups.SpeedBoots);
            }
            break;
          default:
            num6 = ((treasureSpawn != Pickups.SpaceOrb) ? 1 : 0);
            goto IL_0493;
          case Pickups.TimeOrb:
          case Pickups.DarkOrb:
          case Pickups.LavaOrb:
            {
              num6 = 0;
              goto IL_0493;
            }
            IL_0493:
            if (num6 == 0) {
              list2.Add (treasureSpawn);
              list2.Add (Pickups.ChaosOrb);
            } else if (treasureSpawn == Pickups.ChaosOrb) {
              list2.Add (Pickups.LavaOrb);
              list2.Add (Pickups.DarkOrb);
              list2.Add (Pickups.TimeOrb);
              list2.Add (Pickups.SpaceOrb);
              list2.Add (Pickups.ChaosOrb);
            }
            break;
          }
        }
      }
      return list;
    }

  }
}
