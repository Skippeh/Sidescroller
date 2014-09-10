using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine.Shared
{
    public static class Time
    {
        /// <summary>Returns the time it took for the last frame to process in seconds.</summary>
        public static float DT { get; private set; }

        /// <summary>Returns the total seconds elapsed since the game started in seconds.</summary>
        public static float TotalDT { get; private set; }

        public static void SetValues(float dt, float totalDt)
        {
            DT = dt;
            TotalDT = totalDt;
        }
    }
}