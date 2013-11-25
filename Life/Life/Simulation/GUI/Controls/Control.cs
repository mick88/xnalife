using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Life
{
    abstract class Control
    {
        protected Vector2 position;
        protected Texture2D texture = null;
        protected Color color = Color.White;

        virtual public Rectangle Area
        {
            get
            {
                if (texture == null) return new Rectangle((int)position.X, (int)position.Y, 0, 0);
                return new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height); 
            }
        }

        abstract public void Draw(SpriteBatch spriteBatch);
        abstract public void Update(GameTime gameTime);
    }
}
