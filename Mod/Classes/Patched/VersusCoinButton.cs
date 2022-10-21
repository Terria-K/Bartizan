#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using MonoMod;
using Mod;

namespace TowerFall
{
  public class patch_VersusCoinButton : VersusCoinButton
  {
    public patch_VersusCoinButton(Vector2 position, Vector2 tweenFrom)
      : base(position, tweenFrom)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_Render();
    public void patch_Render()
    {
      var mode = MainMenu.VersusMatchSettings.Mode;
      if (mode == RespawnRoundLogic.Mode
        || mode == MobRoundLogic.Mode
      ) {
        MainMenu.VersusMatchSettings.Mode = Modes.HeadHunters;
        orig_Render();
        MainMenu.VersusMatchSettings.Mode = mode;
      } else {
        orig_Render();
      }
    }
  }
}
