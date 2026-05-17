using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using com.pyroduck.eggheads.Runtime.Scripts.Events;
using com.pyroduck.eggheads.Runtime.Scripts.Audio;
using com.pyroduck.eggheads.Runtime.Scripts.Combat;

namespace com.pyroduck.eggheads.Runtime.Scripts.Pool
{
    public class Projectile : MonoBehaviour, IPoolable
    {
        [Header("Motion")]
        [FormerlySerializedAs("speed")]
        [SerializeField] private float _speed = 15f;

        [FormerlySerializedAs("lifetime")]
        [SerializeField] private float _lifetime = 2f;

        [Header("Explosion")]
        [FormerlySerializedAs("isExplosive")]
        [SerializeField] private bool _isExplosive = false;

        [FormerlySerializedAs("explosionRadius")]
        [SerializeField] private float _explosionRadius = 3f;

        [SerializeField] private ExplosionEffect _explosionEffectPrefab;

        [Header("Effects")]
        [SerializeField] private ParticleSystem _bloodParticlePrefab;

        [Header("Impact Sounds")]
        [Tooltip("Random impact clips played on any surface. surfaceTag values on entries are ignored here.")]
        [SerializeField] private List<SoundData> impactSounds = new List<SoundData>();
        [SerializeField] private AudioSource audioSource;

        public float Speed => _speed;
        public float Lifetime => _lifetime;
        public bool IsExplosive => _isExplosive;
        public float ExplosionRadius => _explosionRadius;
        public float Damage { get; private set; }

        private float _lifeTimer;
        private Vector2 _moveDirection;
        private GameObject _owner;
        private bool _detonated;

        // NEW
        private Collider2D _myCollider;
        private List<Collider2D> _ignoredColliders = new List<Collider2D>();

        private void Awake()
        {
            _myCollider = GetComponent<Collider2D>();
            ProjectileCollisionRules.EnsureBulletThrowableIgnored();
        }

        // ── Pool ─────────────────────────────────────────

        public void OnSpawn()
        {
            _lifeTimer     = _lifetime;
            _detonated     = false;
            _moveDirection = Vector2.right;
            _owner         = null;
            Damage         = 0f;
        }

        public void OnDespawn()
        {
            ResetIgnoredCollisions();
        }

        // ── Init ─────────────────────────────────────────

        public void Initialize(Vector2 direction, float damage = 0f, GameObject owner = null)
        {
            _moveDirection = direction.normalized;
            Damage         = damage;
            _owner         = owner;
            _lifeTimer     = _lifetime;
            _detonated     = false;

            float angle = Mathf.Atan2(_moveDirection.y, _moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            SetupOwnerCollisionIgnore();
        }

        private void SetupOwnerCollisionIgnore()
        {
            if (_owner == null || _myCollider == null) return;

            var ownerColliders = _owner.GetComponentsInChildren<Collider2D>();

            foreach (var col in ownerColliders)
            {
                if (col == null) continue;

                Physics2D.IgnoreCollision(_myCollider, col, true);
                _ignoredColliders.Add(col);
            }
        }

        private void ResetIgnoredCollisions()
        {
            if (_myCollider == null) return;

            foreach (var col in _ignoredColliders)
            {
                if (col == null) continue;

                Physics2D.IgnoreCollision(_myCollider, col, false);
            }

            _ignoredColliders.Clear();
        }

        // ── Update ───────────────────────────────────────

        private void Update()
        {
            transform.Translate(_moveDirection * (_speed * Time.deltaTime), Space.World);

            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0) ReturnToPool();
        }

        // ── Collision ────────────────────────────────────

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_detonated) return;
            if (other.collider.isTrigger) return;
            if (TryIgnoreProjectileCollision(other.collider)) return;

