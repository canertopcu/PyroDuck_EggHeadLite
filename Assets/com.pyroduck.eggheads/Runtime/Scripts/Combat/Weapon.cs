using com.pyroduck.eggheads.Runtime.Scripts.Character;
using UnityEngine; 
using com.pyroduck.eggheads.Runtime.Scripts.Pool;

namespace com.pyroduck.eggheads.Runtime.Scripts.Combat
{
    public enum WeaponCategory
    {
        Throwable,
        Ranged,
        Melee
    }

    public enum ThrowableAttackType
    {
        ThrowOnly,       // Throw, stick 2s, drop — must be retrieved
        ThrowAndReturn,  // Boomerang — returns to owner
        Explosive,       // Explodes on impact — respawns in hand
        Shuriken         // Unlimited ammo — sticks 5s then destroys
    }

    public enum RangedAttackType
    {
        Gun,
        Rifle,
        Shotgun,
        Sniper,
        RocketLauncher
    }

    public enum MeleeAttackType
    {
        Sword,
        BaseballBat,
        Pan
    }

    public interface IWeaponAttack
    {
        bool TryAttack(Vector2 attackDirection, bool fireDown, bool fireHeld);
    }

    public interface IMeleeWeapon
    {
        void ExecuteMeleeAttack(Vector2 attackDirection);
    }

    public interface IRangedWeapon
    {
        void ExecuteRangedAttack(Vector2 attackDirection);
    }

    public interface IThrowableWeapon
    {
        void ExecuteThrowableAttack(Vector2 attackDirection);
    }

    public abstract class Weapon : MonoBehaviour, IWeaponAttack
    {
        [Header("Audio")] 
        [SerializeField] protected AudioClip equipSound;
        [SerializeField] protected AudioClip dropSound;
        [SerializeField] protected AudioClip hitSound;
        [SerializeField] protected AudioSource audioSource;
       

        protected ProjectilePool _pool;
        private GameObject _owner;
        public GameObject Owner => _owner;
        protected virtual void Awake()
        {
            _pool = GetComponentInParent<ProjectilePool>();
        }
 
        public void SetOwner(GameObject owner)
        {
            _owner = owner;
        }
        public virtual void SetPool(ProjectilePool pool)
        {
            _pool = pool;
        }

        [Header("Common Settings")] [SerializeField]
        protected float attackCooldown = 0.2f;

        protected float _nextAttackTime;

        public virtual bool TryAttack(Vector2 attackDirection, bool fireDown, bool fireHeld)
        {
            if (Time.time < _nextAttackTime) return false;
            if (!CanAttack(fireDown, fireHeld)) return false;

            EnsureOwnerBound();
            _nextAttackTime = Time.time + attackCooldown;
            ExecuteAttack(attackDirection); 
            return true;
        }

        protected void EnsureOwnerBound()
        {
            if (_owner != null) return;

            var character = GetComponentInParent<BaseCharacterController>();
            if (character != null)
            {
                _owner = character.transform.root != null
                    ? character.transform.root.gameObject
                    : character.gameObject;
            }
        }
         
        public virtual void PlayEquipSound()
        {
            if (equipSound != null)
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(equipSound);
                else
                    AudioSource.PlayClipAtPoint(equipSound, transform.position);
            }
        }

        public virtual void PlayDropSound()
        {
            if (dropSound != null)
            {
                AudioSource.PlayClipAtPoint(dropSound, transform.position);
            }
        }
        
        public virtual void PlayHitSound()
        {
            if (hitSound != null)
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(hitSound);
                else
                    AudioSource.PlayClipAtPoint(hitSound, transform.position);
            }
        }

        protected abstract bool CanAttack(bool fireDown, bool fireHeld);
        protected abstract void ExecuteAttack(Vector2 attackDirection);
 
    }
}
