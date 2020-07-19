using System.Collections.Generic;
using UnityEngine;

namespace RLD
{
    public class Object2ObjectSnapDataDb : Singleton<Object2ObjectSnapDataDb>
    {
        private Dictionary<GameObject, Object2ObjectSnapData> _objectToSnapData = new Dictionary<GameObject, Object2ObjectSnapData>();

        public Object2ObjectSnapData GetObject2ObjectSnapData(GameObject gameObject)
        {
            if (_objectToSnapData.ContainsKey(gameObject)) return _objectToSnapData[gameObject];

            Object2ObjectSnapData snapData = new Object2ObjectSnapData();
            if (!snapData.Initialize(gameObject)) return null;

            _objectToSnapData.Add(gameObject, snapData);
            return snapData;
        }
    }
}
