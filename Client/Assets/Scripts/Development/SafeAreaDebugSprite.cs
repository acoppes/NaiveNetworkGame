using UnityEngine;

namespace Development
{
    [ExecuteInEditMode]
    public class SafeAreaDebugSprite : MonoBehaviour
    {
        public float width;
        public float height;

        public float ppu;
        
        public SpriteRenderer sprite;

        public bool debugArea = true;
        
        private void LateUpdate()
        {
            var safeAreaColor = sprite.color;
            safeAreaColor.a = 0.15f;
            
            if (sprite != null)
                sprite.color = safeAreaColor;
            
            if (ppu > 0)
                transform.localScale = new Vector3(width / ppu, height / ppu, 1);
            
            sprite.enabled = debugArea;
        }
    }
}