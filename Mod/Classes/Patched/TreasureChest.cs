#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using MonoMod;
using Mod;
using Monocle;
using System;

namespace TowerFall
{
  public class patch_TreasureChest : TreasureChest
  {
    public patch_TreasureChest(Vector2 position, Types graphic, AppearModes mode, Pickups pickup, int timer = 0) : base(position, graphic, mode, pickup, timer)
    {
      // no-op
    }

    public patch_TreasureChest(Vector2 position, Types graphic, AppearModes mode, Pickups[] pickups, int timer = 0) : base(position, graphic, mode, pickups, timer)
    {
      // no-op
    }

    public extern void orig_Added();
    public void patch_Added()
    {
      orig_Added();
      flipSprite();
    }

    public void flipSprite()
    {
      bool isRotated = this.sprite.Rotation != 0;
      if (IsAntiGrav() && !isRotated) {
        switch (this.type) {
          case Types.Normal:
          case Types.AutoOpen:
          case Types.Special:
            base.Collider = new WrapHitbox(10f, 10f, -5f, 5f);
            break;
          case Types.Large:
          case Types.Bottomless:
            base.Collider = new WrapHitbox(20f, 14f, -10f, 5f);
            break;
        }
        while (base.CollideCheck(GameTags.Solid, base.Position + Vector2.UnitY)) {
          base.Position -= Vector2.UnitY;
        }
        this.sprite.Rotation = 3.1415926536f;
      } else if (isRotated) {
        Console.WriteLine("Rotating back to normal 3");
        switch (this.type) {
          case Types.Normal:
          case Types.AutoOpen:
          case Types.Special:
            base.Collider = new WrapHitbox (10f, 10f, -5f, -5f);
            break;
          case Types.Large:
          case Types.Bottomless:
            base.Collider = new WrapHitbox(20f, 14f, -10f, -9f);
            break;
        }
        while (base.CollideCheck(GameTags.Solid, base.Position + Vector2.UnitY)) {
          base.Position += Vector2.UnitY;
        }
        this.sprite.Rotation = 0;
      }
    }

    public void patch_OnPlayerGhostCollide(PlayerGhost ghost)
    {
      if (!base.Flashing && this.type != Types.Large && this.type != Types.Bottomless)
      {
        if (((patch_MatchVariants)Level.Session.MatchSettings.Variants).GhostItems)
        {
          patch_PlayerGhost g = (patch_PlayerGhost)ghost;
          if (this.pickups[0].ToString() == "SpeedBoots" && !g.HasSpeedBoots ||
            this.pickups[0].ToString() == "Shield" && !g.HasShield ||
            this.pickups[0].ToString() == "Mirror" && !g.Invisible ||
            this.pickups[0].ToString().Contains("Orb"))
          {
            this.OpenChest(ghost.PlayerIndex);
          } else
          {
            this.OpenChestForceBomb(ghost.PlayerIndex);
          }
        }
        else
        {
          this.OpenChestForceBomb(ghost.PlayerIndex);
        }

        TFGame.PlayerInputs[ghost.PlayerIndex].Rumble(0.5f, 12);
      }
    }

    [MonoModLinkTo("TowerFall.LevelEntity", "Update")]
    [MonoModIgnore]
    public extern void base_Update();

    public void patch_Update ()
    {
      base_Update ();
      if ((bool)this.shakeCounter) {
        this.shakeCounter.Update ();
        if ((bool)this.shakeCounter) {
          this.sprite.X = (float)Calc.Choose (Calc.Random, -1, 0, 1);
        } else {
          this.sprite.X = 0f;
        }
      }
      if (this.sprite.Scale != Vector2.One && this.State != States.Opening) {
        this.sprite.Scale.X = Calc.Approach (this.sprite.Scale.X, 1f, 0.01f * Engine.TimeMult);
        this.sprite.Scale.Y = Calc.Approach (this.sprite.Scale.Y, 1f, 0.01f * Engine.TimeMult);
      }
      switch (this.State) {
      case States.Appearing:
      case States.Opening:
        break;
      case States.WaitingToAppear:
        if (this.ReadyToAppear ()) {
          this.Appear ();
        }
        break;
      case States.Closed:
        if (base.Level.OnInterval (6)) {
          if (this.type == Types.Large) {
            base.Level.Particles.Emit (Particles.TreasureChestGlow, 1, base.Position, new Vector2 (10f, 5f));
          } else {
            base.Level.Particles.Emit (Particles.TreasureChestGlow, 1, base.Position, Vector2.One * 5f);
          }
        }
        if (!base.CheckBelow ()) {
          this.vSpeed = Calc.Approach (this.vSpeed, GetMaxFall(), GetGravity() * Engine.TimeMult);
        } else if (this.type == Types.AutoOpen) {
          this.OpenChest (-1);
        }
        if (this.vSpeed != 0f) {
          base.MoveV (this.vSpeed * Engine.TimeMult, this.hitFloor);
        }
        break;
      case States.Opened:
        if (!base.CheckBelow ()) {
          this.vSpeed = Calc.Approach (this.vSpeed, GetMaxFall(), GetGravity() * Engine.TimeMult);
        }
        if (this.vSpeed != 0f) {
          base.MoveV (this.vSpeed * Engine.TimeMult, this.hitFloor);
        }
        break;
      }
    }

    public bool IsAntiGrav()
    {
      return patch_Level.IsAntiGrav();
    }

    public float GetGravity()
    {
      return 0.2f;
    }

    public float GetMaxFall()
    {
      return IsAntiGrav() ? -3.6f : 3.6f;
    }
  }
}
