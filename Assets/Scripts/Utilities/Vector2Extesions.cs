using UnityEngine;

namespace Utilities
{
    public static class Vector2Extensions
    {
        /// <summary>
        /// Sets any values of the Vector2
        /// </summary>
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(x ?? vector.x, y ?? vector.y);
        }
    }
}
