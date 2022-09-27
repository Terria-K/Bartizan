#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0114 // 'function1' hides inherited member 'function2'. To make the current method override that implementation, add the override keyword. Otherwise add the new keyword.

using MonoMod;

namespace TowerFall
{
  class patch_ChaliceGhost : ChaliceGhost
  {
    public patch_ChaliceGhost(int ownerIndex, Chalice source) : base(ownerIndex, source)
    {
      // no-op. MonoMod ignores this
    }

    [MonoModIgnore]
    [MonoModPublic]
    public extern void Vanish();
  }
}