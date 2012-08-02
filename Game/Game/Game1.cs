using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Xml;

namespace Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        private ThingHandler Things;
        private Sentient Dude;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


            //TEST objects for Thing class
            Things = new ThingHandler(spriteBatch); //create a ThingHandler - a glorified Thing List
            Dude = new Sentient("Arthur", Content);
            Things.Add(Dude);
            Dude.Move(new Vector2(100, 0));

            Things[Things.Add(new Thing("testplat",Content))].Move(new Vector2(100, 300));

            Things[Things.Add(new Thing("testplat", Content))].Move(new Vector2(200, 250));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // XNA crap
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            base.Update(gameTime);


            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {

                //Example code with pull-push method of working with objects
                Dude.Jump("Jump.Up", "Jump.Down", "Jump.Land");

            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                Dude.velocity = new Vector2(40, Dude.velocity.Y);
                Dude.StartAnimation("walk");
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                Dude.velocity = new Vector2(-40, Dude.velocity.Y);
                Dude.StartAnimation("walk");
            }
            else
            {
                Sentient d = (Sentient)Dude;
                    Dude.velocity = new Vector2(0, Dude.velocity.Y);
                    Dude.StartAnimation("stand");
            }


            Things.Update();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Things.Draw();
            base.Draw(gameTime);
        }
    }
}
