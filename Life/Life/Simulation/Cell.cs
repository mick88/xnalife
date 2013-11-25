using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Life
{
    class Cell
    {
        public static int size=1;
        public static Texture2D texture;
        
        Color color = Color.Black,
            newColor= Color.Black;
        Point position;
        bool isAlive=false,
            newState;

        public Point Position
        {
            get { return position; }
        }

        List<Cell> neighbours; //adjacent cells

        public bool IsAlive
        {
            get { return isAlive; }
            set
            {
                if (value != isAlive)
                {
                    isAlive = value;
                    //color = isAlive ? AliveColor : DeadColor;
                }
            }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public void setState(bool newState)
        {
            this.newState = newState;
            this.newColor = color;
        }

        public void setState(bool newState, Color color)
        {
            setState(newState);
            this.newColor=color;
        }

        public bool applyNewState()
        {
            bool result = (isAlive != newState);
            IsAlive = newState;
            color = newColor;

            return result;
        }
        

        /// <summary>
        /// Creates a new Cell
        /// </summary>
        /// 
        /// <param name="x">X location in array</param>
        /// <param name="y">Y location in array</param>
        /// <param name="alive">Is cell alive</param>
        /// <param name="texture">Cell texture</param>
        public Cell(int x, int y, bool alive)
        {
            this.IsAlive = alive;
            this.newState = alive;
            this.position = new Point(x, y);
            this.neighbours = new List<Cell>();  
        }

        public Cell(int x, int y, bool alive, Color color):this(x, y, alive)
        {
            this.color = color;
            this.newColor = color;
        }

        public void addNeighbour(Cell adjacentCell)
        {
            //if (adjacentCell == null) return; //bug detection
            if (neighbours.IndexOf(adjacentCell) == -1)
            {
                neighbours.Add(adjacentCell);

                /*Once neighbour is added, add self as the neighbour's neighbour*/
                adjacentCell.addNeighbour(this);
            }
        }

        /// <summary>
        /// Number of alive neighbours
        /// </summary>
        int AliveNeighbours
        {
            get
            {
                int n=0;
                foreach (Cell cell in neighbours)
                {
                    if (cell.isAlive) n++;
                }
                return n;
            }
        }

        /// <summary>
        /// Progresses simulation
        /// </summary>
        public void Simulate()
        {
            /*
            Vector3 colors = Vector3.Zero;
            int n = 0;
            Color col;
            
            if (isAlive)
            {
                colors += color.ToVector3();
                n++;
            }*/

            switch (AliveNeighbours)
            {
                case 3:
                    if (isAlive == false)
                    {
                        Vector3 colors = Vector3.Zero;
                        int n = 0;
                        Color col;

                        foreach (Cell cell in neighbours) if (cell.isAlive)
                            {
                                colors += cell.color.ToVector3();
                                n++;
                            }
                        col = new Color(colors / (n));
                        setState(true, col);
                    }
                    else setState(true);
                    break;
                case 2:
                    setState(isAlive);
                    break;
                default:
                    setState(false, Color.Black);
                    break;
            }

        }

        public void update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (isAlive) spriteBatch.Draw(texture, new Rectangle((int)(position.X * (size)), (int)(position.Y * (size)), (int)(size), (int)(size)), new Rectangle(0, 0, texture.Width, texture.Height), color);
        }
    }
}
