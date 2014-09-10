using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Engine.Client.Graphics
{
    public class Sprite
    {
        public Texture2D Texture2D;
        public SpriteDefinition SpriteDefinition;
        public string CurrentAnimationName;

        private SpriteAnimation animation;

        private readonly string spritePath;

        public Sprite(Texture2D texture, SpriteDefinition spriteDefinition, string spritePath)
        {
            Texture2D = texture;
            SpriteDefinition = spriteDefinition;
            CurrentAnimationName = "idle";
            this.spritePath = spritePath;

            animation = new SpriteAnimation(SpriteDefinition.AnimationDefinitions[CurrentAnimationName], SpriteDefinition);
        }

        /// <param name="forceRestart">If true, the animation will be restarted if it's already the active one.</param>
        public void SetAnimation(string name, bool forceRestart = false)
        {
            if (!SpriteDefinition.AnimationDefinitions.ContainsKey(name))
            {
                Console.Error.WriteLine("Error: SpriteDefinition does not contain an animation called \"" + name + "\"!");
                return;
            }

            var animDefinition = SpriteDefinition.AnimationDefinitions[name];
            animation.SetDefinition(animDefinition);
        }

        /// <summary>Plays the animation once then reverts to the previous animation.</summary>
        /// <param name="resetFrame">If true, the animation will be played from the beginning.</param>
        public void PlayAnimationOnce(string name)
        {
            if (!SpriteDefinition.AnimationDefinitions.ContainsKey(name))
            {
                Console.Error.WriteLine("Error: SpriteDefinition does not contain an animation called \"" + name + "\"!");
                return;
            }

            animation.PlayOnce(SpriteDefinition.AnimationDefinitions[name]);
        }

        public void Update()
        {
            animation.Update();
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects, Color color)
        {
            spriteBatch.Draw(Texture2D, position, animation.GetRectangle(), color, 0f, animation.Definition.Origin, SpriteDefinition.Scale, spriteEffects, 0);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects)
        {
            Draw(spriteBatch, position, spriteEffects, Color.White);
        }

        /// <summary>Reloads the texture and sprite definition from disk.</summary>
        public void Reload()
        {
            var currentAnimationFrame = animation.GetCurrentFrame();
            var currentAnimationName = animation.Definition.Name;
            var currentPlayOnce = animation.GetPlayOnce();

            var reloadedSprite = Sprite.LoadSprite(spritePath);
            Texture2D = reloadedSprite.Texture2D;
            SpriteDefinition = reloadedSprite.SpriteDefinition;
            var oldAnim = animation;
            animation = reloadedSprite.animation;
            animation.SetDefinition(SpriteDefinition.AnimationDefinitions[currentAnimationName], false);
            animation.SetFrame(currentAnimationFrame);
            animation.SetPlayOnce(currentPlayOnce);
            animation.currentWaitTime = oldAnim.currentWaitTime;
            animation.waitTimeTarget = oldAnim.waitTimeTarget;
            animation.animationDirection = oldAnim.animationDirection;
            animation.Paused = oldAnim.Paused;
            animation.lastAnimationName = oldAnim.lastAnimationName;
        }

        #region Static methods

        public static Sprite LoadSprite(string spritePath)
        {
            if (Path.GetExtension(spritePath) == "")
                spritePath += ".png";

            var texture = Utility.LoadTexture2D("sprites/" + spritePath);

            if (texture == null)
                throw new FileNotFoundException("Could not find sprite at: " + spritePath);

            var pathToDefinition = spritePath.Substring(0, spritePath.LastIndexOf(".")) + ".json";
            var spriteDefinition = Sprite.LoadSpriteDefinition(pathToDefinition);

            if (spriteDefinition == null)
            {
                texture.Dispose();
                throw new FileNotFoundException("Could not find sprite definition at: " + pathToDefinition);
            }

            return new Sprite(texture, spriteDefinition, spritePath);
        }

        public static SpriteDefinition LoadSpriteDefinition(string spritePath)
        {
            spritePath = Utility.GetContentDir("sprites/" + spritePath);

            if (!File.Exists(spritePath))
                return null;

            var definition = new SpriteDefinition();
            string jsonDefinition = File.ReadAllText(spritePath);
            var jDefinition = JsonConvert.DeserializeObject<JToken>(Utility.JsonRemoveComments(jsonDefinition));

            definition.DefaultAnimationFPS = jDefinition["defaultFps"].Value<float>();
            definition.Scale = jDefinition["scale"].Value<float>();
            definition.AnimationDefinitions = new Dictionary<string, SpriteAnimationDefinition>();

            // Add animations
            var jAnimations = jDefinition["animations"].Value<JArray>();
            foreach (JObject jAnimation in jAnimations)
            {
                var animDefinition = new SpriteAnimationDefinition();

                animDefinition.Name = jAnimation["name"].Value<string>();
                animDefinition.FPS = jAnimation["fps"] != null ? jAnimation["fps"].Value<float>() : definition.DefaultAnimationFPS;

                var jType = jAnimation["type"];
                if (jType != null)
                    animDefinition.AnimationType = (SpriteAnimationType)Enum.Parse(typeof (SpriteAnimationType), jType.Value<string>(), true);

                var jOffsetArray = jAnimation["origin"] as JArray;
                if (jOffsetArray != null)
                {
                    if (jOffsetArray.Count != 2)
                        throw new InvalidOperationException("Expected 2 value array in animations' origin. (" + animDefinition.Name + ")");

                    animDefinition.Origin = new Vector2(jOffsetArray[0].Value<float>(), jOffsetArray[1].Value<float>());
                }

                var jFrames = jAnimation["frames"].Value<JArray>();
                animDefinition.Frames = new SpriteAnimFrame[jFrames.Count];

                for (int i = 0; i < jFrames.Count; i++)
                {
                    var jFrame = jFrames[i].Value<JArray>();
                    var animFrame = new SpriteAnimFrame();

                    int x = jFrame[0].Value<int>();
                    int y = jFrame[1].Value<int>();
                    int w = jFrame[2].Value<int>();
                    int h = jFrame[3].Value<int>();
                    animFrame.SubRectangle = new Rectangle(x, y, w, h);

                    if (jFrame.Count > 4)
                        animFrame.WaitTime = jFrame[4].Value<float>();

                    animDefinition.Frames[i] = animFrame;
                }

                if (animDefinition.Frames.Length == 0)
                    Console.Error.WriteLine("Warning: Sprite definition animation contains no frames! (" + spritePath + ", " + animDefinition.Name + ")");

                definition.AnimationDefinitions.Add(animDefinition.Name, animDefinition);
            }

            if (!definition.AnimationDefinitions.ContainsKey("idle"))
                throw new InvalidOperationException("Error: Sprite definition animation does not contain an idle frame!");

            return definition;
        }

        #endregion
    }

    public class SpriteAnimation
    {
        public SpriteDefinition SpriteDefinition;
        public SpriteAnimationDefinition Definition { get; private set; }
        public bool Paused;

        internal string lastAnimationName;

        #region Animation specific variables

        internal float waitTimeTarget; // Target time until switch to next frame.
        internal float currentWaitTime;

        private int currentFrame; // The index of the current frame.
        internal sbyte animationDirection = 1; // Either -1 or 1. Decides which direction the animation is going in.
        private bool playOnce;

        #endregion

        public SpriteAnimation(SpriteAnimationDefinition definition, SpriteDefinition spriteDefinition)
        {
            Definition = definition;
            SpriteDefinition = spriteDefinition;
            Paused = definition.Frames.Length <= 1; // Pause the animation if there's only one frame.
            currentFrame = 0;
            resetWaitTime();
        }

        /// <param name="storePrevious">If true, you can restore to the previous definition by calling RevertDefinition.</param>
        public void SetDefinition(SpriteAnimationDefinition definition, bool storePrevious = true, bool resetFrame = false, bool force = false)
        {
            if (Definition == definition && !force)
                return;

            if (storePrevious)
                lastAnimationName = Definition.Name;

            if (resetFrame)
            {
                currentFrame = 0;
            }
            else
            {
                float currentIndexRatio = (float)currentFrame / (float)Definition.Frames.Length;
                currentFrame = (int)(definition.Frames.Length * currentIndexRatio); // Set the next animation's frame index relative to the current animation and index.
            }

            Definition = definition;
            playOnce = false;
            animationDirection = 1;
            resetWaitTime();
        }

        /// <summary>Called when sprite using this animation is reloaded, used to transparently switch out to the new definition.</summary>
        public void ReloadDefinition(SpriteAnimationDefinition animDefinition)
        {
            Definition = animDefinition;
            if (currentFrame >= Definition.Frames.Length)
                currentFrame = 0;
        }

        /// <param name="switchDefinitions">If true, the current animation will be set as the previous animation.</param>
        public void RevertDefinition(bool switchDefinitions = true)
        {
            var current = Definition;
            var lastDefinition = getLastDefinition();

            if (lastDefinition == null)
                lastAnimationName = "idle";

            lastDefinition = getLastDefinition();

            SetDefinition(lastDefinition, switchDefinitions, true);

            if (switchDefinitions)
                lastAnimationName = current.Name;
        }

        public void Pause()
        {
            Paused = true;
        }

        public void Play()
        {
            Paused = false;
        }

        public void PlayOnce(SpriteAnimationDefinition animDefinition)
        {
            if (Definition == animDefinition)
                return;

            SetDefinition(animDefinition, true, true);
            playOnce = true;
        }

        public void Update()
        {
            if (Paused)
                return;

            currentWaitTime += Time.DT;

            if (currentWaitTime >= waitTimeTarget)
            {
                nextFrame();
                resetWaitTime();
            }
        }

        public Rectangle GetRectangle()
        {
            return Definition.Frames[currentFrame].SubRectangle;
        }

        public int GetCurrentFrame()
        {
            return currentFrame;
        }

        public void SetFrame(int frame)
        {
            if (frame >= Definition.Frames.Length || frame < 0)
            {
                currentFrame = 0;
                return;
            }

            currentFrame = 0;
        }

        public bool GetPlayOnce()
        {
            return playOnce;
        }

        public void SetPlayOnce(bool playOnce)
        {
            this.playOnce = playOnce;
        }

        private void nextFrame()
        {
            if (playOnce && isLastFrame())
            {
                RevertDefinition();
            }
            else
            {
                currentFrame = getNextFrameIndex();
            }
        }

        private int getNextFrameIndex()
        {
            if (Definition.AnimationType == SpriteAnimationType.Default)
            {
                if (currentFrame + 1 >= Definition.Frames.Length)
                    return 0;
                return currentFrame + 1;
            }

            if (Definition.AnimationType == SpriteAnimationType.Swing)
            {
                if (currentFrame + animationDirection >= Definition.Frames.Length || currentFrame + animationDirection < 0)
                    animationDirection *= -1;

                return currentFrame + animationDirection;
            }

            throw new NotImplementedException();
        }

        private float getWaitTime()
        {
            float? waitTime = Definition.Frames[currentFrame].WaitTime;

            if (waitTime != null)
                return waitTime.Value;

            return (1000f / Definition.FPS) / 1000f;
        }

        private void resetWaitTime()
        {
            currentWaitTime = Time.TotalDT;
            waitTimeTarget = currentWaitTime + getWaitTime();
        }

        private bool isLastFrame()
        {
            return (currentFrame + animationDirection >= Definition.Frames.Length && Definition.AnimationType == SpriteAnimationType.Default) || currentFrame + animationDirection < 0;
        }

        private SpriteAnimationDefinition getLastDefinition()
        {
            if (lastAnimationName == null)
                return null;

            if (SpriteDefinition.AnimationDefinitions.ContainsKey(lastAnimationName))
                return SpriteDefinition.AnimationDefinitions[lastAnimationName];

            return null;
        }
    }

    public class SpriteDefinition
    {
        public float Scale;
        public float DefaultAnimationFPS;
        public Dictionary<string, SpriteAnimationDefinition> AnimationDefinitions;

        public SpriteDefinition() : this(0, 0, null) {}
        public SpriteDefinition(float scale, float defaultAnimationFps, Dictionary<string, SpriteAnimationDefinition> animations)
        {
            Scale = scale;
            DefaultAnimationFPS = defaultAnimationFps;
            AnimationDefinitions = animations;
        }
    }

    public class SpriteAnimationDefinition
    {
        public string Name;
        public float FPS;
        public SpriteAnimationType AnimationType;
        public Vector2 Origin;
        public SpriteAnimFrame[] Frames;

        public SpriteAnimationDefinition() : this("", 0, null, Vector2.Zero, SpriteAnimationType.Default) {}
        public SpriteAnimationDefinition(string name, float fps, IEnumerable<SpriteAnimFrame> frames, Vector2 origin, SpriteAnimationType animationType)
        {
            Name = name;
            FPS = fps;
            Origin = origin;
            Frames = frames != null ? frames.ToArray() : null;
        }
    }

    public enum SpriteAnimationType
    {
        /// <summary>Goes forwards and then restarts at the beginning when it reaches the end.</summary>
        Default,

        /// <summary>Goes forwards and backwards through the frames.</summary>
        Swing
    }

    public class SpriteAnimFrame
    {
        public Rectangle SubRectangle;
        public float? WaitTime;

        public SpriteAnimFrame() : this(Rectangle.Empty, null) {}
        public SpriteAnimFrame(Rectangle rectangle, float? waitTime)
        {
            SubRectangle = rectangle;
            WaitTime = waitTime;
        }
    }
}