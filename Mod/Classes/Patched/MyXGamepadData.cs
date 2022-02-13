using Microsoft.Xna.Framework;
using Patcher;

namespace Monocle
{
  [Patch]
  public abstract class MyXGamepadData : MInput.XGamepadData
  {
    public MyXGamepadData (PlayerIndex playerIndex) : base(playerIndex)
    {
    }

    public override void StopRumble ()
    {
      #if (!(EIGHT_PLAYER && WINDOWS))
        // Prevent MacOS freeze with rumble off.
        if (MInput.GamepadVibration) {
          base.StopRumble();
        }
      #else
        base.StopRumble();
      #endif
    }
  }
}
