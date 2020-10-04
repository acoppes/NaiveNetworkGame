using UnityEngine;

namespace Client
{
    public class RandomSpriteDeco : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer model;
        
        [SerializeField]
        private Sprite[] sprites;
        
        private void Start()
        {
            if (model != null && sprites.Length > 0)
            {
                var randomSprite = UnityEngine.Random.Range(0, sprites.Length);
                model.sprite = sprites[randomSprite];
            }
        }
    }
}
