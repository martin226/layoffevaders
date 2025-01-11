using UnityEngine;

namespace Phoenix3D
{

    public class OpenURLScript : MonoBehaviour
    {
        public string url;

        public void Open()
        {
            Application.OpenURL(url);
        }
    }
}