            HandleHit(other.collider, other.contacts[0].point);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_detonated) return;
            if (other.isTrigger) return;
            if (TryIgnoreProjectileCollision(other)) return;

            HandleHit(other, other.bounds.center);
        }

        private void HandleHit(Collider2D other, Vector2 hitPoint)
        {
            if (IsOwnerHierarchy(other.transform)) return;

            if (_isExplosive)
            {
                Explode(hitPoint);
            }
            else
            {
                var damageable = other.GetComponent<IDamageable>()
                                 ?? other.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    damageable.TakeDamage(Damage, _owner, hitPoint);
                    PlayBloodParticle(hitPoint);
                }

                PlayImpactSound(hitPoint);
            }

            ReturnToPool();
        }

        private bool TryIgnoreProjectileCollision(Collider2D other)
        {
            if (!ProjectileCollisionRules.IsProjectileLike(other)) return false;

            ProjectileCollisionRules.IgnoreCollision(_myCollider, other, _ignoredColliders);
            return true;
        }

        private void PlayBloodParticle(Vector2 hitPoint)
        {
            if (_bloodParticlePrefab == null) return;
            var blood = Instantiate(_bloodParticlePrefab, hitPoint, Quaternion.identity);
            var main = blood.main;
            if (main.stopAction == ParticleSystemStopAction.None)
                main.stopAction = ParticleSystemStopAction.Destroy;
            blood.Play();
        }

        private void PlayImpactSound(Vector2 point)
        {
            if (!TryGetRandomImpactSound(out var chosen)) return;

            PlayImpactClipAtPoint(chosen, point);
        }

        private bool TryGetRandomImpactSound(out SoundData chosen)
        {
            chosen = default;
            if (impactSounds == null || impactSounds.Count == 0) return false;

            int validCount = 0;
            for (int i = 0; i < impactSounds.Count; i++)
            {
                if (impactSounds[i].clip != null)
                    validCount++;
            }

            if (validCount == 0) return false;

            int selectedIndex = Random.Range(0, validCount);
            for (int i = 0; i < impactSounds.Count; i++)
            {
                var data = impactSounds[i];
                if (data.clip == null) continue;

                if (selectedIndex == 0)
                {
                    chosen = data;
                    return true;
                }

                selectedIndex--;
            }

            return false;
        }

        private void PlayImpactClipAtPoint(SoundData sound, Vector3 point)
        {
            float volume = sound.volume > 0f ? sound.volume : 1f;
            if (audioSource == null)
            {
                AudioSource.PlayClipAtPoint(sound.clip, point, volume);
                return;
            }

            var temp = new GameObject("ProjectileImpactSound");
            temp.transform.position = point;

            var source = temp.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            source.spatialBlend = audioSource.spatialBlend;
            source.minDistance = audioSource.minDistance;
            source.maxDistance = audioSource.maxDistance;
            source.rolloffMode = audioSource.rolloffMode;
            source.pitch = audioSource.pitch;
            source.PlayOneShot(sound.clip, volume);

            float lifetime = sound.clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch));
            Destroy(temp, lifetime);
        }

        // ── Explosion ────────────────────────────────────

        private void Explode(Vector2 origin)
        {
            _detonated = true;

            var hits = Physics2D.OverlapCircleAll(origin, _explosionRadius);

            foreach (var hit in hits)
            {
                if (IsOwnerHierarchy(hit.transform)) continue;

                var damageable = hit.GetComponent<IDamageable>()
                                 ?? hit.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    float dist     = Vector2.Distance(origin, hit.bounds.center);
                    float falloff  = Mathf.Clamp01(1f - dist / _explosionRadius);
                    float finalDmg = Damage * falloff;

                    damageable.TakeDamage(finalDmg, _owner, origin);
                }
            }

            if (ProjectilePool.Instance != null && _explosionEffectPrefab != null)
            {
                var fx = ProjectilePool.Instance.GetExplosionFromPool(_explosionEffectPrefab, origin);
                if (fx != null) fx.Play(origin);
            }

            EventManager.Publish(new PlaySoundAtEvent
            {
                Id       = SoundId.ProjectileExplosion,
                Position = origin
            });
        }

        private bool IsOwnerHierarchy(Transform candidate)
        {
            if (_owner == null || candidate == null) return false;
            return candidate.IsChildOf(_owner.transform) || _owner.transform.IsChildOf(candidate);
        }

        // ── Return ───────────────────────────────────────

        private void ReturnToPool()
        {
            if (ProjectilePool.Instance != null)
                ProjectilePool.Instance.ReturnProjectile(this);
            else
                Destroy(gameObject);
        }
    }
}
