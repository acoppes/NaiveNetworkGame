using UnityEngine;

namespace NaiveNetworkGame.Common
{
    [CreateAssetMenu(menuName = "NaiveGame/Player Action Type", fileName = "PlayerActionAsset", order = 0)]
    public class PlayerActionAsset : ScriptableObject
    {
        public byte type;
    }
}