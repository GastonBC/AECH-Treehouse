﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

namespace RLD
{
    public delegate void SceneCanRenderCameraIconHandler(Camera camera, YesNoAnswer answer);

    public class RTScene : MonoSingleton<RTScene>
    {
        public SceneCanRenderCameraIconHandler CanRenderCameraIcon;

        private static readonly float _nullCleanupTargetTime = 1.0f;
        private float _elapsedNullCleanupTime = 0.0f;

        private YesNoAnswer _yesNoAnswer = new YesNoAnswer();
        [SerializeField]
        private SceneSettings _settings = new SceneSettings();
        [SerializeField]
        private SceneLookAndFeel _lookAndFeel = new SceneLookAndFeel();

        private List<Camera> _iconRenderIgnoreCamera = new List<Camera>();
        private HashSet<GameObject> _ignoredRootObjects = new HashSet<GameObject>();

        private List<IHoverableSceneEntityContainer> _hoverableSceneEntityContainers = new List<IHoverableSceneEntityContainer>();
        private SceneTree _sceneTree = new SceneTree();

        private List<GameObject> _childrenAndSelf = new List<GameObject>(100);
        private List<GameObject> _rootGameObjects = new List<GameObject>();
        private List<Light> _lights = new List<Light>();
        private List<ParticleSystem> _particleSystems = new List<ParticleSystem>();
        private List<Camera> _cameras = new List<Camera>();

        public SceneSettings Settings { get { return _settings; } }
        public SceneLookAndFeel LookAndFeel { get { return _lookAndFeel; } }

        public void SetRootObjectIgnored(GameObject root, bool ignored)
        {
            if (ignored) _ignoredRootObjects.Add(root);
            else _ignoredRootObjects.Remove(root);
        }

        public void AddIconRenderIgnoreCamera(Camera camera)
        {
            if (!IsIconRenderIgnoreCamera(camera))
            {
                _iconRenderIgnoreCamera.Add(camera);
            }
        }

        public bool IsIconRenderIgnoreCamera(Camera camera)
        {
            return _iconRenderIgnoreCamera.Contains(camera);
        }

        public AABB CalculateBounds()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            List<GameObject> roots = new List<GameObject>(Mathf.Max(10, activeScene.rootCount));
            SceneManager.GetActiveScene().GetRootGameObjects(roots);

            ObjectBounds.QueryConfig boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.NoVolumeSize = Vector3.zero;
            boundsQConfig.ObjectTypes = GameObjectType.Mesh | GameObjectType.Sprite;

            AABB sceneAABB = new AABB();
            foreach(GameObject root in roots)
            {
                List<GameObject> allChildrenAndSelf = root.GetAllChildrenAndSelf();
                foreach(GameObject sceneObject in allChildrenAndSelf)
                {
                    AABB aabb = ObjectBounds.CalcWorldAABB(sceneObject, boundsQConfig);
                    if(aabb.IsValid)
                    {
                        if (sceneAABB.IsValid) sceneAABB.Encapsulate(aabb);
                        else sceneAABB = aabb;
                    }
                }
            }

            return sceneAABB;
        }

        public bool IsAnySceneEntityHovered()
        {
            foreach (IHoverableSceneEntityContainer container in _hoverableSceneEntityContainers)
                if (container.HasHoveredSceneEntity) return true;

            return IsAnyUIElementHovered();
        }

        public void RegisterHoverableSceneEntityContainer(IHoverableSceneEntityContainer container)
        {
            if (!_hoverableSceneEntityContainers.Contains(container)) _hoverableSceneEntityContainers.Add(container);
        }

        public bool IsAnyUIElementHovered()
        {
            return GetHoveredUIElements().Count != 0;
        }

        public List<RaycastResult> GetHoveredUIElements()
        {
            // No event system? Return an empty list.
            if (EventSystem.current == null) return new List<RaycastResult>();

            // Get the input device's screen coords. If the coords are not available, return an empty list.
            IInputDevice inputDevice = RTInputDevice.Get.Device;
            if (!inputDevice.HasPointer()) return new List<RaycastResult>();
            Vector2 inputDevicePos = inputDevice.GetPositionYAxisUp();

            // Construct the pointer event data instance needed for the raycast
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(inputDevicePos.x, inputDevicePos.y);

            // Raycast all and return the result
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            results.RemoveAll(item => item.gameObject.GetComponent<RectTransform>() == null);

            return results;
        }

