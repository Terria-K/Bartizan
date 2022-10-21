#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using MonoMod;
using Mod;
using TowerFall;
using System;
using System.IO;

namespace TowerFall
{
  public class patch_MapScene : MapScene
    {
    private static bool initialLoad = true;

    public patch_MapScene(MainMenu.RollcallModes mode) : base(mode)
    {
      // no-op. Monomod ignores this
    }

    public extern void orig_ctor(MainMenu.RollcallModes mode);
    [MonoModConstructor]
    public void ctor(MainMenu.RollcallModes mode)
    {
      orig_ctor(mode);

      #if (STAT_TRACKING)
        TrackerApiClient client = new TrackerApiClient();
        client.GetPlayerNames();
      #endif
    }

    public extern void orig_InitVersusButtons();
    public void patch_InitVersusButtons()
    {
      orig_InitVersusButtons();

      string disabledMapsFile = Path.Combine(TrackerApiClient.GetSavePath(), "tf-disabled-maps.txt");

      if (initialLoad && File.Exists(disabledMapsFile)) {
        initialLoad = false;
        string[] disabledMaps = File.ReadAllLines(disabledMapsFile);

        if (disabledMaps != null && disabledMaps.Length != 0) {
          for (int i = 0; i < this.Buttons.Count; i++) {
            bool isDisabled = false;
            for (int j = 0; j < disabledMaps.Length; j++) {
              if (disabledMaps[j] == this.Buttons[i].Title) {
                isDisabled = true;
                break;
              }
            }
            if (isDisabled && !((VersusMapButton)this.Buttons[i]).NoRandom) {
              this.Buttons[i].AltAction();
            }
          }
        }
      }
    }
  }
}
