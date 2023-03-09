#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System;
using Mod;

namespace TowerFall
{
  public class patch_PlayerHair : PlayerHair
  {
    public patch_PlayerHair(Entity follow, Vector2 position, float scale) : base(follow, position, scale)
    {
      // no-op. MonoMod ignores this
    }

    public int GetAntiGravAdjustment()
    {
      return 12 - ((int)(this.Position.Y)) * 2 - 14;
    }

    public void patch_Update ()
    {
      this.sine.Update ();
      this.timeSinceLastUpdate += Engine.DeltaTime;
      if (!(this.timeSinceLastUpdate < 0.0166666675f)) {
        this.timeSinceLastUpdate -= 0.0166666675f;
        Vector2 value = this.Follow.Position - this.previousEntityPosition;
        Vector2 vector = (this.Position - this.previousPosition) * 0.2f + value + this.addSpeed;
        if (this.addSpeed != Vector2.Zero) {
          this.addSpeed = Calc.Approach (this.addSpeed, Vector2.Zero, 0.1f);
        }
        this.previousEntityPosition = this.Follow.Position;
        this.previousPosition = this.Position;
        if (vector == Vector2.Zero) {
          vector = -Vector2.UnitY * 0.2f;
        }
        float scaleFactor = vector.LengthSquared () / 9f;
        for (int i = 1; i < this.links; i++) {
          Vector2 vector2 = this.offsets [i - 1];
          Vector2 vector3 = -vector * scaleFactor * 0.5f;
          vector3 = Vector2.Lerp (vector3, this.offsets [i] - vector2, (float)i / (float)this.offsets.Length * 0.3f);

          if (patch_Level.IsAntiGrav()) {
            vector3.Y -= 0.06f + 0.04f * this.sine.Value * Math.Abs (value.X);
          } else {
            vector3.Y += 0.06f + 0.04f * this.sine.Value * Math.Abs (value.X);
          }

          if (Math.Abs (value.X) <= 0.2f) {
            vector3.X += 0.08f * this.sine.Value * Math.Abs (value.Y);
          }
          vector3.Normalize ();
          vector3 *= this.linkDist;
          this.offsets[i] = vector2 + vector3;
        }
      }
    }

    public void patch_Render ()
    {
      for (int i = 0; i < this.links; i++) {
        Vector2 position = this.Follow.Position + this.Position + this.offsets [i];

        if (patch_Level.IsAntiGrav()) {
          position.Y += GetAntiGravAdjustment();
        }

        float rotation = (i == 0) ? 0f : Calc.Angle (this.offsets [i], this.offsets [i - 1]);
        Draw.TextureCentered (this.images [i], position, Color.White * this.Alpha * this.Alpha, this.scale, rotation);
      }
    }

    public void patch_RenderBlack ()
    {
      for (int i = 0; i < this.links; i++) {
        Vector2 position = this.Follow.Position + this.Position + this.offsets [i];

        if (patch_Level.IsAntiGrav()) {
          position.Y += GetAntiGravAdjustment();
        }

        float rotation = (i == 0) ? 0f : Calc.Angle (this.offsets [i], this.offsets [i - 1]);
        Draw.TextureCentered (this.images [i], position, Color.Black * this.Alpha * this.Alpha, this.scale, rotation);
      }
    }

    public void patch_RenderOutline ()
    {
      for (int i = 0; i < this.links; i++) {
        Vector2 value = this.Follow.Position + this.Position + this.offsets [i];

        if (patch_Level.IsAntiGrav()) {
          value.Y += GetAntiGravAdjustment();
        }

        float rotation = (i == 0) ? 0f : Calc.Angle (this.offsets [i], this.offsets [i - 1]);
        for (int j = -1; j < 2; j++) {
          for (int k = -1; k < 2; k++) {
            if (j != 0 || k != 0) {
              Draw.TextureCentered (this.images [i], value + new Vector2 ((float)j, (float)k), Color.Black, this.scale, rotation);
            }
          }
        }
      }
    }
  }
}
