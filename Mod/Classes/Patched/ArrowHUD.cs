#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using Monocle;
using System;
using Microsoft.Xna.Framework;

namespace TowerFall
{
  class patch_ArrowHUD : ArrowHUD
  {

    [MonoModLinkTo("Monocle.Component", "System.Void .ctor(System.Boolean,System.Boolean)")]
    [MonoModRemove]
    public extern void base_ctor(bool active, bool visible);

    [MonoModConstructor]
    public void ctor()
    {
      base_ctor(true, false);
      this.images = new Subtexture[12] {
        TFGame.Atlas ["player/arrowHUD/arrow"],
        TFGame.Atlas ["player/arrowHUD/bombArrow"],
        TFGame.Atlas ["player/arrowHUD/superBombArrow"],
        TFGame.Atlas ["player/arrowHUD/laserArrow"],
        TFGame.Atlas ["player/arrowHUD/brambleArrow"],
        TFGame.Atlas ["player/arrowHUD/drillArrow"],
        TFGame.Atlas ["player/arrowHUD/boltArrow"],
        TFGame.Atlas ["player/arrowHUD/squeakyArrow"],
        TFGame.Atlas ["player/arrowHUD/featherArrow"],
        TFGame.Atlas ["player/arrowHUD/triggerArrow"],
        TFGame.Atlas ["player/arrowHUD/prismArrow"],
        patch_TFGame.ModAtlas ["player/arrowHUD/ghost"]
      };
      this.emptyImage = TFGame.Atlas ["player/noArrowHUD"];
      this.triggerImage = TFGame.Atlas ["player/arrowHUD/trigger"];
      this.showCounter = new Counter ();
      this.triggerColor = ArrowHUD.TriggerColorA;
    }

    public void patch_Render ()
    {
      // Unchanged from original except...
      int amountOfTriggerArrowsActive = this.player.AmountOfTriggerArrowsActive;
      int arrowCountIncludingActiveTriggerArrows = this.player.Arrows.Count + amountOfTriggerArrowsActive;
      if (arrowCountIncludingActiveTriggerArrows > 0) {
        bool flag = this.player.Level.Session.MatchSettings.Variants.SecretArrows [this.player.PlayerIndex];
        if (!flag && !this.player.Level.Session.MatchSettings.SoloMode) {
          flag = (this.player.State == Player.PlayerStates.Ducking || this.player.DodgeSliding || this.player.Invisible);
        }
        if ((bool)this.showCounter || this.player.Aiming || !flag) {
          float x = -4f * ((float)arrowCountIncludingActiveTriggerArrows * 0.5f);
          for (int i = 0; i < amountOfTriggerArrowsActive; i++) {
            Draw.Texture (
              this.triggerImage,
              Calc.Floor(
                this.player.Position +
                new Vector2(x, patch_Level.IsAntiGrav() ? 12f : -20f) +
                new Vector2 ((float)(4 * i), 0f)
              ),
              this.triggerColor
            );
          }
          for (int i = 0; i < this.player.Arrows.Count; i++) {
            int arrowTypeInt = (int)this.player.Arrows.Arrows[i];
            // ...here where we use Arrow.GetColor instead of Arrow.Colors
            Draw.Texture(
              this.images[arrowTypeInt],
              Calc.Floor(
                this.player.Position +
                new Vector2 (x, patch_Level.IsAntiGrav() ? 10f : -22f) +
                new Vector2((float)(4 * (i + amountOfTriggerArrowsActive)), 0f)
              ),
              patch_Arrow.GetColor(arrowTypeInt)
            );
          }
        }
      } else if ((bool)this.showCounter) {
        Draw.TextureCentered(
          this.emptyImage,
          this.player.Position +
          new Vector2 (0f, patch_Level.IsAntiGrav() ? 16f : -18f),
          Arrow.NoneColors[(int)this.player.Level.FrameCounter / 6 % Arrow.NoneColors.Length]
        );
      }
    }
  }
}