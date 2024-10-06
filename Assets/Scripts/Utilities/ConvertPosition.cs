using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class Convert
    {

        public static (int, int) PositionToGrid(Vector2 position, Vector3Int origin)
        {
            int x = (int)position.x - origin.x;
            int y = (int)position.y - origin.y;

            return (x, y);
        }
    }
}
