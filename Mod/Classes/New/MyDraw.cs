using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Monocle;

namespace Mod
{
  public abstract class MyDraw
  {
    public static void OutlineTextureCentered(Subtexture subTexture, Vector2 position, Color color, float rotation)
    {
      for (int i = -1; i <= 1; i++) {
        for (int j = -1; j <= 1; j++) {
          if (i != 0 || j != 0) {
            Draw.SpriteBatch.Draw(
              subTexture.Texture.Texture2D,
              Calc.Floor(position) + new Vector2 ((float)i, (float)j),
              subTexture.Rect,
              Color.Black,
              rotation,
              new Vector2 ((float)(subTexture.Rect.Width / 2),
              (float)(subTexture.Rect.Height / 2)), 1f, SpriteEffects.None, 0f
            );
          }
        }
      }
      Draw.SpriteBatch.Draw(
        subTexture.Texture.Texture2D,
        Calc.Floor(position),
        subTexture.Rect,
        color,
        rotation,
        new Vector2 ((float)(subTexture.Rect.Width / 2),
        (float)(subTexture.Rect.Height / 2)),
        1f,
        SpriteEffects.None,
        0f
      );
    }
  }
}
