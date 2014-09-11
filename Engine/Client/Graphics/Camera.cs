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

            var matrix = Matrix.Identity;
            matrix *= Matrix.CreateTranslation(-Position.X, -Position.Y, 0);
            matrix *= Matrix.CreateRotationZ(MathHelper.ToRadians(Rotation));
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
    }
}