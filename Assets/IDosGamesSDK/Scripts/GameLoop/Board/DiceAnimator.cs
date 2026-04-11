using System;
using System.Collections;
using UnityEngine;

namespace IDosGames
{
    public class DiceAnimator : MonoBehaviour
    {
        public static DiceAnimator Instance { get; private set; }

        [Header("Dices")]
        [SerializeField] private Transform dice1;
        [SerializeField] private Transform dice2;

        [Header("Player Token")]
        [SerializeField] private Transform playerToken;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnHeight = 1.5f;
        [SerializeField] private Vector3 spawnDirectionOffset = new Vector3(-1.5f, 0f, -1.5f);
        [SerializeField] private float landOffset = 0.35f;
        [SerializeField] private float landRadius = 0.2f;

        [Header("Physics Launch")]
        [SerializeField] private float throwForce = 3.5f;
        [SerializeField] private float throwForceJitter = 0.45f;
        [SerializeField] private float upForce = 4.0f;
        [SerializeField] private float upForceJitter = 0.45f;
        [SerializeField] private float sideForceJitter = 0.2f;
        [SerializeField] private float randomTorque = 8f;
        [SerializeField] private float spinTorque = 4f;

        [Header("Loaded Dice Settings (AAA Трюк)")]
        [Tooltip("Насколько сильно смещать центр масс (от 0 до 0.45).")]
        [SerializeField, Range(0f, 1f)] private float centerOfMassBiasNormalized = 0.4f;
        [Tooltip("Если true, кубик плавно потеряет смещение центра масс в конце падения, чтобы не качаться как неваляшка.")]
        [SerializeField] private bool antiWobbleEnabled = true;

        [Header("Settle")]
        [SerializeField] private float settleTimeout = 3.5f;
        [SerializeField] private float initialFlightTime = 0.45f;
        [SerializeField] private float settleLinearVelocityThreshold = 0.05f;
        [SerializeField] private float settleAngularVelocityThreshold = 0.2f;

        [Header("Timings")]
        [SerializeField] private float scaleInDuration = 0.15f;
        [SerializeField] private float scaleOutDuration = 0.2f;
        [SerializeField] private float showResultDuration = 1.0f;

        [Header("Dice Face Settings")]
        [SerializeField]
        private Vector3[] faceRotations = new Vector3[6]
        {
            new Vector3(-90, 0, 0),  // 1
            new Vector3(0, 0, 0),   // 2
            new Vector3(0, 0, -90), // 3
            new Vector3(0, 0, 90),  // 4
            new Vector3(180, 0, 0), // 5
            new Vector3(90, 0, 0),  // 6
        };

        private Rigidbody _rb1;
        private Rigidbody _rb2;
        private float _defaultAngularDrag = 0.05f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            _rb1 = dice1 != null ? dice1.GetComponent<Rigidbody>() : null;
            _rb2 = dice2 != null ? dice2.GetComponent<Rigidbody>() : null;

            if (_rb1) _defaultAngularDrag = _rb1.angularDamping;

            ConfigureRigidbody(_rb1);
            ConfigureRigidbody(_rb2);

            SetKinematic(true);
            ResetDiePhysicsState(_rb1);
            ResetDiePhysicsState(_rb2);
            SetDiceScale(Vector3.one);
        }

        public Coroutine PlayHideBeforeRoll(Action onComplete)
        {
            return StartCoroutine(HideRoutine(onComplete));
        }

        public Coroutine PlayDiceRoll(int totalSteps, Action onComplete)
        {
            var (val1, val2) = SplitIntoDice(totalSteps);
            return StartCoroutine(ThrowRoutine(val1, val2, onComplete));
        }

        public Coroutine PlayDiceRoll(int dice1Value, int dice2Value, Action onComplete)
        {
            dice1Value = Mathf.Clamp(dice1Value, 1, 6);
            dice2Value = Mathf.Clamp(dice2Value, 1, 6);
            return StartCoroutine(ThrowRoutine(dice1Value, dice2Value, onComplete));
        }

        private IEnumerator HideRoutine(Action onComplete)
        {
            ResetDiePhysicsState(_rb1);
            ResetDiePhysicsState(_rb2);
            SetKinematic(true);

            yield return ScaleBoth(Vector3.one, Vector3.zero, scaleOutDuration);
            onComplete?.Invoke();
        }

