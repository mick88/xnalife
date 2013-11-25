using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Life
{
    class Input
    {
        Camera camera;
        Simulation simulation;
        Texture2D cursor;
        Color selectorKillColor = Color.Red,
            selectorReviveColor = Color.Green;

        Color selectedColor;

        /*Used for mouse controls:*/
        Point SelectedCell;
        MouseState lastMouseState = new MouseState();
        KeyboardState lastKeyboardState = new KeyboardState();
        public Vector2 mousePosition;
        public int mouseWheelMove = 0;

        int selectedCreature = 0;
        List<bool[,]> spawnableCreatures;

        public MouseState LastMouseState
        {
            get { return lastMouseState; }
        }

        public int SelectedCreatureID
        {
            get { return selectedCreature; }
            set { selectedCreature = (int)MathHelper.Clamp(value, 0, spawnableCreatures.Count-1); }
        }

        public bool[,] SelectedCreature
        {
            get { return spawnableCreatures[selectedCreature]; }
        }

        public Color SelectedColor
        {
            get { return selectedColor; }
            set { selectedColor = value; }
        }

        public Input(Simulation simulation)
        {
            this.camera = simulation.camera;
            this.simulation = simulation;

            this.selectedColor = simulation.baseColors[simulation.random.Next() % simulation.baseColors.Length];

            loadCreatures();
        }

        public void update(GameTime gameTime)
        {
            double timeDelta = gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboardState = Keyboard.GetState();

            /*Zoom buttons*/
            if (keyboardState.IsKeyDown(Keys.PageUp) || keyboardState.IsKeyDown(Keys.Add))
            {
                camera.Scale += (float)timeDelta;
            }
            if (keyboardState.IsKeyDown(Keys.PageDown) || keyboardState.IsKeyDown(Keys.Subtract))
            {
                camera.Scale -= (float)timeDelta;
            }

            /*Arow buttons*/
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S)) camera.move(0f, (float)timeDelta);
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W)) camera.move(0f, -(float)timeDelta);
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D)) camera.move((float)timeDelta, 0f);
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A)) camera.move(-(float)timeDelta, 0f);

            /*Numeric buttons*/
            if (keyboardState.IsKeyDown(Keys.D1)) SelectedCreatureID = 0;
            else if (keyboardState.IsKeyDown(Keys.D2)) SelectedCreatureID = 1;
            else if (keyboardState.IsKeyDown(Keys.D3)) SelectedCreatureID = 2;
            else if (keyboardState.IsKeyDown(Keys.D4)) SelectedCreatureID = 3;
            else if (keyboardState.IsKeyDown(Keys.D5)) SelectedCreatureID = 4;
            else if (keyboardState.IsKeyDown(Keys.D6)) SelectedCreatureID = 5;
            else if (keyboardState.IsKeyDown(Keys.D7)) SelectedCreatureID = 6;
            else if (keyboardState.IsKeyDown(Keys.D8)) SelectedCreatureID = 7;
            else if (keyboardState.IsKeyDown(Keys.D9)) SelectedCreatureID = 8;
            else if (keyboardState.IsKeyDown(Keys.D0)) SelectedCreatureID = 9;

            if (keyboardState.IsKeyDown(Keys.Q) && lastKeyboardState.IsKeyUp(Keys.Q)) SelectedCreatureID--;
            if (keyboardState.IsKeyDown(Keys.E) && lastKeyboardState.IsKeyUp(Keys.E)) SelectedCreatureID++;

            if (keyboardState.IsKeyDown(Keys.Space) && lastKeyboardState.IsKeyUp(Keys.Space)) simulation.Paused = !simulation.Paused;

            /*Mouse controls*/
            MouseState mouseState = Mouse.GetState();
            //Point mouseScreenPos = new Point(mouseState.X, mouseState.Y);
            mousePosition = new Vector2(mouseState.X, mouseState.Y);
            mouseWheelMove = mouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue;

            if (simulation.controlPanel.controlPanelArea.Contains(new Point(mouseState.X, mouseState.Y)) == false)
            {
                Vector2 mouseMove = new Vector2(mouseState.X - lastMouseState.X, mouseState.Y - lastMouseState.Y);                

                /*mouseMove around*/
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    camera.move(-mouseMove);
                }
                /*mouse zoom*/
                if (mouseWheelMove != 0)
                {
                    camera.Scale += (mouseWheelMove * 0.005f) * camera.Scale;
                }

                Vector2 MouseCell = Vector2.Transform(mousePosition, Matrix.Invert(camera.TransformationMatrix));
                SelectedCell = new Point((int)(MouseCell.X / Cell.size) - (spawnableCreatures[SelectedCreatureID].GetLength(0) / 2), (int)(MouseCell.Y / Cell.size) - (spawnableCreatures[SelectedCreatureID].GetLength(1) / 2));

                /*spawn cells by mouseclick*/
                if (mouseState.LeftButton == ButtonState.Pressed && (lastMouseState.LeftButton == ButtonState.Released || mouseMove != Vector2.Zero))
                {
                    simulation.spawnCreature(SelectedCell, spawnableCreatures[selectedCreature], selectedColor);
                    //selectedColor = simulation.baseColors[simulation.random.Next() % simulation.baseColors.Length];
                }
                //simulation.Paused = (mouseState.LeftButton == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Space));
            }
            lastMouseState = mouseState;
            lastKeyboardState = keyboardState;
        }

        enum FlipType
        {
            noFlip,
            xAxis,
            yAxis,
        }

        bool[,] flipArray(bool[,] array, FlipType flipType)
        {
            int width = array.GetLength(0),
                height = array.GetLength(1);
            bool[,] result = new bool [width,height];

            switch (flipType)
            {
                case FlipType.xAxis:
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            result[x,y] = array[width-x-1,y];
                        }
                    }
                        break;
                case FlipType.yAxis:
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                result[x, y] = array[x, y-height-1];
                            }
                        }
                    break;
                default:
                    result = array;
                    break;
            }
            return result;
        }

        public void draw(SpriteBatch spriteBatch)
        {
            Rectangle allowedArea = simulation.GridArea;

            /*Draw selector*/
            bool[,] selCells = spawnableCreatures[SelectedCreatureID];
            for (int y = 0; y < selCells.GetLength(1); y++)
            {
                for (int x = 0; x < selCells.GetLength(0); x++)
                {
                    /*draws only on allowed portion of the screen*/
                    Rectangle rect = new Rectangle((SelectedCell.X + x) * Cell.size, (SelectedCell.Y + y) * Cell.size, Cell.size, Cell.size);
                    if (allowedArea.Contains(rect) == false) continue;

                    spriteBatch.Draw(cursor, rect, new Rectangle(0, 0, cursor.Width, cursor.Height), selCells[x, y] ? /*selectorReviveColor*/selectedColor : /*selectorKillColor*/Color.Gray);
                }
            }
        }

        public void loadContent(ContentManager contentManager)
        {
            cursor = contentManager.Load<Texture2D>(@"Textures\SelectedCell");
        }

        void loadCreatures()
        {
            spawnableCreatures = new List<bool[,]>();

            spawnableCreatures.Add(new bool[,] { { true } });
            spawnableCreatures.Add(new bool[,]
            {
                {false, false, true},
                {true, true, true},
                {false, true, false},
            });
            spawnableCreatures.Add(new bool[,] { { true, true, true } });

            spawnableCreatures.Add(new bool[,] { 
            { false, false, false },
            { false, false, false },
            { false, false, false }});
            spawnableCreatures.Add(new bool[,] { { true, true }, { true, true } });
            spawnableCreatures.Add(new bool[,]
            {
                {false, true, false},
                {false, false, true},
                {true, true, true},
            });

            spawnableCreatures.Add(new bool[,]
            {
                {false, true, true, true, true},
                {true, false, false, false, true},
                {false, false, false, false, true},
                {true, false, false, true, false},
            });

            spawnableCreatures.Add(new bool[,]
            {
                {true, true, true, false, true},
                {true, false, false, false, false},
                {false, false, false, true, true},
                {false, true, true, false, true},
                {true, false, true, false, true},
            });

            spawnableCreatures.Add(new bool[,]{
                {false, false, false, false, false, false, false, false},
                {false, false, false, false, false, false, false, false},
                {false, false, false, false, false, false, false, false},
                {false, false, false, false, false, false, false, false},
                {false, false, false, false, false, false, false, false},
                {false, false, false, false, false, false, false, false},
                {false, false, false, false, false, false, false, false},
                {false, false, false, false, false, false, false, false},
            });

            spawnableCreatures.Add(new bool[,] { 
            { true, true, true, true, true },
           { true, true, true, true, true },
           { true, true, true, true, true },
           { true, true, true, true, true },
           { true, true, true, true, true },
            });

        }
    }
}
