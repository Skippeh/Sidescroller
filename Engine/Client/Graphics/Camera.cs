using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Shared;
using Microsoft.Xna.Framework;

namespace Engine.Client.Graphics
{
    public class Camera
    {
        public Matrix TransformationMatrix { get; private set; }

        public Vector2 Position;
        public float Zoom;

        private Matrix negRotation;
        private float rotationAngle;
        public float Rotation
        {
            get { return rotationAngle; }
            set
            {
                rotationAngle = value;
                negRotation = Matrix.CreateRotationZ(-MathHelper.ToRadians(value));
            }
        }

        private float shakeIntensityP;
        private float shakeIntensityR;
        private float shakeStartTime;
        private float shakeFalloff;
        private bool shakeInfinitely;

        public Camera()
        {
            Zoom = 1f;
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(TransformationMatrix));
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, TransformationMatrix);
        }

        public void UpdateTransformation()
        {
            var winSize = Utility.GetWindowSize();

            var positionShake = new Vector2((float)Math.Sin(Utility.Random.NextDouble() * MathHelper.TwoPi),
                                            (float)Math.Cos(Utility.Random.NextDouble() * MathHelper.TwoPi)) * shakeIntensityP;
            var rotationShake = (float)(shakeIntensityR * ((Utility.Random.NextDouble() * 2d) - 1d));

            if (!shakeInfinitely)
            {
                float amount01 = 1f - (Time.TotalDT - shakeStartTime) / shakeFalloff;
                if (amount01 < 0)
                    amount01 = 0;

                positionShake *= Vector2.Lerp(Vector2.Zero, positionShake, Utility.Random.NextDouble() > 0.5 ? amount01 : -amount01);
                rotationShake = MathHelper.Lerp(0, rotationShake, amount01);
                Console.WriteLine(positionShake);
            }

            var matrix = Matrix.Identity;
            matrix *= Matrix.CreateTranslation(-(Position.X + positionShake.X), -(Position.Y + positionShake.Y), 0);
            matrix *= Matrix.CreateRotationZ(MathHelper.ToRadians(Rotation + rotationShake));
            matrix *= Matrix.CreateTranslation(new Vector3(winSize.X / 2 / Zoom, winSize.Y / 2 / Zoom, 0));
            matrix *= Matrix.CreateScale(Zoom);

            TransformationMatrix = matrix;
        }

        /// <param name="relative">If true, position added will be relative to current camera rotation.</param>
        public void Move(Vector2 amount, bool relative = false)
        {
            if (relative)
            {
                Position += Vector2.Transform(amount, negRotation);
            }
            else
            {
                Position += amount;
            }
        }

        /// <param name="relative">If true, position added will be relative to current camera rotation.</param>
        public void MoveX(float amount, bool relative = false)
        {
            Move(new Vector2(Position.X + amount, 0), relative);
        }

        /// <param name="relative">If true, position added will be relative to current camera rotation.</param>
        public void MoveY(float amount, bool relative = false)
        {
            Move(new Vector2(0, Position.Y + amount), relative);
        }

        /// <summary>Shakes the camera position by pixels and rotation by degrees.</summary>
        /// <param name="positionAmount">The offset in pixels.</param>
        /// <param name="rotationAmount">The offset in degrees.</param>
        /// <param name="falloff">Time in seconds to dissipate. Set to 0 to shake until manually turning off.</param>
        public void SetShake(float positionAmount, float rotationAmount, float falloff = 0)
        {
            shakeIntensityP = positionAmount;
            shakeIntensityR = rotationAmount;
            shakeStartTime = Time.TotalDT;
            shakeFalloff = falloff;
            shakeInfinitely = falloff == 0f;
        }
    }
}