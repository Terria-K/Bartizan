#pragma warning disable CS0108 // 'member1' hides inherited member 'member2'. Use the new keyword if hiding was intended.

using MonoMod;
using Monocle;
using System;

namespace TowerFall
{
  public class patch_OptionsButton : OptionsButton
  {
    [MonoModPublic]
    public string title;

    [MonoModPublic]
    public Wiggler changedWiggler;

    [MonoModPublic]
    public Wiggler selectedWiggler;

    [MonoModPublic]
    public int wiggleDir;

    [MonoModPublic]
    public Action onLeft;

    [MonoModPublic]
    public bool CanLeft;

    [MonoModPublic]
    public bool CanRight;

    [MonoModPublic]
    public Image rightArrow;

    [MonoModPublic]
    public Image leftArrow;

    [MonoModPublic]
    public SineWave sine;

    public patch_OptionsButton(string title) : base(title) {
      // no-op. MonoMod ignores this
    }
  }
}