using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Phoenix3D
{
    public class OpacityChanger : MonoBehaviour
    {
        private Text textComponent;
        private float timeElapsed = 0f;
        private float duration = 1f;
        private float startOpacity = 0.25f;
        private float endOpacity = 1f;

        void Start()
        {
            textComponent = GetComponent<Text>();
        }

        void Update()
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            float opacity = Mathf.Lerp(startOpacity, endOpacity, t);
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, opacity);
            if (t >= 1f)
            {
                float temp = startOpacity;
                startOpacity = endOpacity;
                endOpacity = temp;
                timeElapsed = 0f;
            }
        }
    }
}
