using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace Life
{
    class ControlPanel
    {
        public Simulation simulation;
        Input input;
        public Rectangle controlPanelArea;
        public static int height = 32;
        const float scale = 1.0f;
        Texture2D texture;
        
        List<Control> controls;
        public Point mousePosition;
        Label 
            performanceLabel,
            generationLabel,
            gpsLabel;
        Button startPauseButton;

        /// <summary>
        /// used for drawing CP controls
        /// </summary>
        public Matrix TransformationMatrix
        {
            get { return Matrix.CreateScale(scale) * Matrix.CreateTranslation(new Vector3(controlPanelArea.X, controlPanelArea.Y, 0f)); }
        }

        public ControlPanel(Simulation simulation, Input input)
        {
            this.simulation = simulation;
            this.input = input;

            this.controls = new List<Control>();

            Rectangle screenArea = new Rectangle(0, 0, (int)Game1.windowSize.X, (int)Game1.windowSize.Y);
            controlPanelArea = new Rectangle(0, screenArea.Height - height, screenArea.Width, height);
        }

        public void Update(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 pos = Vector2.Transform(simulation.input.mousePosition, Matrix.Invert(TransformationMatrix));
            mousePosition = new Point((int)pos.X, (int)pos.Y);

            gpsLabel.setValue(simulation.GenerationsPerSecond);
            gpsLabel.Color = simulation.RunningSlowly ? Color.Red : Color.Green;

            generationLabel.setValue(simulation.CurrentGeneration);
            performanceLabel.setValue(simulation.Performance);

            startPauseButton.Color = simulation.Paused ? Button.colorDefault : Button.colorOn;

            foreach (Control control in controls) 
                control.Update(gameTime);
        }

        public void LoadContent(ContentManager contentManager)
        {
            texture = contentManager.Load<Texture2D>(@"Textures\Square");

            Texture2D buttonTexture = contentManager.Load<Texture2D>(@"Textures\Buttons\_Button");
            SpriteFont labelFont = contentManager.Load<SpriteFont>(@"Fonts\ScreenFont");

            Texture2D btnPlaySymbol = contentManager.Load<Texture2D>(@"Textures\Buttons\Play");

            int btnY = 5,
                btnSpc = 25,
                lblY = 3;

            startPauseButton = new Button(this, buttonTexture, new Vector2(5, btnY), Button.ButtonValue.StartPause, btnPlaySymbol);
            controls.Add(startPauseButton);

            generationLabel = new Label(new Vector2(30, lblY), labelFont, "Gen", simulation.CurrentGeneration);
            performanceLabel = new Label(new Vector2(300, lblY), labelFont, "Performance", simulation.Performance);
            gpsLabel = new Label(new Vector2(140, lblY), labelFont, "GPS", simulation.GenerationsPerSecond);
            controls.Add(generationLabel);
            controls.Add(gpsLabel);
            //controls.Add(performanceLabel);

            Texture2D btnPlus = contentManager.Load<Texture2D>(@"Textures\Buttons\Plus");
            controls.Add(new Button(this, buttonTexture, new Vector2(235, btnY), Button.ButtonValue.Faster, btnPlus));
            Texture2D btnMinus = contentManager.Load<Texture2D>(@"Textures\Buttons\Minus");
            controls.Add(new Button(this, buttonTexture, new Vector2(260, btnY), Button.ButtonValue.Slower, btnMinus));

            Texture2D btnClear = contentManager.Load<Texture2D>(@"Textures\Buttons\Clear");
            controls.Add(new Button(this, buttonTexture, new Vector2(290, btnY), Button.ButtonValue.Clear, btnClear));
            Texture2D btnRandom = contentManager.Load<Texture2D>(@"Textures\Buttons\Shuffle");
            controls.Add(new Button(this, buttonTexture, new Vector2(315, btnY), Button.ButtonValue.Random, btnRandom));

            Texture2D btnLoad = contentManager.Load<Texture2D>(@"Textures\Buttons\FileOpen");
            controls.Add(new Button(this, buttonTexture, new Vector2(345, btnY), Button.ButtonValue.LoadFile, btnLoad));
            Texture2D btnSave = contentManager.Load<Texture2D>(@"Textures\Buttons\FileSave");
            controls.Add(new Button(this, buttonTexture, new Vector2(345+btnSpc, btnY), Button.ButtonValue.SaveFile, btnSave));

            /*Shape picker*/
            ShapePicker shapePicker = new ShapePicker(buttonTexture, new Rectangle(500, 1, 28, 28), input);
            controls.Add(shapePicker);
            Texture2D btnLeftArrow = contentManager.Load<Texture2D>(@"Textures\Buttons\ArrowLeft");
            Texture2D btnRightArrow = contentManager.Load<Texture2D>(@"Textures\Buttons\ArrowRight");
            controls.Add(new Button(this, buttonTexture, new Vector2(shapePicker.Area.Left - btnSpc, btnY), Button.ButtonValue.PrevShape, btnLeftArrow));
            controls.Add(new Button(this, buttonTexture, new Vector2(shapePicker.Area.Right + 5, btnY), Button.ButtonValue.NextShape, btnRightArrow));

            /*Color picker*/
            for (int i = 0; i < simulation.baseColors.Length; i++)
            {
                Button btn = new Button(this, buttonTexture, new Vector2(700+(btnSpc*i), btnY),Button.ButtonValue.ColorSelect , i);
                btn.Color = simulation.baseColors[i];
                controls.Add(btn);
            }
        }

        public void UnloadContent()
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, controlPanelArea, Color.Brown);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, TransformationMatrix);
            foreach (Control control in controls)
            {
                control.Draw(spriteBatch);
            }
            spriteBatch.End();

        }

        public void onButtonPressed(Button.ButtonValue buttonValue, Button button)
        {
            switch (buttonValue)
            {
                case Button.ButtonValue.StartPause:
                    simulation.Paused = !simulation.Paused;
                    break;
                case Button.ButtonValue.Faster:
                    {
                        simulation.GenerationsPerSecond += 1f;
                    }
                    break;
                case Button.ButtonValue.Slower:
                    {
                        simulation.GenerationsPerSecond -= 1f;
                    }
                    break;
                case Button.ButtonValue.ColorSelect:
                    input.SelectedColor = button.Color;
                    break;

                case Button.ButtonValue.Clear:
                    simulation.clearBoard();
                    break;

                case Button.ButtonValue.LoadFile:
                    Thread loadFileThread = new Thread(new ThreadStart(loadFileDialog));
                    loadFileThread.SetApartmentState(ApartmentState.STA);
                    loadFileThread.Start();
                    break;

                case Button.ButtonValue.SaveFile:
                    Thread loadSaveThread = new Thread(new ThreadStart(saveFileDialog));
                    loadSaveThread.SetApartmentState(ApartmentState.STA);
                    loadSaveThread.Start();
                    break;

                case Button.ButtonValue.Random:
                    simulation.spawnRandomCells(10);
                    break;

                case Button.ButtonValue.PrevShape:
                    input.SelectedCreatureID--;
                    break;
                    
                case Button.ButtonValue.NextShape:
                    input.SelectedCreatureID++;
                    break;
            }
        }

        void loadFileDialog()
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Bitmap images|*.bmp";
            simulation.Paused = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = dialog.FileName;
                simulation.loadFromBitmap(filename);
            }
            simulation.Paused = false;
        }

        void saveFileDialog()
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "Bitmap images|*.bmp";
            dialog.AddExtension = true;
            simulation.Paused = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filename = dialog.FileName;
                simulation.saveToBitmap(filename);
            }
            simulation.Paused = false;
        }
    }
}