        public GameObject[] GetSceneObjects()
        {
            return GameObject.FindObjectsOfType<GameObject>();
        }

        public List<GameObject> OverlapBox(OBB obb)
        {
            if(Settings.PhysicsMode == ScenePhysicsMode.UnityColliders)
            {
                // Retrieve the overlapped 3D objects and store them inside the output list
                Collider[] overlapped3DColliders = Physics.OverlapBox(obb.Center, obb.Extents, obb.Rotation);
                List<GameObject> overlappedObjects = new List<GameObject>(50);
                foreach (Collider collider in overlapped3DColliders) overlappedObjects.Add(collider.gameObject);

                // Retrieve the overlapped 2D objects. This requires some further calculations
                // because colliders of 2D sprites exist in the XY plane, so we must project the
                // OBB onto the XY plane, and use the min and max projected points to build an 
                // overlap area.
                Plane xyPlane = new Plane(Vector3.forward, 0.0f);
                List<Vector3> projectedCorners = xyPlane.ProjectAllPoints(obb.GetCornerPoints());
                AABB projectedPtsAABB = new AABB(projectedCorners);

                // The OBB has been projected onto the sprite XY plane. We can now use the AABB's min and max
                // extents to construct the overlap area and use the 'OverlapAreaAll' function to retrieve the
                // overlapped 2D colliders.
                Collider2D[] overlapped2DColliders = Physics2D.OverlapAreaAll(projectedPtsAABB.Min, projectedPtsAABB.Max);
                foreach (Collider2D collider in overlapped2DColliders) overlappedObjects.Add(collider.gameObject);
               
                return overlappedObjects;
            }
            else return _sceneTree.OverlapBox(obb);
        }

        public List<GameObject> OverlapBox(OBB obb, SceneOverlapFilter overlapFilter)
        {
            List<GameObject> gameObjects = OverlapBox(obb);
            overlapFilter.FilterOverlaps(gameObjects);

            return gameObjects;
        }

        public SceneRaycastHit Raycast(Ray ray, SceneRaycastPrecision rtRaycastPrecision, SceneRaycastFilter raycastFilter)
        {
            List<GameObjectRayHit> allObjectHits = RaycastAllObjectsSorted(ray, rtRaycastPrecision, raycastFilter);
            GameObjectRayHit closestObjectHit = allObjectHits.Count != 0 ? allObjectHits[0] : null;
            XZGridRayHit gridRayHit = RaycastSceneGridIfVisible(ray);

            return new SceneRaycastHit(closestObjectHit, gridRayHit);
        }

        public List<GameObjectRayHit> RaycastAllObjects(Ray ray, SceneRaycastPrecision rtRaycastPrecision)
        {
            if (Settings.PhysicsMode == ScenePhysicsMode.UnityColliders)
            {
                RaycastHit[] hits3D = Physics.RaycastAll(ray, float.MaxValue);
                RaycastHit2D[] hits2D = Physics2D.GetRayIntersectionAll(ray, float.MaxValue);

                List<GameObjectRayHit> allHits = new List<GameObjectRayHit>(GameObjectRayHit.Create(ray, hits3D));
                allHits.AddRange(GameObjectRayHit.Create(ray, hits2D));

                return allHits;
            }
            else return _sceneTree.RaycastAll(ray, rtRaycastPrecision);
        }

        public List<GameObjectRayHit> RaycastAllObjectsSorted(Ray ray, SceneRaycastPrecision raycastPresicion)
        {
            List<GameObjectRayHit> allHits = RaycastAllObjects(ray, raycastPresicion);
            GameObjectRayHit.SortByHitDistance(allHits);
  
            return allHits;
        }

        public List<GameObjectRayHit> RaycastAllObjectsSorted(Ray ray, SceneRaycastPrecision rtRaycastPrecision, SceneRaycastFilter raycastFilter)
        {
            if (raycastFilter != null && 
                raycastFilter.AllowedObjectTypes.Count == 0) return new List<GameObjectRayHit>();

            List<GameObjectRayHit> sortedHits = RaycastAllObjectsSorted(ray, rtRaycastPrecision);
            if (raycastFilter != null) raycastFilter.FilterHits(sortedHits);

            return sortedHits;
        }

