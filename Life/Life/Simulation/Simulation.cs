using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.Diagnostics;

namespace Life
{
    class Simulation
    {
        #region variables
        volatile List<Cell> cells;
        public Camera camera;
        public Input input;
        public Random random;
        public ControlPanel controlPanel;
        Thread simulation;
        float generationPeriod; //duration of 1 generation
        Texture2D
            bgTexture;
        SpriteFont font;
        int gridWidth, gridHeight;
        Rectangle gridArea;
        bool isRunningSlowly = false;
        bool paused = false;
        int performance = 0;
        volatile bool simulating = false; //for thread
        
        Color
            bgColor = Color.White;
        int currentGeneration;

        public Color[] baseColors = new Color[] { 
            new Color(255,0,0), 
            new Color(0,255,0),
            new Color(0,0,255),
            /*new Color(255,255,0), 
            new Color(0,255,255),
            new Color(255,0,255),*/ 
        };
        #endregion

        #region Properties

        public bool RunningSlowly
        {
            get { return isRunningSlowly; }
        }

        /// <summary>
        /// Time in ms taken to simulate last generation
        /// </summary>
        public int Performance
        {
            get { return performance; }
        }

        public bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }

        public int GridWidth
        {
            get { return gridWidth; }
        }

        public int GridHeight
        {
            get { return gridHeight; }
        }

        public Rectangle GridArea
        {
            get { return gridArea; }
        }

        public float GenerationsPerSecond
        {
            get { return 1 / generationPeriod; }
            set {
                value = MathHelper.Clamp(value, 0.5f, 500f);
                generationPeriod = 1 / value; 
            }
        }

        public int CurrentGeneration
        {
            get { return currentGeneration; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates new simulation
        /// </summary>
        /// <param name="contentManager">Content manager</param>
        /// <param name="width"> Number of cells in a row</param>
        /// <param name="height">Number of cells in each column</param>
        /// <param name="generationsPerSecond">Generations Per Secondnotepad</param>
        public Simulation(ContentManager contentManager, int width, int height, float generationsPerSecond)
        {
            this.gridWidth = width;
            this.gridHeight = height;

            GenerationsPerSecond = generationsPerSecond;
            
            random = new Random();
            simulation = new Thread(new ThreadStart(simulateCells));

            //randomizeState();
            //loadState(new Point[]{});
            //loadFromBitmap("life.bmp");
            initializeBoard();

            this.gridArea = new Rectangle(0, 0, gridWidth, gridHeight);

            this.camera = new Camera(gridArea);
            this.input = new Input(this);
            this.controlPanel = new ControlPanel(this, input);
            LoadContent(contentManager);
        }
        #endregion

        #region public  methods
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.TransformationMatrix);

            spriteBatch.Draw(bgTexture, new Rectangle(0, 0, gridWidth * Cell.size, gridHeight * Cell.size), bgColor);
            /*Draw cells*/
            foreach (Cell cell in cells)
            {
                cell.Draw(spriteBatch);
            }

            input.draw(spriteBatch);
            spriteBatch.End();

            controlPanel.Draw(spriteBatch);           
        }
        public void Update(GameTime gameTime)
        {
            input.update(gameTime);
            controlPanel.Update(gameTime);

            foreach (Cell cell in cells) cell.update(gameTime);
 
        }

        public void LoadContent(ContentManager contentManager)
        {
            Cell.texture = contentManager.Load<Texture2D>(@"Textures\Square");
            font = contentManager.Load<SpriteFont>(@"Fonts\ScreenFont");
            bgTexture = contentManager.Load<Texture2D>(@"Textures\Square");
            input.loadContent(contentManager);
            controlPanel.LoadContent(contentManager);
        }

        public void spawnRandomCells(int population = 10)
        {
            foreach (Cell cell in cells)
            {
                if ((random.Next() % population) == 0)
                {
                    cell.setState(true, baseColors[random.Next() % baseColors.Length] /*new Color(random.Next() % 256, random.Next() % 256, random.Next() % 256)*/);
                    cell.applyNewState();
                }
            }
        }

