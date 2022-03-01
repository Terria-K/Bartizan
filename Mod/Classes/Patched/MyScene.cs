using Patcher;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Mod;

namespace Monocle
{
  [Patch]
  public class MyScene : Scene
  {
    public MyScene(): base()
    {
      // Update tag count to include mod game tags
      int myGameTagsCount = Enum.GetNames(typeof(MyGlobals.GameTags)).Length;
      int tagAmount = Engine.TagAmount + myGameTagsCount;

      this.Layers = new Dictionary<int, Layer> ();
      this.Tags = new List<Entity>[tagAmount];
      this.tagsToRemove = new HashSet<Entity>[tagAmount];
      this.tagsToAdd = new HashSet<Entity>[tagAmount];
      for (int i = 0; i < tagAmount; i++) {
        this.Tags [i] = new List<Entity> ();
        this.tagsToRemove [i] = new HashSet<Entity> ();
        this.tagsToAdd [i] = new HashSet<Entity> ();
      }
      this.Camera = new Camera ();
    }
  }
}
