using UnityEngine;

namespace RLD
{
    public class GizmoThinCircle3DBorderController : GizmoCircle3DBorderController
    {
        public GizmoThinCircle3DBorderController(GizmoCircle3DBorderControllerData controllerData)
            :base(controllerData)
        {
        }

        public override void UpdateHandles()
        {
            _data.TargetHandle.Set3DShapeVisible(_data.BorderTorusIndex, false);
            _data.TargetHandle.Set3DShapeVisible(_data.BorderCylTorusIndex, false);
            _data.TargetHandle.Set3DShapeVisible(_data.BorderCircleIndex, _data.Border.IsVisible);
        }

        public override void UpdateEpsilons(float zoomFactor)
        {
            _data.BorderCircle.WireEps = zoomFactor * _data.PlaneSlider.Settings.BorderLineHoverEps;
            _data.BorderCircle.ExtrudeEps = _data.BorderCircle.WireEps;
        }

        public override void UpdateTransforms(float zoomFactor)
        {
            CircleShape3D targetCircle = _data.TargetCircle;
            CircleShape3D borderCircle = _data.BorderCircle;

            borderCircle.Rotation = targetCircle.Rotation;
            borderCircle.Center = targetCircle.Center;
            borderCircle.Radius = targetCircle.Radius;
        }
    }
}
