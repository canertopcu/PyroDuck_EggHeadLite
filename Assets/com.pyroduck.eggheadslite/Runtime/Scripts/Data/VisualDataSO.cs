using UnityEngine;
using com.pyroduck.eggheadslite.Runtime.Scripts.Character;
using com.pyroduck.eggheadslite.Runtime.Scripts.Events;
using com.pyroduck.eggheadslite.Runtime.Scripts.Combat;

namespace com.pyroduck.eggheadslite.Runtime.Scripts.Data
{
    [CreateAssetMenu(fileName = "BodyPartSO", menuName = "PyroDuck/EggHeadsLite/Visual Data")]
    public class VisualDataSO : ScriptableObject
    {
        public string Name;
        public GameObject BodyPartPrefab;
        public Sprite IconSprite;
        public bool isColorable;
    }
}

