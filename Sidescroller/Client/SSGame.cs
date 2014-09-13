using System;
using System.Linq;
using Engine.Client.Graphics;
using Engine.Client.Graphics.Text;
using Engine.Server.GameCode;
using Engine.Shared;
using Engine.Shared.Tmx;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Sidescroller.Client
{
    public class SSGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private Sprite testSprite;

        private KeyboardState oldState;
        private KeyboardState state;

        private MouseState oldMState;
        private MouseState mState;

        private World world;
        private WorldRenderer worldRenderer;

        private BMFont fontTest;

        public SSGame()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.ApplyChanges();

            Utility.Initialize(graphics, Content, "content");

            testSprite = Sprite.LoadSprite("characters/elisa-spritesheet.png");

            var tmxMap = TmxMap.Load("content/maps/twilight_test/twilight_test.json");
            world = new World(this, tmxMap);
            worldRenderer = new WorldRenderer(this, world);

            Components.Add(world);
            Components.Add(worldRenderer);

            Components.Add(new SpritebatchRenderer(this, Utility.SpriteBatch) {DrawOrder = Int32.MaxValue});

            fontTest = new BMFont(GraphicsDevice, "content/fonts/open_sans", false);

            Utility.Camera.Position = world.Map.Layers.First(layer => layer.Type == TmxLayerType.Object).Objects["PlayerSpawn"][0].Position * world.Scale;
            Console.WriteLine(Utility.Camera.Position);

            oldState = state = Keyboard.GetState();
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            Time.SetValues((float)gameTime.ElapsedGameTime.TotalSeconds, (float)gameTime.TotalGameTime.TotalSeconds); // Update Time class.
            testSprite.Update();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                testSprite.PlayAnimationOnce("strike2");

            oldState = state;
            oldMState = mState;
            state = Keyboard.GetState();
            mState = Mouse.GetState();

            if (state.IsKeyDown(Keys.R) && !oldState.IsKeyDown(Keys.R))
            {
                Console.WriteLine("Reloading sprite");
                testSprite.Reload();
            }

            //Utility.Camera.Move(new Vector2(10, 0) * Time.DT);

            if (mState.RightButton == ButtonState.Pressed)
            {
                Utility.Camera.Rotation += (mState.X - oldMState.X) / 4f;
            }
            if (mState.LeftButton == ButtonState.Pressed)
            {
                Utility.Camera.Move(-new Vector2(mState.X - oldMState.X, mState.Y - oldMState.Y), true);
            }
            if (mState.MiddleButton == ButtonState.Pressed && oldMState.MiddleButton == ButtonState.Released)
            {
                Utility.Camera.Rotation = 0;
            }

            if (state.IsKeyDown(Keys.Space))
            {
                Utility.Camera.SetShake(2, 0.5f, 0.75f);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gt)
        {
            GraphicsDevice.Clear(worldRenderer.ClearColor);

            var spriteBatch = Utility.SpriteBatch;
            Utility.Camera.UpdateTransformation();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, Utility.Camera.TransformationMatrix);

            testSprite.Draw(spriteBatch, new Vector2(0, 0), SpriteEffects.None);

            base.Draw(gt); // spritebatch is ended from a component.
        }

        private class SpritebatchRenderer : DrawableGameComponent
        {
            private readonly SpriteBatch spriteBatch;

            public SpritebatchRenderer(Game game, SpriteBatch spriteBatch) : base(game)
            {
                this.spriteBatch = spriteBatch;
            }

            public override void Draw(GameTime gameTime)
            {
                spriteBatch.End();
            }
        }
    }
}