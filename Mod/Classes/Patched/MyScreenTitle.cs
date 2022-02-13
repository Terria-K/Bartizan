using TowerFall;
using Patcher;

namespace Mod
{
  [Patch]
  class MyScreenTitle : ScreenTitle
  {
    public MyScreenTitle(MainMenu.MenuState state) : base(state)
    {
      #if (STAT_TRACKING)
        this.textures [(MainMenu.MenuState)MyMainMenu.ROSTER] = TFGame.MenuAtlas ["menuTitles/roster"];
      #endif
    }
  }
}