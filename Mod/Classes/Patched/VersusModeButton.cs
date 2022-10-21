#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using Mod;

using System;

namespace TowerFall
{
  public class patch_VersusModeButton : VersusModeButton
  {
    static List<Modes> VersusModes = new List<Modes> {
      Modes.LastManStanding,
      Modes.HeadHunters,
      Modes.TeamDeathmatch,
      RespawnRoundLogic.Mode,
      MobRoundLogic.Mode,
    };

    public patch_VersusModeButton(Vector2 position, Vector2 tweenFrom)
      : base(position, tweenFrom)
    {
      // no-op. MonoMod ignores this
    }

    public extern static string orig_GetModeName(Modes mode);
    public new static string GetModeName(Modes mode)
    {
      switch (mode) {
        case RespawnRoundLogic.Mode:
          return "RESPAWN";
        case MobRoundLogic.Mode:
          return "CRAWL";
        default:
          return orig_GetModeName(mode);
      }
    }

    public extern static Subtexture orig_GetModeIcon(Modes mode);
    public new static Subtexture GetModeIcon(Modes mode)
    {
      switch (mode) {
        case RespawnRoundLogic.Mode:
          return patch_TFGame.ModAtlas["gameModes/respawn"];
        case MobRoundLogic.Mode:
          return patch_TFGame.ModAtlas["gameModes/crawl"];
        default:
          return orig_GetModeIcon(mode);
      }
    }

    [MonoModLinkTo("TowerFall.BorderButton", "Update")]
    [MonoModIgnore]
    public extern void base_Update();
    // completely re-write to make it enum-independent
    public void patch_Update()
    {
      base_Update();

      Modes mode = MainMenu.VersusMatchSettings.Mode;
      if (this.Selected) {
        int idx = VersusModes.IndexOf(mode);
        if (idx < VersusModes.Count - 1 && MenuInput.Right) {
          MainMenu.VersusMatchSettings.Mode = VersusModes[idx + 1];
          Sounds.ui_move2.Play(160f, 1f);
          this.iconWiggler.Start();
          base.OnConfirm();
          this.UpdateSides();
        } else if (idx > 0 && MenuInput.Left) {
          MainMenu.VersusMatchSettings.Mode = VersusModes[idx - 1];
          Sounds.ui_move2.Play(160f, 1f);
          this.iconWiggler.Start();
          base.OnConfirm();
          this.UpdateSides();
        }
      }
    }

    public extern void orig_UpdateSides();
    public void patch_UpdateSides()
    {
      orig_UpdateSides();
      this.DrawRight = (MainMenu.VersusMatchSettings.Mode < VersusModes[VersusModes.Count-1]);
    }
  }
}
