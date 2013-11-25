using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Life
{
    class Camera
    {
        Rectangle simulationArea; //simulation are
        Vector2 position; //upper left corner
        Vector2 screenSize;
        float scale = 1f;
        float moveSpeed = 100f;

        float maxScale;
        float minScale;

        public Rectangle SimulationArea
        {
            get { return simulationArea; }
            set 
            { 
                simulationArea = value;
                calculateScaleConstraints();
            }
        }

        public float Scale
        {
            get { return scale; }
            set { 
                scale = MathHelper.Clamp(value, minScale, maxScale);
                if (scale == minScale)
                {
                    position = new Vector2(simulationArea.Width, simulationArea.Height) * Cell.size / 2;
                }
            }
        }

        public Matrix TransformationMatrix
        {
            get { return  Matrix.CreateTranslation(new Vector3(-position, 0f)) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(new Vector3(screenSize / 2, 0f))  /*Matrix.CreateRotationZ((float)Math.PI / 4)*/; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public void move(float x, float y)
        {
            position.X += x * moveSpeed;
            position.Y += y * moveSpeed;

            move(new Vector2(x, y) * moveSpeed);
        }

        public void move(Vector2 moveVector)
        {
            position += moveVector / scale;

            position.X = MathHelper.Clamp(position.X, 0, (simulationArea.Width) * Cell.size);
            position.Y = MathHelper.Clamp(position.Y, 0, (simulationArea.Height) * Cell.size);
        }

        /// <summary>
        /// Creates instance of Camera
        /// </summary>
        /// <param name="simulationArea">Area of simulation</param>
        public Camera(Rectangle simulationArea)
        {
            this.simulationArea = simulationArea;
            screenSize = Game1.windowSize;
            screenSize.Y -= ControlPanel.height;
            position = new Vector2(simulationArea.Width, simulationArea.Height) / 2;

            calculateScaleConstraints();
        }

        void calculateScaleConstraints()
        {
            float minW = screenSize.X / simulationArea.Width,
                minH = screenSize.Y / simulationArea.Height;

            minScale = Math.Min(minW / Cell.size, minH / Cell.size);
            maxScale = 50f / Cell.size;
            if (maxScale < minScale) maxScale = minScale;

            Scale = 1f;
        }
    }
}
