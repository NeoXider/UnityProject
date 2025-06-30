using UnityEngine;

using Object = UnityEngine.Object;

namespace Postica.Common
{
    /// <summary>
    /// Provides multiple additional utility functions to <see cref="Vector2"/>, <see cref="Vector3"/> and <see cref="Vector4"/> classes.
    /// </summary>
    internal static class VectorUtility
    {
        public static Vector2Int ToVector2Int(this Vector2 vector)
        {
            return new Vector2Int((int)vector.x, (int)vector.y);
        }
        
        public static Vector3Int ToVector3Int(this Vector3 vector)
        {
            return new Vector3Int((int)vector.x, (int)vector.y, (int)vector.z);
        }
        
        public static Vector4 ToVector4(this Vector3 vector, float w)
        {
            return new Vector4(vector.x, vector.y, vector.z, w);
        }
        
        public static Vector4 ToVector4(this Vector2 vector, float z, float w)
        {
            return new Vector4(vector.x, vector.y, z, w);
        }
        
        public static Vector4 ToVector4(this Vector2 vector, Vector2 other)
        {
            return new Vector4(vector.x, vector.y, other.x, other.y);
        }
        
        public static Vector2 Inverse(this Vector2 vector)
        {
            return new Vector2(vector.x != 0 ? 1f / vector.x : 1f, 
                               vector.y != 0 ? 1f / vector.y : 1f);
        }
        
        public static Vector3 Inverse(this Vector3 vector)
        {
            return new Vector3(vector.x != 0 ? 1f / vector.x : 1f, 
                               vector.y != 0 ? 1f / vector.y : 1f,
                               vector.z != 0 ? 1f / vector.z : 1f);
        }
        
        public static Vector4 Inverse(this Vector4 vector)
        {
            return new Vector4(vector.x != 0 ? 1f / vector.x : 1f, 
                               vector.y != 0 ? 1f / vector.y : 1f,
                               vector.z != 0 ? 1f / vector.z : 1f,
                               vector.w != 0 ? 1f / vector.w : 1f);
        }
        
        public static Vector2Int Inverse(this Vector2Int vector)
        {
            return new Vector2Int(vector.x != 0 ? 1 / vector.x : 1, 
                                  vector.y != 0 ? 1 / vector.y : 1);
        }
        
        public static Vector3Int Inverse(this Vector3Int vector)
        {
            return new Vector3Int(vector.x != 0 ? 1 / vector.x : 1, 
                                  vector.y != 0 ? 1 / vector.y : 1,
                                  vector.z != 0 ? 1 / vector.z : 1);
        }
    }

}
