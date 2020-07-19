namespace RLD
{
    public class GizmoThinCircle2DBorderController : GizmoCircle2DBorderController
    {
        public GizmoThinCircle2DBorderController(GizmoCircle2DBorderControllerData data)
            : base(data)
        {
        }

        public override void UpdateHandles()
        {
            _data.TargetHandle.Set2DShapeVisible(_data.BorderCircleIndex, _data.Border.IsVisible);
        }

        public override void UpdateEpsilons()
        {
            _data.BorderCircle.WireEps = _data.PlaneSlider.Settings.BorderLineHoverEps;
        }

        public override void UpdateTransforms()
        {
            CircleShape2D targetCircle = _data.TargetCircle;
            CircleShape2D borderCircle = _data.BorderCircle;

            borderCircle.Center = targetCircle.Center;
            borderCircle.RotationDegrees = targetCircle.RotationDegrees;
            borderCircle.Radius = targetCircle.Radius;
        }
    }
}
