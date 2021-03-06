﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using engine;

namespace FallingBlockGame
{
    public class GameplayState : IGameState
    {
        private const int BLOCK_SIZE = 32;
        private const int MOVE_SPEED = 100;
        private const string BLOCK_TEXTURE_ATLAS = "blocks";

        private const string BACKGROUND = "menubackground";

        private const int SCORE_X = 400;
        private const int SCORE_Y = 70;
        private const int SCORE_WIDTH = 140;
        private const int SCORE_HEIGTH = 30;

        private const int SPEED_X = SCORE_X;
        private const int SPEED_Y = SCORE_Y + 50;

        private const int NEXTSHAPE_X = SCORE_X;
        private const int NEXTSHAPE_Y = SPEED_Y + 50;

        private FallingBlockGame game;
        private FieldGameObjectBuilder fieldGameObjectBuilder;
        private GameLogic gameLogic;

        //private List<List<GameObject>> nextShapes;
        private List<Texture2D> nextShapes;

        public bool IsActive { get; set; }
        public Dictionary<string, IGameState> ChildStates { get; set; }

        private Label score;
        private Panel scorePanel;
        private Label speed;
        private Panel speedPanel;
        private Panel nextShapePanel;
        private Image nextShape;
        private Texture2D Background;
        private Border fieldBorder;

        private SoundEffect blockFall;
        private SoundEffect lineClear;

        private double timer;
        private double keyDownTimer;

        public GameplayState(FallingBlockGame game)
        {
            this.game = game;
            gameLogic = new GameLogic();

            IsActive = false;

            Texture2D texture = game.Content.Load<Texture2D>(BLOCK_TEXTURE_ATLAS);
            TextureAtlas blockTextureAtlas = new TextureAtlas(texture, 2, 3);

            blockFall = game.Content.Load<SoundEffect>("sfx_fall");
            lineClear = game.Content.Load<SoundEffect>("sfx_lineClear");
            
            fieldGameObjectBuilder = new FieldGameObjectBuilder(
                gameLogic.Field,
                BLOCK_SIZE,
                blockTextureAtlas);

            ChildStates = new Dictionary<string, IGameState>();
            ChildStates.Add("gameover", new GameOverState(game));
            ChildStates.Add("pause", new PauseState(game));

            timer = gameLogic.UpdateRate;
            keyDownTimer = MOVE_SPEED;

            Background = game.Content.Load<Texture2D>(BACKGROUND);

            score = new Label("Score: 0");
            score.Font = game.Content.Load<SpriteFont>("test");
            score.TextColor = Color.Black;
            score.Position = new Vector2(SCORE_X + 5, SCORE_Y + 5);

            scorePanel = new Panel(game.GraphicsDevice);
            scorePanel.Position = new Vector2(SCORE_X, SCORE_Y);
            scorePanel.Heigth = SCORE_HEIGTH;
            scorePanel.Width = SCORE_WIDTH;

            speed = new Label("Speed: 1");
            speed.Font = game.Content.Load<SpriteFont>("test");
            speed.TextColor = Color.Black;
            speed.Position = new Vector2(SPEED_X + 5, SPEED_Y + 5);

            speedPanel = new Panel(game.GraphicsDevice);
            speedPanel.Position = new Vector2(SPEED_X, SPEED_Y);
            speedPanel.Heigth = SCORE_HEIGTH;
            speedPanel.Width = SCORE_WIDTH;

            nextShapePanel = new Panel(game.GraphicsDevice);
            nextShapePanel.Position = new Vector2(NEXTSHAPE_X, NEXTSHAPE_Y);
            nextShapePanel.Heigth = 150;
            nextShapePanel.Width = SCORE_WIDTH;

            fieldBorder = new Border(game.GraphicsDevice);
            fieldBorder.Position = new Vector2(gameLogic.Field.X, gameLogic.Field.Y);
            fieldBorder.Width = BLOCK_SIZE * gameLogic.Field.Grid[0].Length;
            fieldBorder.Heigth = BLOCK_SIZE * (gameLogic.Field.Grid.Length - 2);
            fieldBorder.BorderColor = Color.White;

            string shape = "shape";
            nextShapes = new List<Texture2D>();
            for (int i = 1; i <= gameLogic.blockTypes.Length; i++)
            {
                nextShapes.Add(game.Content.Load<Texture2D>(shape + i));
            }
            
            nextShape = new Image(nextShapes[1]);
            nextShape.ImageTexture = nextShapes[gameLogic.NextShapeType];
            nextShape.Position = new Vector2(NEXTSHAPE_X + 22, NEXTSHAPE_Y + 11);
            nextShape.Width = 96;
            nextShape.Heigth = 128;
        }