        public void clearBoard()
        {
            foreach (Cell cell in cells)
            {
                cell.setState(false, Color.Black);
                cell.applyNewState();
            }
            currentGeneration = 0;
        }

        public void UnloadContent()
        {
            this.stop();
        }

        public void spawnCreature(Point location, bool[,] creature, Color color)
        {
            for (int y = 0; y < creature.GetLength(1); y++)
            {
                for (int x = 0; x < creature.GetLength(0); x++)
                {
                    Cell cell = getCell(new Point(x + location.X, y + location.Y));
                    if (cell == null) continue;

                    cell.Color = color;
                    cell.setState(creature[x, y]);
                    cell.applyNewState();
                }
            }
        }
        #endregion

        #region state savers/loaders
        /*public void loadState(Point[] aliveCells)
        {
            Cell[,] cellsTmp = new Cell[gridWidth, gridHeight];
            cells = new List<Cell>(gridWidth * gridHeight);

            #region spawn Cells
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {

                    Cell cell = new Cell(x, y, false);
                    cellsTmp[x, y] = cell;
                    cells.Add(cell);

                    if (x > 0)
                    {
                        if (y > 0) cell.addNeighbour(cellsTmp[x - 1, y - 1]);
                        cell.addNeighbour(cellsTmp[x - 1, y]);
                    }
                    if (y > 0)
                    {
                        cell.addNeighbour(cellsTmp[x, y - 1]);
                        if (x+1 < gridWidth) cell.addNeighbour(cellsTmp[x + 1, y - 1]);
                    }
                }
            }

            foreach (Point point in aliveCells)
            {
                cellsTmp[point.X, point.Y].IsAlive = true;
            }
            #endregion
            currentGeneration = 0;
        }*/



