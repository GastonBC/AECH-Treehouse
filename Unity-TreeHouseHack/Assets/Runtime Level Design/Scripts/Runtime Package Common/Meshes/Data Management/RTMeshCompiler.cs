using UnityEngine;

namespace RLD
{
    public static class RTMeshCompiler 
    {
        public static void CompileEntireScene()
        {
            GameObject[] sceneObjects = RTScene.Get.GetSceneObjects();
            foreach(GameObject sceneObject in sceneObjects)
                CompileForObject(sceneObject);
        }

        public static bool CompileForObject(GameObject gameObject)
        {
            if (gameObject.isStatic) return false;

            Mesh mesh = gameObject.GetMesh();
            if (mesh == null) return false;

            RTMesh rtMesh = RTMeshDb.Get.GetRTMesh(mesh);
            if (rtMesh == null) return false;

            if (!rtMesh.IsTreeBuilt) rtMesh.BuildTree();
            return true;
        }
    }
}