        public GameObjectRayHit RaycastMeshObject(Ray ray, GameObject meshObject)
        {
            if (Settings.PhysicsMode == ScenePhysicsMode.UnityColliders)
            {
                Collider raycastCollider = null;
                MeshCollider meshCollider = meshObject.GetComponent<MeshCollider>();
                if(meshCollider != null) raycastCollider = meshCollider;
                if(raycastCollider == null) raycastCollider = meshObject.GetComponent<Collider>();

                if(raycastCollider != null)
                {
                    RaycastHit rayHit;
                    if (raycastCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);
                }
                return null;
            }
            else return _sceneTree.RaycastMeshObject(ray, meshObject);
        }

        public GameObjectRayHit RaycastMeshObjectReverseIfFail(Ray ray, GameObject meshObject)
        {
            GameObjectRayHit hit = RaycastMeshObject(ray, meshObject);
            if (hit == null) hit = RaycastMeshObject(new Ray(ray.origin, -ray.direction), meshObject);

            return hit;
        }

        public GameObjectRayHit RaycastSpriteObject(Ray ray, GameObject spriteObject)
        {
            // NOTE: 'ObjectInteractionMode.UnityColliders' must be ignored here as there doesn't seem to
            //       be a way to raycast against a 2D collider.
            return _sceneTree.RaycastSpriteObject(ray, spriteObject);
        }

        public GameObjectRayHit RaycastTerrainObject(Ray ray, GameObject terrainObject)
        {
            TerrainCollider terrainCollider = terrainObject.GetComponent<TerrainCollider>();
            if (terrainCollider == null) return null;

            RaycastHit rayHit;
            if (terrainCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);

            return null;
        }

        public GameObjectRayHit RaycastTerrainObject(Ray ray, GameObject terrainObject, TerrainCollider terrainCollider)
        {
            RaycastHit rayHit;
            if (terrainCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);

            return null;
        }

        public GameObjectRayHit RaycastTerrainObjectReverseIfFail(Ray ray, GameObject terrainObject)
        {
            GameObjectRayHit hit = RaycastTerrainObject(ray, terrainObject);
            if (hit == null) hit = RaycastTerrainObject(new Ray(ray.origin, -ray.direction), terrainObject);

            return hit;
        }

        public XZGridRayHit RaycastSceneGridIfVisible(Ray ray)
        {
            if (!RTSceneGrid.Get.Settings.IsVisible) return null;

            float t;
            if(RTSceneGrid.Get.Raycast(ray, out t))
            {
                XZGridCell hitCell = RTSceneGrid.Get.CellFromWorldPoint(ray.GetPoint(t));
                return new XZGridRayHit(ray, hitCell, t);
            }

            return null;
        }

