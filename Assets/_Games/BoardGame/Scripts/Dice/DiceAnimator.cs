using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace IDosGames
{
    public class DiceAnimator : MonoBehaviour
    {
        public static DiceAnimator Instance { get; private set; }

        [Header("Predicted Dice")]
        [SerializeField] private Dice dice1;
        [SerializeField] private Dice dice2;

        [Header("Player Token")]
        [SerializeField] private Transform playerToken;

        [Header("Throw Area")]
        [SerializeField] private Collider throwAreaCollider;
        [SerializeField] private float throwAreaSurfaceOffset = 0.02f;

        [Header("Spawn / Lanes")]
        [SerializeField] private float spawnHeight = 0.5f;
        [SerializeField] private float throwDistanceTowardPlayer = 0.75f;
        [SerializeField] private float laneHalfOffset = 0.2f;

        [Header("Impulse Settings")]
        [SerializeField] private float throwForce = 3.5f;
        [SerializeField] private float throwForceJitter = 0.45f;
        [SerializeField] private float upForce = 4.0f;
        [SerializeField] private float upForceJitter = 0.45f;
        [SerializeField] private float sideForceJitter = 0.2f;
        [SerializeField] private float randomTorque = 2f;
        [SerializeField] private float spinTorque = 1f;

        [Header("Timings")]
        [SerializeField] private float scaleInDuration = 0.15f;
        [SerializeField] private float scaleOutDuration = 0.2f;
        [SerializeField] private float showResultDuration = 1.0f;

        private Coroutine _activeRoutine;

        private Vector3 _dice1BaseScale = Vector3.one;
        private Vector3 _dice2BaseScale = Vector3.one;

        private UnityAction<int> _dice1EndListener;
        private UnityAction<int> _dice2EndListener;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            if (dice1 != null)
                _dice1BaseScale = dice1.transform.localScale;

            if (dice2 != null)
                _dice2BaseScale = dice2.transform.localScale;
        }

        private void Start()
        {
            ResetVisibleDiceState();
            SetDiceScale(1f);
        }

        private void OnDisable()
        {
            StopActiveRoutine();
            CleanupRollListeners();
        }

        public Coroutine PlayHideBeforeRoll(Action onComplete)
        {
            StopActiveRoutine();
            _activeRoutine = StartCoroutine(HideRoutine(onComplete));
            return _activeRoutine;
        }

        public Coroutine PlayDiceRoll(int totalSteps, Action onComplete)
        {
            var (value1, value2) = SplitIntoDice(totalSteps);
            StopActiveRoutine();
            _activeRoutine = StartCoroutine(ThrowRoutine(value1, value2, onComplete));
            return _activeRoutine;
        }

        public Coroutine PlayDiceRoll(int dice1Value, int dice2Value, Action onComplete)
        {
            dice1Value = Mathf.Clamp(dice1Value, 1, 6);
            dice2Value = Mathf.Clamp(dice2Value, 1, 6);

            StopActiveRoutine();
            _activeRoutine = StartCoroutine(ThrowRoutine(dice1Value, dice2Value, onComplete));
            return _activeRoutine;
        }

        private void StopActiveRoutine()
        {
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }

            CleanupRollListeners();
        }

        private void CleanupRollListeners()
        {
            if (dice1 != null && _dice1EndListener != null)
            {
                dice1.OnRollEnd.RemoveListener(_dice1EndListener);
                _dice1EndListener = null;
            }

            if (dice2 != null && _dice2EndListener != null)
            {
                dice2.OnRollEnd.RemoveListener(_dice2EndListener);
                _dice2EndListener = null;
            }
        }

        private IEnumerator HideRoutine(Action onComplete)
        {
            yield return ScaleBoth(1f, 0f, scaleOutDuration);

            ResetVisibleDiceState();

            _activeRoutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator ThrowRoutine(int value1, int value2, Action onComplete)
        {
            if (dice1 == null || dice2 == null)
            {
                Debug.LogError("DiceAnimator: dice1 or dice2 is not assigned.");
                _activeRoutine = null;
                onComplete?.Invoke();
                yield break;
            }

            if (DiceManager.Instance == null)
            {
                Debug.LogError("DiceAnimator: ProjectionSceneManager.Instance is null.");
                _activeRoutine = null;
                onComplete?.Invoke();
                yield break;
            }

            Vector3 targetCenter = GetThrowAreaCenterPoint();
            Vector3 areaUp = GetThrowAreaUpVector();

            Vector3 throwDir = GetThrowDirectionTowardPlayer(targetCenter, areaUp);

            Vector3 laneRight = Vector3.Cross(areaUp, throwDir);
            if (laneRight.sqrMagnitude < 0.0001f)
                laneRight = GetThrowAreaRightVector();
            else
                laneRight.Normalize();

            Vector3 spawnBase = targetCenter + areaUp * spawnHeight;
            Vector3 spawn1 = spawnBase - laneRight * laneHalfOffset;
            Vector3 spawn2 = spawnBase + laneRight * laneHalfOffset;

            Vector3 targetBase = targetCenter + throwDir * throwDistanceTowardPlayer;
            Vector3 target1 = targetBase - laneRight * laneHalfOffset;
            Vector3 target2 = targetBase + laneRight * laneHalfOffset;

            PrepareDieForThrow(dice1, spawn1);
            PrepareDieForThrow(dice2, spawn2);

            yield return ScaleBoth(0f, 1f, scaleInDuration);

            RollData roll1 = CreateRollData(value1, spawn1, target1, areaUp);
            RollData roll2 = CreateRollData(value2, spawn2, target2, areaUp);

            dice1.RollDiceWithOutCome(roll1);
            dice2.RollDiceWithOutCome(roll2);

            DiceManager.Instance.Simulate();

            bool dice1Finished = false;
            bool dice2Finished = false;

            CleanupRollListeners();

            _dice1EndListener = _ => { dice1Finished = true; };
            _dice2EndListener = _ => { dice2Finished = true; };

            dice1.OnRollEnd.AddListener(_dice1EndListener);
            dice2.OnRollEnd.AddListener(_dice2EndListener);

            dice1.PlaySimulation();
            dice2.PlaySimulation();

            yield return new WaitUntil(() => dice1Finished && dice2Finished);

            CleanupRollListeners();

            if (showResultDuration > 0f) yield return new WaitForSeconds(showResultDuration);

            _activeRoutine = null;
            onComplete?.Invoke();
        }

        private void PrepareDieForThrow(Dice dice, Vector3 spawnPosition)
        {
            if (dice == null) return;

            Rigidbody rb = dice.GetComponent<Rigidbody>();
            SetBodyStoppedKinematic(rb);

            dice.transform.position = spawnPosition;
            dice.transform.rotation = Random.rotationUniform;

            dice.ResetGraphicRotation();
        }

        private void ResetVisibleDiceState()
        {
            ResetSingleDieState(dice1);
            ResetSingleDieState(dice2);
        }

        private void ResetSingleDieState(Dice dice)
        {
            if (dice == null) return;

            Rigidbody rb = dice.GetComponent<Rigidbody>();
            SetBodyStoppedKinematic(rb);

            dice.ResetGraphicRotation();
        }

        private void SetBodyStoppedKinematic(Rigidbody rb)
        {
            if (rb == null) return;

            bool wasKinematic = rb.isKinematic;

            if (wasKinematic) rb.isKinematic = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
        }

        private RollData CreateRollData(int outcome, Vector3 spawnPosition, Vector3 targetPosition, Vector3 areaUp)
        {
            Vector3 dir = (targetPosition - spawnPosition).normalized;
            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector3.forward;

            Vector3 right = Vector3.Cross(areaUp, dir);
            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.right;
            else
                right.Normalize();

            float launchForward = throwForce + Random.Range(-throwForceJitter, throwForceJitter);
            float launchUp = upForce + Random.Range(-upForceJitter, upForceJitter);
            float launchSide = Random.Range(-sideForceJitter, sideForceJitter);

            Vector3 force =
                dir * launchForward +
                areaUp * launchUp +
                right * launchSide;

            Vector3 torque =
                Random.onUnitSphere * randomTorque +
                dir * spinTorque +
                areaUp * Random.Range(-spinTorque, spinTorque);

            return new RollData
            {
                faceValue = Mathf.Clamp(outcome, 1, 6),
                force = force,
                torque = torque
            };
        }

        private Vector3 GetThrowDirectionTowardPlayer(Vector3 centerPoint, Vector3 areaUp)
        {
            if (playerToken != null)
            {
                Vector3 toPlayer = playerToken.position - centerPoint;
                Vector3 planarToPlayer = Vector3.ProjectOnPlane(toPlayer, areaUp);

                if (planarToPlayer.sqrMagnitude > 0.0001f)
                    return planarToPlayer.normalized;
            }

            if (throwAreaCollider != null)
            {
                Vector3 fallback = Vector3.ProjectOnPlane(throwAreaCollider.transform.forward, areaUp);
                if (fallback.sqrMagnitude > 0.0001f)
                    return fallback.normalized;
            }

            return Vector3.forward;
        }

        private void SetDiceScale(float factor)
        {
            if (dice1 != null)
                dice1.transform.localScale = _dice1BaseScale * factor;

            if (dice2 != null)
                dice2.transform.localScale = _dice2BaseScale * factor;
        }

        private IEnumerator ScaleBoth(float from, float to, float duration)
        {
            if (duration <= 0f)
            {
                SetDiceScale(to);
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                float current = Mathf.Lerp(from, to, t);
                SetDiceScale(current);
                yield return null;
            }

            SetDiceScale(to);
        }

        private (int, int) SplitIntoDice(int total)
        {
            if (total <= 1)
                return (1, 1);

            if (total > 12)
                total = 12;

            int minFirst = Mathf.Max(1, total - 6);
            int maxFirst = Mathf.Min(6, total - 1);

            int value1 = Random.Range(minFirst, maxFirst + 1);
            int value2 = total - value1;

            return (value1, value2);
        }

        private Vector3 GetThrowAreaCenterPoint()
        {
            if (throwAreaCollider == null)
                return playerToken != null ? playerToken.position : Vector3.zero;

            if (throwAreaCollider is BoxCollider box)
            {
                Transform t = box.transform;
                Vector3 half = box.size * 0.5f;

                Vector3 localTopCenter = new Vector3(
                    box.center.x,
                    box.center.y + half.y,
                    box.center.z
                );

                Vector3 worldPoint = t.TransformPoint(localTopCenter);
                worldPoint += t.up * throwAreaSurfaceOffset;
                return worldPoint;
            }

            Bounds b = throwAreaCollider.bounds;
            return new Vector3(
                b.center.x,
                b.max.y + throwAreaSurfaceOffset,
                b.center.z
            );
        }

        private Vector3 GetThrowAreaUpVector()
        {
            return throwAreaCollider != null ? throwAreaCollider.transform.up : Vector3.up;
        }

        private Vector3 GetThrowAreaRightVector()
        {
            return throwAreaCollider != null ? throwAreaCollider.transform.right : Vector3.right;
        }
    }
}
