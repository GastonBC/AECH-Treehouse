using System.Collections.Generic;
using UnityEngine;

namespace RLD
{
    public static class SpriteEx
    {
        public static List<Vector3> GetWorldVerts(this Sprite sprite, Transform spriteTransform)
        {
            List<Vector3> verts = new List<Vector3>(sprite.GetModelVerts());
            spriteTransform.TransformPoints(verts);
            return verts;
        }

        public static List<Vector3> GetModelVerts(this Sprite sprite)
        {
            List<Vector3> verts = new List<Vector3>(7);
            Vector2[] modelVerts = sprite.vertices;

            foreach (Vector2 pt in modelVerts)
                verts.Add(pt);

            return verts;
        }
    }
}
