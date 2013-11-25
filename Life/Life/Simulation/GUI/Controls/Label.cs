using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Life
{
    class Label:Control
    {
        SpriteFont font;

        string 
            label,
            value,
            caption;

        public Color Color
        {
            get { return color; }
            set { this.color = value; }
        }

        #region Value setters
        public void setValue(string value)
        {
            this.value = value;
            this.caption = string.IsNullOrEmpty(label)?value:string.Format("{0}: {1}", label, value);
        }

        public void setValue()
        {
            setValue("");
        }

        public void setValue(float value)
        {
            setValue(value.ToString("#"));
        }

        public void setValue(int value)
        {
            setValue(value.ToString());
        }

        public void setValue(Point value)
        {
            setValue(string.Format("[{0},{1}]", value.X, value.Y));
        }

        public void setValue(Vector2 value)
        {
            setValue(string.Format("[{0},{1}]", value.X, value.Y));
        }
        #endregion

        #region Constructors
        public Label(Vector2 position, SpriteFont font)
        {
            this.position = position;
            this.font = font;
            this.color = Color.Black;
            setValue();
        }

        public Label(Vector2 position, SpriteFont font, string label)
            :this(position, font)
        {
            this.label = label;
        }

        public Label(Vector2 position, SpriteFont font, string name, string value)
            : this(position, font, name)
        {
            setValue(value);
        }

        public Label(Vector2 position, SpriteFont font, string name, int value)
            : this(position, font, name)
        {
            setValue(value);
        }

        public Label(Vector2 position, SpriteFont font, string name, float value)
            : this(position, font, name)
        {
            setValue(value);
        }
        #endregion

        #region Public methods
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(font, caption, position, color);
        }

        public override void Update(GameTime gameTime)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
