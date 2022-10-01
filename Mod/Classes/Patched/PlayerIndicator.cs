#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using Mod;
using Monocle;
using MonoMod;
using System;

namespace TowerFall
{
  public class patch_PlayerIndicator : PlayerIndicator
  {
    public const int MAX_NAME_LENGTH = 9;

    public patch_PlayerIndicator (Vector2 offset, int playerIndex, bool crown)
      : base (offset, playerIndex, crown)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(Vector2 offset, int playerIndex, bool crown);
    [MonoModConstructor]
    public void ctor(Vector2 offset, int playerIndex, bool crown)
    {
      orig_ctor(offset, playerIndex, crown);

      if (MyGlobals.playerNames != null) {
        string playerName = MyGlobals.playerNames.GetName(playerIndex);
        this.text = playerName.ToUpper().Substring(
          0,
          Math.Min(playerName.Length, MAX_NAME_LENGTH)
        );
      } else {
        this.text = "P" + (playerIndex + 1).ToString();
      }
    }

    public void patch_Render ()
    {
      Color color = this.colorSwitch ? ArcherData.Archers [this.characterIndex].ColorB : ArcherData.Archers [this.characterIndex].ColorA;
      Vector2 value = base.Entity.Position + this.offset + new Vector2 (0f, -32f);
      value.Y = Math.Max (10f, value.Y);
      value.Y += this.sine.Value * 3f;
      Vector2 vector = TFGame.Font.MeasureString (this.text) * 2f;
      if (this.crown) {
        Draw.OutlineTextureCentered (TFGame.Atlas ["versus/crown"], value + new Vector2 (0f, -12f), Color.White);
      }
      float textScale;
      if (this.text.Length > 2) {
        textScale = 1.0f;
      } else {
        textScale = 2.0f;
      }
      Draw.OutlineTextCentered (TFGame.Font, this.text, value + new Vector2 (1f, 0f), color, textScale);
      Draw.OutlineTextureCentered (TFGame.Atlas ["versus/playerIndicator"], value + new Vector2 (0f, 8f), color);
    }
  }
}
