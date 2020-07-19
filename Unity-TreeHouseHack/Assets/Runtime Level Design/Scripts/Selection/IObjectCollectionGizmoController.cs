using System.Collections.Generic;
using UnityEngine;

namespace RLD
{
    public interface IObjectCollectionGizmoController
    {
        void SetTargetObjectCollection(IEnumerable<GameObject> targetObjectCollection);
    }
}