        private IEnumerator ThrowRoutine(int value1, int value2, Action onComplete)
        {
            Vector3 tokenPos = playerToken != null ? playerToken.position : Vector3.zero;

            Vector3 land1 = tokenPos + new Vector3(
                landOffset + UnityEngine.Random.Range(-landRadius, landRadius),
                0f,
                UnityEngine.Random.Range(-landRadius, landRadius));

            Vector3 land2 = tokenPos + new Vector3(
                -landOffset + UnityEngine.Random.Range(-landRadius, landRadius),
                0f,
                UnityEngine.Random.Range(-landRadius, landRadius));

            Vector3 spawn1 = land1 + spawnDirectionOffset + Vector3.up * spawnHeight;
            Vector3 spawn2 = land2 + spawnDirectionOffset + Vector3.up * spawnHeight;

            // Подготавливаем кубики (центр масс смещается здесь)
            PrepareDieForThrow(dice1, _rb1, spawn1, value1);
            PrepareDieForThrow(dice2, _rb2, spawn2, value2);

            yield return ScaleBoth(Vector3.zero, Vector3.one, scaleInDuration);

            SetKinematic(false);

            LaunchDice(_rb1, dice1, land1);
            LaunchDice(_rb2, dice2, land2);

            // Запускаем отслеживание и стабилизацию для естественного приземления
            if (antiWobbleEnabled)
            {
                StartCoroutine(StabilizeRollRoutine(_rb1, dice1, value1));
                StartCoroutine(StabilizeRollRoutine(_rb2, dice2, value2));
            }

            // Ждем успокоения физики
            yield return WaitForSettle();

            // Плавная доводка, если кубик встал на ребро
            StartCoroutine(SmoothFinalSnap(_rb1, dice1, value1));
            StartCoroutine(SmoothFinalSnap(_rb2, dice2, value2));

            yield return new WaitForSeconds(0.3f + showResultDuration);

            onComplete?.Invoke();
        }

        private void ConfigureRigidbody(Rigidbody rb)
        {
            if (rb == null) return;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.maxAngularVelocity = 60f;
        }

        private void PrepareDieForThrow(Transform dice, Rigidbody rb, Vector3 spawnPosition, int targetValue)
        {
            if (dice == null) return;

            if (rb != null)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                rb.ResetCenterOfMass();
                rb.angularDamping = _defaultAngularDrag; // Сброс сопротивления вращению

                // Делаем кубик "свинцовым" со старта в нужной точке
                rb.centerOfMass = GetLoadedCenterOfMass(dice, targetValue);
                rb.isKinematic = true;
            }

