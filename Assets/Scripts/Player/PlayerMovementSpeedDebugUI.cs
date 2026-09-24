using System.Globalization;
using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovementSpeedDebugUI : MonoBehaviour
    {
        private const float LabelWidth = 220f;
        private const float LabelHeight = 36f;
        private const float RightMargin = 16f;

        private Rigidbody2D body;
        private GUIStyle labelStyle;
        private string speedText = "이동 속도: 0.00";

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void LateUpdate()
        {
            float actualSpeed = body.linearVelocity.magnitude;
            speedText = "이동 속도: " + actualSpeed.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void OnGUI()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                    fontSize = 20,
                    normal = { textColor = Color.white }
                };
            }

            Rect labelRect = new Rect(
                Screen.width - LabelWidth - RightMargin,
                (Screen.height - LabelHeight) * 0.5f,
                LabelWidth,
                LabelHeight);

            GUI.Label(labelRect, speedText, labelStyle);
        }
    }
}
