using UnityEngine;

namespace Scripts.Player
{
    [CreateAssetMenu(fileName = "PlayerSO", menuName="ScriptableObjects/PlayerSO")]
    public class PlayerSO: ScriptableObject
    {
        public string characterName;
    }
}