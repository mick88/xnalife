using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Life
{
    class Button:Control
    {
        public enum ButtonValue
        {
            StartPause,
            Faster,
            Slower,
            ColorSelect,
            Clear,
            LoadFile,
            SaveFile,
            Random,
            NextShape,
            PrevShape
        }

        public static Color 
            colorDefault = Color.Yellow,
            colorHover = Color.Green,
            colorOn = Color.Orange;
        Texture2D symbol=null;
        ControlPanel controlPanel;
        Simulation simulation;
        ButtonValue buttonValue;
        Rectangle destinationRectangle;
        bool isOn = false,
            isPressed = false;
        int numericValue;

        public int NumValue
        {
            get { return numericValue; }
        }

        public bool IsOn
        {
            get { return isOn; }
            set { 
                isOn = value;
                color = isOn ? colorOn : colorDefault;
            }
        }

        public ButtonValue Value
        {
            get { return buttonValue; }
            set { buttonValue = value; }
        }

        public Color Color
        {
            set { color = value; }
            get { return color; }
        }

        public Button(ControlPanel controlPanel, Texture2D texture, Vector2 position, ButtonValue buttonValue)
        {
            this.controlPanel = controlPanel;
            this.texture = texture;
            this.color = colorDefault;
            this.position = position;
            this.buttonValue = buttonValue;
            this.simulation = controlPanel.simulation;
        }

        public Button(ControlPanel controlPanel, Texture2D texture, Vector2 position, ButtonValue buttonValue, int numericValue)
            :this(controlPanel, texture, position, buttonValue)
        {
            this.numericValue = numericValue;
        }

        public Button(ControlPanel controlPanel, Texture2D texture, Vector2 position, ButtonValue buttonValue, Texture2D symbol)
            : this(controlPanel, texture, position, buttonValue)
        {
            this.symbol = symbol;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager contentManager)
        {
            throw new NotImplementedException();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (texture != null)
            {
                spriteBatch.Draw(texture, destinationRectangle, color);
                if (symbol != null)
                {
                    spriteBatch.Draw(symbol, destinationRectangle, Color.White);
                }
            }
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            destinationRectangle = Area;

            if (Area.Contains(controlPanel.mousePosition) == true)
            {
                bool mousePressed = (simulation.input.LastMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed);              

                if (mousePressed != isPressed)
                {
                    if (mousePressed == true) controlPanel.onButtonPressed(buttonValue, this);
                    isPressed = mousePressed;
                }

                if (!isPressed)
                {
                    destinationRectangle.Inflate(3, 3);
                    if ((buttonValue == ButtonValue.Faster || buttonValue == ButtonValue.Slower) && simulation.input.mouseWheelMove != 0)
                    {
                        simulation.GenerationsPerSecond += (float)(simulation.input.mouseWheelMove) * 0.01f;
                    }
                }
            }
        }
    }
}
