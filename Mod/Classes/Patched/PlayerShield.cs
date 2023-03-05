#pragma warning disable CS0108 // 'member1' hides inherited member 'member2'. Use the new keyword if hiding was intended.

using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace TowerFall
{
  public abstract class patch_PlayerShield : PlayerShield
  {
    [MonoModPublic]
    public SpritePart<int> sprite;

    public patch_PlayerShield (LevelEntity owner) : base (owner)
    {
      // No-op. MonoMod ignores this
    }
  }
}