        public void Update_SystemCall()
        {
            _elapsedNullCleanupTime += Time.deltaTime;
            if (_elapsedNullCleanupTime >= _nullCleanupTargetTime)
            {
                _sceneTree.RemoveNodesWithNullObjects();
                RTMeshDb.Get.RemoveNullMeshEntries();
                _lights.RemoveAll(item => item == null);
                _particleSystems.RemoveAll(item => item == null);
                _cameras.RemoveAll(item => item == null);
                _elapsedNullCleanupTime = 0.0f;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            int numRoots = activeScene.rootCount;
            if (_rootGameObjects.Capacity <= numRoots) _rootGameObjects.Capacity = numRoots + 100;
            activeScene.GetRootGameObjects(_rootGameObjects);

            for (int rootIndex = 0; rootIndex < numRoots; ++rootIndex)
            {
                GameObject rootObject = _rootGameObjects[rootIndex];
                if (_ignoredRootObjects.Contains(rootObject)) continue;

                rootObject.GetAllChildrenAndSelf(_childrenAndSelf);

                int numObjectsInHierarchy = _childrenAndSelf.Count;
                for (int sceneObjIndex = 0; sceneObjIndex < numObjectsInHierarchy; ++sceneObjIndex)
                {
                    GameObject sceneObject = _childrenAndSelf[sceneObjIndex];
                    if (!_sceneTree.IsObjectRegistered(sceneObject))
                    {
                        _sceneTree.RegisterObject(sceneObject);

                        Light light = sceneObject.GetComponent<Light>();
                        if (light != null) _lights.Add(light);

                        ParticleSystem particleSystem = sceneObject.GetComponent<ParticleSystem>();
                        if (particleSystem != null) _particleSystems.Add(particleSystem);

                        Camera camera = sceneObject.GetComponent<Camera>();
                        if (camera != null && !RTGizmosEngine.Get.IsSceneGizmoCamera(camera)) _cameras.Add(camera);
                    }
                    else
                    {
                        Transform objectTransform = sceneObject.transform;
                        if (objectTransform.hasChanged)
                        {
                            _sceneTree.OnObjectTransformChanged(objectTransform);
                            objectTransform.hasChanged = false;
                        }
                    }
                }
            }
        }

        public void Render_SystemCall()
        {
            Material iconMaterial = MaterialPool.Get.TintedTexture;
            Mesh iconMesh = MeshPool.Get.UnitQuadXY;

            Camera renderCam = Camera.current;
            if (IsIconRenderIgnoreCamera(renderCam)) return;

            Transform renderCamTransform = renderCam.transform;
            Vector3 renderCamPos = renderCamTransform.position;
            Quaternion renderCamRotation = renderCamTransform.rotation;

            if (LookAndFeel.DrawCameraIcons && LookAndFeel.CameraIcon != null)
            {
                iconMaterial.SetTexture("_MainTex", LookAndFeel.CameraIcon);
                iconMaterial.SetColor(ColorEx.KeepAllButAlpha(Color.white, LookAndFeel.CameraIconAlpha));
                iconMaterial.SetZTestAlways();
                iconMaterial.SetPass(0);

                Camera targetFocusCamera = RTFocusCamera.Get.TargetCamera;

                Vector3 scale = new Vector3(Settings.NonMeshObjectSize, Settings.NonMeshObjectSize, 1.0f);
                for (int cameraIndex = 0; cameraIndex < _cameras.Count; ++cameraIndex)
                {
                    Camera camera = _cameras[cameraIndex];
                    if (camera != null && camera.gameObject.activeInHierarchy && !ReferenceEquals(targetFocusCamera, camera))
                    {
                        if (CanRenderCameraIcon != null)
                        {
                            CanRenderCameraIcon(camera, _yesNoAnswer);
                            if (_yesNoAnswer.HasNo) continue;
                        }

                        Transform objectTransform = camera.gameObject.transform;
                        Vector3 position = objectTransform.position;
                        Quaternion rotation = renderCamTransform.rotation;

                        Matrix4x4 transformMatrix = Matrix4x4.TRS(position, rotation, scale);
                        Graphics.DrawMeshNow(iconMesh, transformMatrix);
                    }
                }
            }

            if (LookAndFeel.DrawParticleSystemIcons && LookAndFeel.ParticleSystemIcon != null)
            {
                iconMaterial.SetTexture("_MainTex", LookAndFeel.ParticleSystemIcon);
                iconMaterial.SetColor(ColorEx.KeepAllButAlpha(Color.white, LookAndFeel.ParticleSystemIconAlpha));
                iconMaterial.SetZTestAlways();
                iconMaterial.SetPass(0);

                Vector3 scale = new Vector3(Settings.NonMeshObjectSize, Settings.NonMeshObjectSize, 1.0f);
                for (int psIndex = 0; psIndex < _particleSystems.Count; ++psIndex)
                {
                    ParticleSystem particleSystem = _particleSystems[psIndex];
                    if (particleSystem != null && particleSystem.gameObject.activeInHierarchy)
                    {
                        Transform objectTransform = particleSystem.gameObject.transform;
                        Vector3 position = objectTransform.position;
                        Quaternion rotation = renderCamTransform.rotation;

                        Matrix4x4 transformMatrix = Matrix4x4.TRS(position, rotation, scale);
                        Graphics.DrawMeshNow(iconMesh, transformMatrix);
                    }
                }
            }

            if (LookAndFeel.DrawLightIcons && LookAndFeel.LightIcon != null)
            {
                iconMaterial.SetTexture("_MainTex", LookAndFeel.LightIcon);
                iconMaterial.SetZTestAlways();

                Vector3 scale = new Vector3(Settings.NonMeshObjectSize, Settings.NonMeshObjectSize, 1.0f);
                for (int lightIndex = 0; lightIndex < _lights.Count; ++lightIndex)
                {
                    Light light = _lights[lightIndex];
                    if (light != null && light.enabled && light.gameObject.activeInHierarchy)
                    {
                        Transform objectTransform = light.gameObject.transform;
                        Vector3 position = objectTransform.position;
                        Quaternion rotation = renderCamTransform.rotation;

                        Matrix4x4 transformMatrix = Matrix4x4.TRS(position, rotation, scale);

                        iconMaterial.SetColor(ColorEx.KeepAllButAlpha(light.color, LookAndFeel.LightIconAlpha));
                        iconMaterial.SetPass(0);
                        Graphics.DrawMeshNow(iconMesh, transformMatrix);
                    }
                }
            }
        }
    }
}
