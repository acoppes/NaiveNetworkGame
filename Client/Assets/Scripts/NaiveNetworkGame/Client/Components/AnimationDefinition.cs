using System.Collections.Generic;
using UnityEngine;

namespace NaiveNetworkGame.Client.Components
{
    [CreateAssetMenu(menuName = "Game/Sprites Animation")]
    public class AnimationDefinition : ScriptableObject
    {
        public List<Sprite> sprites;
        public float fps;
    }
}