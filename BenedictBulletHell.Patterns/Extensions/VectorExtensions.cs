using System.Numerics;

namespace BenedictBulletHell.Patterns.Extensions
{
    /// <summary>
    /// Extension methods for converting between different Vector2 types.
    /// </summary>
    public static class VectorExtensions
    {
#if MONOGAME || FNA
        /// <summary>
        /// Converts a System.Numerics.Vector2 to MonoGame/FNA Vector2.
        /// </summary>
        public static Microsoft.Xna.Framework.Vector2 ToXna(this Vector2 v)
        {
            return new Microsoft.Xna.Framework.Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Converts a MonoGame/FNA Vector2 to System.Numerics.Vector2.
        /// </summary>
        public static Vector2 FromXna(Microsoft.Xna.Framework.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }
#endif

#if UNITY
        /// <summary>
        /// Converts a System.Numerics.Vector2 to Unity Vector2.
        /// </summary>
        public static UnityEngine.Vector2 ToUnity(this Vector2 v)
        {
            return new UnityEngine.Vector2(v.X, v.Y);
        }

        /// <summary>
        /// Converts a Unity Vector2 to System.Numerics.Vector2.
        /// </summary>
        public static Vector2 FromUnity(UnityEngine.Vector2 v)
        {
            return new Vector2(v.x, v.y);
        }
#endif
    }
}


