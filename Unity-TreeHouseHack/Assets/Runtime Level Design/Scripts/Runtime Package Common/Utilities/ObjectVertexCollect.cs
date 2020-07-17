using UnityEngine;
using System.Collections.Generic;

namespace RLD
{
    public static class ObjectVertexCollect
    {
        public static List<Vector3> CollectModelSpriteVerts(Sprite sprite, AABB collectAABB)
        {
            Vector2[] spriteModelVerts = sprite.vertices;
            List<Vector3> collectedVerts = new List<Vector3>(7);

            foreach(Vector2 vertPos in spriteModelVerts)
            {
                if(BoxMath.ContainsPoint(vertPos, collectAABB.Center, collectAABB.Size, Quaternion.identity))
                    collectedVerts.Add(vertPos);
            }

            return collectedVerts;
        }

        public static List<Vector3> CollectWorldSpriteVerts(Sprite sprite, Transform spriteTransform, OBB collectOBB)
        {
            List<Vector3> spriteWorldVerts = sprite.GetWorldVerts(spriteTransform);
            List<Vector3> collectedVerts = new List<Vector3>(7);

            foreach (Vector3 vertPos in spriteWorldVerts)
            {
                if (BoxMath.ContainsPoint(vertPos, collectOBB.Center, collectOBB.Size, collectOBB.Rotation))
                    collectedVerts.Add(vertPos);
            }

            return collectedVerts;
        }

        public static List<Vector3> CollectHierarchyVerts(GameObject root, BoxFace collectFace, float collectBoxScale, float collectEps)
        {
            List<GameObject> meshObjects = root.GetMeshObjectsInHierarchy();
            List<GameObject> spriteObjects = root.GetSpriteObjectsInHierarchy();
            if (meshObjects.Count == 0 && spriteObjects.Count == 0) return new List<Vector3>();

            ObjectBounds.QueryConfig boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectType.Mesh | GameObjectType.Sprite;

            OBB hierarchyOBB = ObjectBounds.CalcHierarchyWorldOBB(root, boundsQConfig);
            if (!hierarchyOBB.IsValid) return new List<Vector3>();

            int faceAxisIndex = BoxMath.GetFaceAxisIndex(collectFace);
            Vector3 faceCenter = BoxMath.CalcBoxFaceCenter(hierarchyOBB.Center, hierarchyOBB.Size, hierarchyOBB.Rotation, collectFace);
            Vector3 faceNormal = BoxMath.CalcBoxFaceNormal(hierarchyOBB.Center, hierarchyOBB.Size, hierarchyOBB.Rotation, collectFace);

            float sizeEps = collectEps * 2.0f;
            Vector3 collectAABBSize = hierarchyOBB.Size;
            collectAABBSize[faceAxisIndex] = (hierarchyOBB.Size[faceAxisIndex] * collectBoxScale) + sizeEps;   
            collectAABBSize[(faceAxisIndex + 1) % 3] += sizeEps;
            collectAABBSize[(faceAxisIndex + 2) % 3] += sizeEps;

            OBB collectOBB = new OBB(faceCenter + faceNormal * (-collectAABBSize[faceAxisIndex] * 0.5f + collectEps), collectAABBSize);
            collectOBB.Rotation = hierarchyOBB.Rotation;

            List<Vector3> collectedVerts = new List<Vector3>(80);
            foreach(GameObject meshObject in meshObjects)
            {
                Mesh mesh = meshObject.GetMesh();
                RTMesh rtMesh = RTMeshDb.Get.GetRTMesh(mesh);
                if (rtMesh == null) continue;

                List<Vector3> verts = rtMesh.OverlapVerts(collectOBB, meshObject.transform);
                if (verts.Count != 0) collectedVerts.AddRange(verts);
            }

            foreach (GameObject spriteObject in spriteObjects)
            {
                List<Vector3> verts = CollectWorldSpriteVerts(spriteObject.GetSprite(), spriteObject.transform, collectOBB);
                if (verts.Count != 0) collectedVerts.AddRange(verts);
            }

            return collectedVerts;
        }
    }
}
