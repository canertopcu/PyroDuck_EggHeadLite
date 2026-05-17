using UnityEngine;

namespace com.pyroduck.eggheads.Runtime.Scripts.Combat
{
    /// <summary>
    /// Attach to spike / hazard objects in the scene.
    /// The character resolves this component from its collision or trigger
    /// contact and applies <see cref="Damage"/> to itself.
    /// </summary>
    public class SpikeHazard : MonoBehaviour
    {
        [SerializeField] private float damage = 20f;

        public float Damage => damage;
    }
}
