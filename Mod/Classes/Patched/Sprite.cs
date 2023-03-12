#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TowerFall;

namespace Monocle
{
  public class patch_Sprite<T> : Sprite<T>
  {
    public patch_Sprite(Subtexture subTexture, int frameWidth, int frameHeight, int frameSep = 0)
      : base (subTexture, frameWidth, frameHeight, frameSep)
    {
    }

    public T ReplaceIdIfAntiGrav(T id)
    {
      Type itemType = typeof(T);
      if (itemType == typeof(string)) {
        if (!patch_Level.IsAntiGrav() ) {
          return id;
        }
        switch ((string)(object)id) {
          case "lookUp":
            return (T)(object)"lookDown";
          case "lookDown":
            return (T)(object)"lookUp";
          case "lookUpJump":
            return (T)(object)"lookDownJump";
          case "lookDownJump":
            return (T)(object)"lookUpJump";
          case "lookUpFall":
            return (T)(object)"lookDownFall";
          case "lookDownFall":
            return (T)(object)"lookUpFall";
          default:
            return id;
        }
      }
      return id;
    }

    public extern void orig_Play(T id, bool restart = false);
    public void patch_Play(T id, bool restart = false)
    {
      orig_Play(ReplaceIdIfAntiGrav(id), restart);
    }
  }
}
