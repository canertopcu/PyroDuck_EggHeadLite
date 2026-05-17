using System.Collections.Generic;
using com.pyroduck.eggheads.Runtime.Scripts.Character;
using com.pyroduck.eggheads.Runtime.Scripts.Events;
using com.pyroduck.eggheads.Runtime.Scripts.Combat;
using com.pyroduck.eggheads.Runtime.Scripts.Enums;
using UnityEngine;

namespace com.pyroduck.eggheads.Runtime.Scripts.Data
{
    [CreateAssetMenu(fileName = "BodyPartGroupSO", menuName = "Scriptable Objects/BodyPartGroupSO")]
    public class VisualGroupsDataSO : ScriptableObject
    {
        public string Name;
        public bool IsSubGroup;
        public List<VisualDataSO> VisualList;
        public VisualType VisualType;
    }
}

