#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Monocle;
using MonoMod;
using System;

namespace TowerFall
{
  public class patch_OrbLogic : OrbLogic
  {
    public Counter reverseGravityStartCounter = new Counter ();

    public Counter reverseGravityEndCounter = new Counter ();

    public patch_OrbLogic(Level level) : base(level)
    {
      // no-op. MonoMod ignores this
    }

    public extern void orig_ctor(Level level);
    [MonoModConstructor]
    public void ctor(Level level)
    {
      orig_ctor(level);
      // Need to set these here otherwise there's a NullReferenceException in Update
      this.reverseGravityStartCounter = new Counter();
      this.reverseGravityEndCounter = new Counter();
    }


    public extern void orig_Update();
    public void patch_Update()
    {
      orig_Update();
      if ((bool)this.reverseGravityEndCounter) {
        this.reverseGravityEndCounter.Update();
        if (!(bool)this.reverseGravityEndCounter) {
          patch_Level.SetReverseGravity(false);
        }
      } else if ((bool)this.reverseGravityStartCounter) {
        this.reverseGravityStartCounter.Update();
        if (!(bool)this.reverseGravityStartCounter) {
          this.reverseGravityEndCounter.Set(Calc.Range (Calc.Random, 600, 300));
          patch_Level.SetReverseGravity(true);
        }
      }
    }


    public void DoReverseGravityOrb()
    {
      if (!this.Level.Ending) {
        if ((bool)this.reverseGravityEndCounter) {
          this.reverseGravityEndCounter.SetMax(Calc.Range(Calc.Random, 600, 300));
        } else {
          this.reverseGravityStartCounter.Set(1);
        }
      }
    }
  }
}