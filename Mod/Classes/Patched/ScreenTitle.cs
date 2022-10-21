#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'

using MonoMod;
using Monocle;
using System.Collections.Generic;

namespace TowerFall
{
  class patch_ScreenTitle
  {
    public Dictionary<MainMenu.MenuState, Subtexture> textures;

    public extern void orig_ctor(MainMenu.MenuState state);
    [MonoModConstructor]
    public void ctor(MainMenu.MenuState state)
    {
      orig_ctor(state);
      #if (STAT_TRACKING)
        this.textures [(MainMenu.MenuState)patch_MainMenu.ROSTER] = patch_TFGame.ModAtlas ["menuTitles/roster"];
      #endif
    }
  }
}
