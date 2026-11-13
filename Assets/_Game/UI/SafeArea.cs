using UnityEngine;
using UnityEngine.UIElements;

namespace OutOfOffice.UI
{
    /// <summary>
    /// "safe-area" sınıflı elemanlara cihazın çentik ve köşe boşluklarını iç boşluk olarak uygular.
    /// Değerler cihaza göre değiştiği için burada (ve yalnızca burada) satır içi stil kullanılır.
    /// </summary>
    public static class SafeArea
    {
        public static void Apply(VisualElement root)
        {
            root.Query(className: "safe-area").ForEach(element =>
                element.RegisterCallback<GeometryChangedEvent, VisualElement>(OnGeometryChanged, element));
        }

        static void OnGeometryChanged(GeometryChangedEvent evt, VisualElement element)
        {
            var panel = element.panel;
            if (panel == null || Screen.width <= 0 || Screen.height <= 0) return;

            // Panel birimi / ekran pikseli.
            float scale = panel.visualTree.layout.width / Screen.width;
            var safe = Screen.safeArea;

            Set(element.style.paddingLeft, safe.xMin * scale, v => element.style.paddingLeft = v);
            Set(element.style.paddingRight, (Screen.width - safe.xMax) * scale, v => element.style.paddingRight = v);
            Set(element.style.paddingTop, (Screen.height - safe.yMax) * scale, v => element.style.paddingTop = v);
            Set(element.style.paddingBottom, safe.yMin * scale, v => element.style.paddingBottom = v);
        }

        static void Set(StyleLength current, float value, System.Action<float> apply)
        {
            value = Mathf.Round(value);
            if (current.keyword == StyleKeyword.Undefined && Mathf.Approximately(current.value.value, value)) return;
            apply(value);
        }
    }
}
