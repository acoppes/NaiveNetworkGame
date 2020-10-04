using UnityEngine;

namespace Development
{
    [ExecuteInEditMode]
    public class PixelPerfectCameraSizeScript : MonoBehaviour {

        public Camera myCamera;

        // public bool useWidth;
        public int dpi = 100;
        public float ppuScale = 1;

        public void Update()
        {
            if (myCamera == null)
                return;
            
            // * Orthographic size = ((Vert Resolution)/(PPUScale * PPU)) * 0.5
            var div = (ppuScale * dpi);
            if (div <= 0.001f)
                return;
            myCamera.orthographicSize = 0.5f * (myCamera.pixelHeight / (ppuScale * dpi));
        }
    }
}