        private Movement UpdateKeyHoldDownMovement(GameTime gameTime,  Movement movement)
        {
            keyDownTimer -= gameTime.ElapsedGameTime.Milliseconds;
            if (keyDownTimer < 0)
            {
                    keyDownTimer = MOVE_SPEED;
                    return movement;
            }

            return Movement.None;
        }
        
        public void Update(GameTime gameTime)
        {
            double elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
            timer -= elapsedTime;

            if (!gameLogic.IsGameOver) 
            {

                if (InputManager.KeyPressed(Keys.Space) || ChildStates["pause"].IsActive)
                    ChildStates["pause"].IsActive = true;
                else
                {
                    Movement move = Movement.None;
                    if (InputManager.KeyReleased(Keys.Down))
                    {
                        move = Movement.Down;
                    }
                    else if (InputManager.KeyReleased(Keys.Left))
                        move = Movement.Left;
                    else if (InputManager.KeyReleased(Keys.Right))
                        move = Movement.Right;
                    else if (InputManager.KeyReleased(Keys.Up))
                        move = Movement.Rotate;

                    if (InputManager.KeyboardState.IsKeyDown(Keys.Down))
                        move = UpdateKeyHoldDownMovement(gameTime, Movement.Down);
                    else if (InputManager.KeyboardState.IsKeyDown(Keys.Left))
                        move = UpdateKeyHoldDownMovement(gameTime, Movement.Left);
                    else if (InputManager.KeyboardState.IsKeyDown(Keys.Right))
                        move = UpdateKeyHoldDownMovement(gameTime, Movement.Right);
                    else
                        keyDownTimer = MOVE_SPEED;

                    if (timer < 0)
                    {
                        timer = gameLogic.UpdateRate;
                        gameLogic.Update(Movement.Down);
                    }

                    gameLogic.Update(move);

                    PlaySoundEffects();

                    UpdateScore();
                    UpdateSpeed();
                    nextShape.ImageTexture = nextShapes[gameLogic.NextShapeType];
                }
            }
            else if (!ChildStates["gameover"].IsActive)
            {
                ChildStates["gameover"].IsActive = true;
            }
            else
            {
                if (InputManager.IsAnyKeyPressed() && !InputManager.LastKeyboardState.IsKeyDown(Keys.Down))
                {
                    gameLogic.Restart();
                    ChildStates["gameover"].IsActive = false;
                }
            }
        }

        private void UpdateScore()
        {
            score.Text = string.Format("Score: {0}", gameLogic.Score);
        }

        private void UpdateSpeed()
        {
            speed.Text = string.Format("Speed: {0}", gameLogic.Speed);
        }

        private void PlaySoundEffects()
        {
            if (gameLogic.IsDown)
            {
                blockFall.Play(1.0f, 0.0f, 0.0f);
                gameLogic.IsDown = false;
            }
            if (gameLogic.IsFullRow)
            {
                lineClear.Play(0.5f, 0.0f, 0.0f);
                gameLogic.IsFullRow = false;
            }
        }

        public void Draw(GameTime gameTime)
        {
            game.RenderManager.Graphics.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null);
            game.RenderManager.Graphics.SpriteBatch.Draw(Background, new Vector2(0, 0), new Rectangle(0, 0, 600, 700), Color.White);
            game.RenderManager.Graphics.SpriteBatch.End();

            List<GameObject> gameObjects = fieldGameObjectBuilder.CreateGameObjects();

            game.RenderManager.Draw(gameObjects);
            fieldBorder.Draw(game.RenderManager.Graphics);

            scorePanel.Draw(game.RenderManager.Graphics);
            score.Draw(game.RenderManager.Graphics);

            speedPanel.Draw(game.RenderManager.Graphics);
            speed.Draw(game.RenderManager.Graphics);

            nextShapePanel.Draw(game.RenderManager.Graphics);
            nextShape.ImageTexture = nextShapes[gameLogic.NextShapeType];
            nextShape.Draw(game.RenderManager.Graphics);
        }

    }
}
