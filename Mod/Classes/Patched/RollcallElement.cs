#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Mod;

namespace TowerFall
{
  public class patch_RollcallElement : RollcallElement
  {
    public patch_RollcallElement(int playerIndex) : base(playerIndex)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ForceStart();
    public void patch_ForceStart()
    {
      patch_VersusPlayerMatchResults.PlayerWins = new int[MyGlobals.MAX_PLAYERS];
      orig_ForceStart();
    }

    public extern void orig_StartVersus();
    public void patch_StartVersus()
    {
      patch_VersusPlayerMatchResults.PlayerWins = new int[MyGlobals.MAX_PLAYERS];
      orig_StartVersus();
    }
  }
}
