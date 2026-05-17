using UnityEngine;
using com.pyroduck.eggheads.Runtime.Scripts.Character;
using com.pyroduck.eggheads.Runtime.Scripts.Events;
using com.pyroduck.eggheads.Runtime.Scripts.Combat;

namespace com.pyroduck.eggheads.Runtime.Scripts.Data
{
    [CreateAssetMenu(fileName = "BodyPartSO", menuName = "Scriptable Objects/BodyPartSO")]
    public class VisualDataSO : ScriptableObject
    {
        public string Name;
        public GameObject BodyPartPrefab;
        public Sprite IconSprite;
        public bool isColorable;
    }
}

