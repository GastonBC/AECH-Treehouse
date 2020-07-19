#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RLD
{
    public static class EditorUndoEx
    {
        public static void Record(UnityEngine.Object objectToRecord)
        {
            if (!Application.isPlaying) Undo.RecordObject(objectToRecord, "RTTGizmos Undo");
        }
    }
}
#endif