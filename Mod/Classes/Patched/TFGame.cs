#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
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
using TowerFall;

namespace TowerFall
{
  class patch_TFGame : TFGame
  {
    public static Atlas ModAtlas {
      get;
      set;
    }

    public patch_TFGame(bool noIntro) : base(noIntro) {
      // no-op. MonoMod ignores this - we only need this to make the compiler shut up.
    }

    // Skip intro
    public extern void orig_ctor(bool noIntro);
    [MonoModConstructor]
    public void ctor(bool noIntro) {
      orig_ctor(noIntro);
      this.noIntro = true;
    }

    public extern void orig_Initialize();
    public void patch_Initialize()
    {
      orig_Initialize();
      // MInput.GamepadVibration does not exist in 8-Player Windows
      #if (!(EIGHT_PLAYER && WINDOWS))
        // Fix bug where rumble always initializes to enabled
        MInput.GamepadVibration = SaveData.Instance.Options.GamepadVibration;
      #endif

      InitCustomCommands();
    }

    public void InitCustomCommands()
    {
      Commands commands = Engine.Instance.Commands;
      commands.RegisterCommand("gravity", delegate {
        if (base.Scene is Level) {
          patch_Level level = ((patch_Level)(base.Scene));
          bool antiGravEnabled = level.ToggleGravity();
          if (antiGravEnabled) {
            commands.Log("Anti-Gravity Enabled");
          } else {
            commands.Log("Anti-Gravity Disabled");
          }
        } else {
          commands.Log("Command can only be used during gameplay!");
        }
      });
    }

    public extern void orig_LoadContent();
    public void patch_LoadContent()
    {
      orig_LoadContent();

      if (patch_TFGame.ModAtlas == null) {
        patch_TFGame.ModAtlas = new Atlas ("Atlas/modAtlas.xml", "Atlas/modAtlas.png", true);
      } else {
        patch_TFGame.ModAtlas.Load ();
      }
    }
  }
}
