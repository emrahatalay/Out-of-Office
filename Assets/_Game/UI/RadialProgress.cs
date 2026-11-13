using UnityEngine;
using UnityEngine.UIElements;

namespace OutOfOffice.UI
{
    /// <summary>
    /// Dairesel ilerleme halkası (ör. yetenek bekleme süresi). Renk ve kalınlık USS'ten gelir:
    /// --ring-width, --ring-track-color, --ring-fill-color.
    /// </summary>
    [UxmlElement]
    public partial class RadialProgress : VisualElement
    {
        static readonly CustomStyleProperty<float> WidthProperty = new CustomStyleProperty<float>("--ring-width");
        static readonly CustomStyleProperty<Color> TrackProperty = new CustomStyleProperty<Color>("--ring-track-color");
        static readonly CustomStyleProperty<Color> FillProperty = new CustomStyleProperty<Color>("--ring-fill-color");

        float _progress = 1f;
        float _width = 10f;
        Color _track = new Color(1f, 1f, 1f, 0.15f);
        Color _fill = Color.white;

        /// <summary>0: boş, 1: dolu.</summary>
        [UxmlAttribute]
        public float progress
        {
            get => _progress;
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(value, _progress)) return;
                _progress = value;
                MarkDirtyRepaint();
            }
        }

        public RadialProgress()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
        }

        void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(WidthProperty, out float width)) _width = width;
            if (evt.customStyle.TryGetValue(TrackProperty, out var track)) _track = track;
            if (evt.customStyle.TryGetValue(FillProperty, out var fill)) _fill = fill;
            MarkDirtyRepaint();
        }

        void Draw(MeshGenerationContext context)
        {
            var rect = contentRect;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f - _width * 0.5f;
            if (radius <= 0f) return;

            var painter = context.painter2D;
            painter.lineWidth = _width;
            painter.lineCap = LineCap.Round;

            painter.strokeColor = _track;
            painter.BeginPath();
            painter.Arc(rect.center, radius, 0f, 360f);
            painter.Stroke();

            if (_progress <= 0f) return;

            painter.strokeColor = _fill;
            painter.BeginPath();
            painter.Arc(rect.center, radius, -90f, -90f + 360f * _progress);
            painter.Stroke();
        }
    }
}
