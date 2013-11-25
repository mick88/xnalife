using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Life
{
    /// <summary>
    /// A control used for selecting spawnable shape
    /// </summary>
    class ShapePicker:Control
    {
        Texture2D cellTexture;
        Color cellColor;
        bool[,] shape;
        Input input;
        float cellSize = 0;
        Rectangle area;
        const float cellSpacing = 1f;
        const int frameSize = 1;

        public override Rectangle Area
        {
            get
            {
                return area;
            }
        }

        public ShapePicker(Texture2D texture, Rectangle area, Input input)
        {
            this.texture = texture;
            this.area = area;
            this.position.X = area.X;
            this.position.Y = area.Y;
            this.color = Color.Black;
            this.cellTexture = Cell.texture;
            this.cellColor = Color.White;
            this.input = input;
        }        

        public override void Update(GameTime gameTime)
        {
            shape = input.SelectedCreature;
            cellSize = Math.Min(
                (area.Width / shape.GetLength(0)) - cellSpacing,
                (area.Height / shape.GetLength(1)) - cellSpacing);

            cellColor = input.SelectedColor;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, area, color);
            if (shape != null && cellTexture != null)
            {
                for (int x = 0; x < shape.GetLength(0); x++)
                {
                    for (int y = 0; y < shape.GetLength(1); y++)
                    {
                        if (shape[x, y] == true) spriteBatch.Draw(cellTexture, new Rectangle((int)(x * (cellSize+cellSpacing))+area.X+frameSize, (int)(y * (cellSize+cellSpacing))+area.Y+frameSize, (int)cellSize, (int)cellSize), cellColor);
                    }
                }
            }
        }
    }
}