        /// <summary>
        /// Initializes simulation area
        /// </summary>
        /// <param name="width">Number of cells in a row</param>
        /// <param name="height">Number if cekks ub a column</param>
        public void initializeBoard(int width, int height)
        {
            this.gridWidth = width;
            this.gridHeight = height;
            Cell[,] cellsTmp = new Cell[gridWidth, gridHeight];
            cells = new List<Cell>(gridWidth * gridHeight);
            this.gridArea = new Rectangle(0, 0, gridWidth, gridHeight);

            #region spawn Cells
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {

                    Cell cell = new Cell(x, y, false);
                    cellsTmp[x, y] = cell;
                    cells.Add(cell);

                    if (x > 0)
                    {
                        if (y > 0) cell.addNeighbour(cellsTmp[x - 1, y - 1]);
                        cell.addNeighbour(cellsTmp[x - 1, y]);
                    }
                    if (y > 0)
                    {
                        cell.addNeighbour(cellsTmp[x, y - 1]);
                        if (x + 1 < gridWidth) cell.addNeighbour(cellsTmp[x + 1, y - 1]);
                    }
                }
            }
            #endregion
            currentGeneration = 0;
        }

        void initializeBoard()
        {
            initializeBoard(gridWidth, gridHeight);
        }

        public void saveToBitmap(string filename)
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(gridWidth, GridHeight);
            foreach (Cell cell in cells)
            {
                System.Drawing.Color color = new System.Drawing.Color();
                if (cell.IsAlive == true)
                {
                    color = System.Drawing.Color.FromArgb(cell.Color.R, cell.Color.G, cell.Color.B);
                }
                else
                {
                    color = System.Drawing.Color.White;
                }
                bitmap.SetPixel(cell.Position.X, cell.Position.Y, color);
            }
            bitmap.Save(filename);
            bitmap.Dispose();
        }

        public void loadFromBitmap(string filename)
        {
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(filename);
            gridWidth = bitmap.Width;
            gridHeight = bitmap.Height;

            /*Pause thread and wait for it to finish*/
            paused = true;
            while (simulating == true);

            Cell[,] cellsTmp = new Cell[gridWidth, gridHeight];
            List<Cell> cellList = new List<Cell>(gridWidth * gridHeight); ;
            //cells = new List<Cell>(gridWidth * gridHeight);

            #region spawn Cells
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    System.Drawing.Color color = bitmap.GetPixel(x, y);

                    Cell cell = new Cell(x, y, (color.R < 200 || color.G < 200 || color.B < 200) , new Color(color.R, color.G, color.B));
                    
                    cellsTmp[x, y] = cell;
                    cellList.Add(cell);

                    if (x > 0)
                    {
                        if (y > 0) cell.addNeighbour(cellsTmp[x - 1, y - 1]);
                        cell.addNeighbour(cellsTmp[x - 1, y]);
                    }
                    if (y > 0)
                    {
                        cell.addNeighbour(cellsTmp[x, y - 1]);
                        if (x + 1 < gridWidth) cell.addNeighbour(cellsTmp[x + 1, y - 1]);
                    }

                    if (cell.IsAlive == true)
                    {
                        continue;
                    }
                }
            }
            #endregion
            bitmap.Dispose();
            cells = cellList; //updates main list of cells
            this.gridArea = new Rectangle(0, 0, gridWidth, gridHeight); //updates size
            camera.SimulationArea = gridArea; //updates camera area
            currentGeneration = 0;
            paused = false;            
        }

        public void randomizeState()
        {
            Cell[,] cellsTmp = new Cell[gridWidth, gridHeight];
            cells = new List<Cell>(gridWidth * gridHeight);

            #region spawn Cells
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {

                    Cell cell = new Cell(x, y, (random.Next() % 15) == 0 ? true : false, baseColors[random.Next() % baseColors.Length]);
                    cellsTmp[x, y] = cell;
                    cells.Add(cell);

                    if (x > 0)
                    {
                        if (y > 0) cell.addNeighbour(cellsTmp[x - 1, y - 1]);
                        cell.addNeighbour(cellsTmp[x - 1, y]);
                    }

                    if (y > 0)
                    {
                        cell.addNeighbour(cellsTmp[x, y - 1]);
                        if (x < gridWidth - 1) cell.addNeighbour(cellsTmp[x + 1, y - 1]);
                    }
                }
            }
            #endregion
            currentGeneration = 0;
        }
        #endregion

        #region private methods
        bool isCellAlive(int x, int y)
        {
            return getCell(x, y).IsAlive;
        }

        Cell getCell(Point location)
        {
            return getCell(location.X, location.Y);
        }

        Cell getCell(int x, int y)
        {
            if (x >= gridWidth || y >= gridHeight || x < 0 || y < 0) return null;
            return cells[(y * gridWidth) + x];
        }
        #endregion

        #region Simulation control
        /// <summary>
        /// Starts simulation
        /// </summary>
        public void start()
        {
            simulation.Start();
        }

        /// <summary>
        /// Stops simulation
        /// </summary>
        public void stop()
        {
            simulation.Abort();
        }

        /// <summary>
        /// Runs cell simulation in a loop
        /// </summary>
        void simulateCells()
        {
            Thread.Sleep((int)(generationPeriod * 1000f));
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (true)
            {
                if (paused == false)
                {
                    try
                    {
                        simulating = true;
                        foreach (Cell cell in cells) cell.Simulate();
                        int n=0;
                        foreach (Cell cell in cells)
                        {
                            if (cell.applyNewState()) n++;
                        }
                        if (n > 0) currentGeneration++;
                    }
                    catch
                    {
                    }
                    simulating = false;

                    int timeLeft = (int)(1000 * generationPeriod) - (int)timer.ElapsedMilliseconds;
                    performance = (int)timer.ElapsedMilliseconds;
                    if (timeLeft > 0)
                    {
                        isRunningSlowly = false;
                        Thread.Sleep(timeLeft);
                    }
                    else
                    {
                        isRunningSlowly = true;
                        //generationPeriod = (float)timer.ElapsedMilliseconds / 1000f; //slows down simulation
                    }                    
                }
                timer.Restart();

            }
        }
        #endregion
    }
}
