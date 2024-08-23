using UnityEngine;

namespace Utilities
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Sets any values of the Vector3
        /// </summary>
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }
    }
}
