#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using System;

namespace Monocle
{
  class patch_MInput
  {
    class patch_XGamepadData : MInput.XGamepadData
    {
      public patch_XGamepadData (PlayerIndex playerIndex) : base(playerIndex)
      {
        // no-op. MonoMod ignores this
      }

      public extern void orig_StopRumble();
      public void patch_StopRumble()
      {
        #if (!(EIGHT_PLAYER && WINDOWS))
          // Prevent MacOS freeze with rumble off.
          if (MInput.GamepadVibration) {
            orig_StopRumble();
          }
        #else
          orig_StopRumble();
        #endif
      }
    }
  }
}