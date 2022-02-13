using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Patcher;
using TowerFall;

namespace Mod
{
  [Patch]
  public class MyTFGame : TFGame
  {
    public MyTFGame(bool noIntro) : base(noIntro)
    {
    }

    public override void Initialize ()
    {
      base.Initialize();
      // MInput.GamepadVibration does not exist in 8-Player Windows
      #if (!(EIGHT_PLAYER && WINDOWS))
        // Fix bug where rumble always initializes to enabled
        MInput.GamepadVibration = SaveData.Instance.Options.GamepadVibration;
      #endif
    }
  }
}
