using System.Collections;
using UnityEngine;

namespace IDosGames
{
    public class HammerSpriteHit : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private Transform hammerVisual;

        [SerializeField] private float startDelay = 0f;

        [Header("Angles")]
        [SerializeField] private float idleAngle = -35f;
        [SerializeField] private float hitAngle = 20f;
        [SerializeField] private float reboundAngle = -10f;

        [Header("Timing")]
        [SerializeField] private float downDuration = 0.06f;
        [SerializeField] private float reboundDuration = 0.05f;
        [SerializeField] private float returnDuration = 0.08f;
        [SerializeField] private float pauseBetweenHits = 0.04f;

        [Header("Visual Juice")]
        [SerializeField] private float hitScale = 0.92f;
        [SerializeField] private float shakeDistance = 0.04f;

        private Vector3 _startLocalPos;
        private Vector3 _startLocalScale;
        private Coroutine _loopRoutine;

        private void Awake()
        {
            if (hammerVisual == null && transform.childCount > 0)
                hammerVisual = transform.GetChild(0);

            if (hammerVisual == null)
            {
                Debug.LogError("HammerSpriteHit: hammerVisual is not assigned.");
                enabled = false;
                return;
            }

            _startLocalPos = hammerVisual.localPosition;
            _startLocalScale = hammerVisual.localScale;

            ResetState();
        }

        private void OnEnable()
        {
            if (hammerVisual == null)
                return;

            ResetState();

            if (_loopRoutine != null)
                StopCoroutine(_loopRoutine);

            _loopRoutine = StartCoroutine(HammerLoop());
        }

        private void OnDisable()
        {
            if (_loopRoutine != null)
            {
                StopCoroutine(_loopRoutine);
                _loopRoutine = null;
            }

            if (hammerVisual != null)
                ResetState();
        }

        private IEnumerator HammerLoop()
        {
            if (startDelay > 0f)
                yield return new WaitForSeconds(startDelay);

            while (true)
            {
                // Удар
                yield return AnimateHammer(
                    fromAngle: idleAngle,
                    toAngle: hitAngle,
                    duration: downDuration,
                    targetLocalPos: _startLocalPos + Vector3.down * shakeDistance,
                    targetLocalScale: _startLocalScale * hitScale
                );

                // Отскок
                yield return AnimateHammer(
                    fromAngle: hitAngle,
                    toAngle: reboundAngle,
                    duration: reboundDuration,
                    targetLocalPos: _startLocalPos,
                    targetLocalScale: _startLocalScale
                );

                // Возврат в замах
                yield return AnimateHammer(
                    fromAngle: reboundAngle,
                    toAngle: idleAngle,
                    duration: returnDuration,
                    targetLocalPos: _startLocalPos,
                    targetLocalScale: _startLocalScale
                );

                if (pauseBetweenHits > 0f)
                    yield return new WaitForSeconds(pauseBetweenHits);
            }
        }

        private IEnumerator AnimateHammer(
            float fromAngle,
            float toAngle,
            float duration,
            Vector3 targetLocalPos,
            Vector3 targetLocalScale)
        {
            float time = 0f;
            Vector3 startPos = hammerVisual.localPosition;
            Vector3 startScale = hammerVisual.localScale;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float eased = EaseOutCubic(t);

                float angle = Mathf.Lerp(fromAngle, toAngle, eased);
                transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                hammerVisual.localPosition = Vector3.Lerp(startPos, targetLocalPos, eased);
                hammerVisual.localScale = Vector3.Lerp(startScale, targetLocalScale, eased);

                yield return null;
            }

            transform.localRotation = Quaternion.Euler(0f, 0f, toAngle);
            hammerVisual.localPosition = targetLocalPos;
            hammerVisual.localScale = targetLocalScale;
        }

        private void ResetState()
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, idleAngle);
            hammerVisual.localPosition = _startLocalPos;
            hammerVisual.localScale = _startLocalScale;
        }

        private float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}