            dice.position = spawnPosition;
            dice.rotation = UnityEngine.Random.rotationUniform;
            dice.localScale = Vector3.zero;
        }

        private void LaunchDice(Rigidbody rb, Transform dice, Vector3 target)
        {
            if (rb == null || dice == null) return;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 dir = (target - dice.position).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

            float launchForward = throwForce + UnityEngine.Random.Range(-throwForceJitter, throwForceJitter);
            float launchUp = upForce + UnityEngine.Random.Range(-upForceJitter, upForceJitter);
            float launchSide = UnityEngine.Random.Range(-sideForceJitter, sideForceJitter);

            Vector3 force = dir * launchForward + Vector3.up * launchUp + right * launchSide;

            Vector3 torque =
                UnityEngine.Random.onUnitSphere * randomTorque +
                dir * spinTorque +
                Vector3.up * UnityEngine.Random.Range(-spinTorque, spinTorque);

            rb.AddForce(force, ForceMode.VelocityChange);
            rb.AddTorque(torque, ForceMode.VelocityChange);
        }

        /// <summary>
        /// Ключевая функция для естественности (Анти-неваляшка).
        /// Наблюдает за летящим кубиком. Как только он почти остановился и смотрит нужной гранью вверх,
        /// плавно возвращает центр масс в норму, чтобы кубик перестал раскачиваться.
        /// </summary>
        private IEnumerator StabilizeRollRoutine(Rigidbody rb, Transform dice, int targetValue)
        {
            float elapsed = 0f;
            Vector3 desiredLocalUp = GetFaceLocalUpNormal(targetValue);

            while (elapsed < settleTimeout)
            {
                if (rb == null || rb.isKinematic) yield break;

                // Проверяем: кубик замедлился после отскоков?
                if (rb.linearVelocity.magnitude < 2.0f && rb.angularVelocity.magnitude < 5.0f)
                {
                    Vector3 currentWorldUp = dice.TransformDirection(desiredLocalUp);
                    float angleError = Vector3.Angle(currentWorldUp, Vector3.up);

                    // Если нужная грань смотрит примерно вверх (погрешность до 45 градусов)
                    if (angleError < 45f)
                    {
                        // Плавно возвращаем центр масс в геометрический центр (0,0,0)
                        rb.centerOfMass = Vector3.Lerp(rb.centerOfMass, Vector3.zero, Time.fixedDeltaTime * 10f);

                        // Слегка увеличиваем трение вращения, чтобы кубик "шлепнулся", а не качался
                        rb.angularDamping = Mathf.Lerp(rb.angularDamping, 2.5f, Time.fixedDeltaTime * 8f);
                    }
                }

                if (IsSettled(rb)) yield break;

                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }

        private IEnumerator WaitForSettle()
        {
            float elapsed = 0f;
            yield return new WaitForSeconds(initialFlightTime);
            elapsed += initialFlightTime;

            while (elapsed < settleTimeout)
            {
                if (IsSettled(_rb1) && IsSettled(_rb2))
                    yield break;

                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
            }
        }

        private bool IsSettled(Rigidbody rb)
        {
            if (rb == null) return true;

            return rb.IsSleeping() ||
                   (rb.linearVelocity.magnitude < settleLinearVelocityThreshold &&
                    rb.angularVelocity.magnitude < settleAngularVelocityThreshold);
        }

        private Vector3 GetLoadedCenterOfMass(Transform dice, int targetValue)
        {
            float halfExtent = GetApproxLocalHalfExtent(dice);
            float biasDistance = halfExtent * centerOfMassBiasNormalized;

            Vector3 desiredTopLocal = GetFaceLocalUpNormal(targetValue);
            return -desiredTopLocal * biasDistance;
        }

        private float GetApproxLocalHalfExtent(Transform dice)
        {
            if (dice == null) return 0.5f;

            BoxCollider box = dice.GetComponent<BoxCollider>();
            if (box != null)
            {
                Vector3 scaledSize = Vector3.Scale(box.size, dice.localScale);
                return Mathf.Max(Mathf.Abs(scaledSize.x), Mathf.Abs(scaledSize.y), Mathf.Abs(scaledSize.z)) * 0.5f;
            }

            MeshFilter meshFilter = dice.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                Vector3 scaledSize = Vector3.Scale(meshFilter.sharedMesh.bounds.size, dice.localScale);
                return Mathf.Max(Mathf.Abs(scaledSize.x), Mathf.Abs(scaledSize.y), Mathf.Abs(scaledSize.z)) * 0.5f;
            }

            return Mathf.Max(Mathf.Abs(dice.localScale.x), Mathf.Abs(dice.localScale.y), Mathf.Abs(dice.localScale.z)) * 0.5f;
        }

        private Vector3 GetFaceLocalUpNormal(int faceValue)
        {
            faceValue = Mathf.Clamp(faceValue, 1, 6);
            Quaternion finalRotation = Quaternion.Euler(faceRotations[faceValue - 1]);
            return (Quaternion.Inverse(finalRotation) * Vector3.up).normalized;
        }

        private IEnumerator SmoothFinalSnap(Rigidbody rb, Transform dice, int targetValue)
        {
            if (rb == null || dice == null) yield break;

            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.ResetCenterOfMass();
            rb.isKinematic = true;

            Vector3 desiredLocalUp = GetFaceLocalUpNormal(targetValue);
            Vector3 currentWorldUp = dice.TransformDirection(desiredLocalUp);
            float angleError = Vector3.Angle(currentWorldUp, Vector3.up);

            if (angleError < 5f) yield break;

            Vector3 localRef = Mathf.Abs(Vector3.Dot(desiredLocalUp, Vector3.forward)) < 0.95f
                ? Vector3.Cross(desiredLocalUp, Vector3.forward).normalized
                : Vector3.Cross(desiredLocalUp, Vector3.right).normalized;

            Vector3 worldRef = Vector3.ProjectOnPlane(dice.rotation * localRef, Vector3.up).normalized;
            if (worldRef.sqrMagnitude < 0.0001f) worldRef = Vector3.forward;

            Quaternion localBasis = Quaternion.LookRotation(localRef, desiredLocalUp);
            Quaternion worldBasis = Quaternion.LookRotation(worldRef, Vector3.up);

            Quaternion startRotation = dice.rotation;
            Quaternion targetRotation = worldBasis * Quaternion.Inverse(localBasis);

            float duration = angleError > 20f ? 0.25f : 0.1f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                dice.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }

            dice.rotation = targetRotation;
        }

        private void ResetDiePhysicsState(Rigidbody rb)
        {
            if (rb != null)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.ResetCenterOfMass();
                rb.angularDamping = _defaultAngularDrag; // Обязательно сбрасываем Drag
            }
        }

        private void SetKinematic(bool kinematic)
        {
            if (_rb1 != null) _rb1.isKinematic = kinematic;
            if (_rb2 != null) _rb2.isKinematic = kinematic;
        }

        private void SetDiceScale(Vector3 scale)
        {
            if (dice1) dice1.localScale = scale;
            if (dice2) dice2.localScale = scale;
        }

        private IEnumerator ScaleBoth(Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, duration)));

                if (dice1) dice1.localScale = Vector3.Lerp(from, to, t);
                if (dice2) dice2.localScale = Vector3.Lerp(from, to, t);

                yield return null;
            }

            if (dice1) dice1.localScale = to;
            if (dice2) dice2.localScale = to;
        }

        private (int, int) SplitIntoDice(int total)
        {
            if (total <= 1) return (1, 1);
            if (total > 12) total = 12;

            int minFirst = Mathf.Max(1, total - 6);
            int maxFirst = Mathf.Min(6, total - 1);

            int val1 = UnityEngine.Random.Range(minFirst, maxFirst + 1);
            int val2 = total - val1;

            return (val1, val2);
        }
    }
}
