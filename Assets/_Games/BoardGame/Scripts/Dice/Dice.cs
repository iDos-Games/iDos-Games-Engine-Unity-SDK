using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace IDosGames
{
    public class Dice : MonoBehaviour
    {
        [SerializeField] private bool _usePlaybackTime = false;
        [field: SerializeField, Min(0.001f)] public float PlaybackTime { get; private set; } = 1;
        [SerializeField] private bool hideGraphicObject = true;
        [SerializeField] private DiceManager _diceManager;

        [SerializeField] private List<Face> faces = new();
        public List<Face> Faces => faces;
        private IEnumerator _play;

        private bool _isClone;

        private Transform _graphicTransform;
        private DiceLocomotion _locomotion;
        private DiceLocomotion _simulationLocomotion;
        private GameObject _simulationObject;

        private readonly List<Pose> _trajectory = new();
        public List<Pose> Trajectory => _trajectory;

        public Collider SimulationCollider => _simulationLocomotion.Collider;
        public DiceLocomotion SimulationLocomotion => _simulationLocomotion;
        public UnityAction OnSimulationStationary = delegate { };

        public Pose GetPose() => _locomotion.GetPose();
        private RollData _rollData = RollData.Default;
        public RollData RollData => _rollData;

        public UnityEvent OnRollStart = new();
        public UnityEvent<int> OnRollEnd = new();

        [System.Serializable]
        public struct Face
        {
            public int faceValue;
            public Vector3 faceDirection;
            public Face(int faceValue, Vector3 faceDirection)
            {
                this.faceValue = faceValue;
                this.faceDirection = faceDirection;
            }
        }

        private void SetClone(out GameObject obj)
        {
            enabled = false;
            _isClone = true;
            obj = gameObject;
            foreach (Transform childs in transform)
            {
                if (childs.gameObject != gameObject) DestroyImmediate(childs.gameObject);
            }
            DestroyImmediate(this);
        }

        private void Start()
        {
            if (_isClone) return;
            CreateGfx();
            DestroyRenderComponents(gameObject);
            SetupLocomotion();
            SetupProjection();
        }

        private void OnDestroy()
        {
            if (_isClone) return;
            if (_diceManager) _diceManager.RemoveDice(this);
            DestroySimulation();
        }

        private void CreateGfx()
        {
            Instantiate(this, transform, true).SetClone(out var createdGfx);
            if (hideGraphicObject) createdGfx.hideFlags = HideFlags.HideInHierarchy;
            else createdGfx.hideFlags = HideFlags.NotEditable;

            DestroyNonRenderComponents(createdGfx.gameObject);
            _graphicTransform = createdGfx.transform;
        }

        private void SetupLocomotion()
        {
            _locomotion = gameObject.AddComponent<DiceLocomotion>();
        }

        private void SetupProjection()
        {
            var transformCache = transform;
            Instantiate(this, transformCache.position, transformCache.rotation).SetClone(out var createdSimulation);

            _simulationObject = createdSimulation;
            _simulationLocomotion = createdSimulation.GetComponent<DiceLocomotion>();

            Physics.IgnoreCollision(_locomotion.Collider, _simulationLocomotion.Collider);
            _diceManager.AddDice(this);
        }

        public void RollDiceWithOutCome(RollData data)
        {
            if (!enabled || !gameObject.activeInHierarchy) return;
            if (_play != null) StopCoroutine(_play);

            ResetSimulationDice();
            RollSimulation(data.force, data.torque);
            _rollData = data;
        }

        public void PlaySimulation()
        {
            if (!_locomotion || !enabled) return;
            if (_rollData.faceValue != RollData.RandomFace) ChangeOutcome(_rollData.faceValue);

            OnRollStart?.Invoke();

            if (_usePlaybackTime)
                _play = _locomotion.PlayInTime(Trajectory, () => OnRollEnd?.Invoke(_rollData.faceValue), PlaybackTime);
            else
                _play = _locomotion.Play(Trajectory, () => OnRollEnd?.Invoke(_rollData.faceValue));

            StartCoroutine(_play);
        }

        private void ChangeOutcome(int faceValue)
        {
            ResetGraphicRotation();
            Face outcome = GetFace(Outcome());
            if (outcome.faceValue == faceValue) return;
            var availableFaces = GetFaces(faceValue);
            Face targetFace = availableFaces[Random.Range(0, availableFaces.Length)];
            Vector3 changeVec = outcome.faceDirection;
            Vector3 targetDir = targetFace.faceDirection;
            Vector3 rotationAxis = CalculateRotationAxis(changeVec, targetDir);
            float rotationAngle = CalculateRotationAngle(changeVec, targetDir, rotationAxis);
            Quaternion targetRotation = Quaternion.AngleAxis(rotationAngle, -rotationAxis);
            ChangeGraphicRotation(targetRotation);
        }

        public void SetOutCome(int faceValue)
        {
            ResetGraphicRotation();

            Face outCome = GetFace(Outcome());
            if (outCome.faceValue == faceValue) return;

            Face targetFace = GetFace(faceValue);

            Vector3 changeVec = outCome.faceDirection;
            Vector3 targetDir = targetFace.faceDirection;

            Vector3 rotationAxis = Vector3.Cross(changeVec, targetDir).normalized;
            float rotationAngle = Mathf.Acos(Vector3.Dot(changeVec.normalized, targetDir.normalized)) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
            ChangeGraphicRotation(targetRotation);
        }

        private Vector3 CalculateRotationAxis(Vector3 changeVec, Vector3 targetDir)
        {
            Vector3 rotationAxis = Vector3.Cross(changeVec, targetDir);
            if (rotationAxis == Vector3.zero) rotationAxis = (changeVec == Vector3.up || changeVec == Vector3.down) ? Vector3.Cross(Vector3.right, changeVec) : Vector3.Cross(Vector3.up, changeVec);
            return rotationAxis;
        }

        private float CalculateRotationAngle(Vector3 changeVec, Vector3 targetDir, Vector3 rotationAxis)
        {
            float rotationAngle = Vector3.Angle(changeVec, targetDir);
            if (rotationAxis == Vector3.zero) rotationAngle = 180;
            return rotationAngle;
        }

        private static void DestroyNonRenderComponents(GameObject target)
        {
            foreach (Component component in target.GetComponents<Component>())
            {
                if (component is Transform) continue;
                if (!(component is MeshRenderer || component is MeshFilter)) DestroyImmediate(component);
            }
        }

        private static void DestroyRenderComponents(GameObject target)
        {
            foreach (Component component in target.GetComponents<Component>())
            {
                if (component is Transform) continue;
                if (component is MeshRenderer || component is MeshFilter) DestroyImmediate(component);
            }
        }
        public void AddFace(Face face)
        {
            faces.Add(face);
        }

        public Face GetFace(int faceValue)
        {
            return faces.FirstOrDefault(x => x.faceValue == faceValue);
        }
        public Face[] GetFaces(int faceValue)
        {
            return faces.Where(x => x.faceValue == faceValue).ToArray();
        }

        public int FaceLookingUp(Matrix4x4 matrix4X4)
        {
            var maxDot = -1f;
            var faceValue = RollData.RandomFace;
            for (int i = 0; i < faces.Count; i++)
            {
                var face = faces[i];
                var directionUp = matrix4X4.MultiplyVector(face.faceDirection);
                var dot = Vector3.Dot(Vector3.up, directionUp);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    faceValue = face.faceValue;
                }
            }

            return faceValue;
        }

        public Vector3 GetFaceDirection(int faceValue)
        {
            var face = faces.FirstOrDefault(x => x.faceValue == faceValue);
            return face.faceDirection;
        }

        public void RollSimulation(Vector3 force, Vector3 torque)
        {
            _simulationLocomotion.Roll(force, torque);
        }

        public void AddPoseToTrajectory()
        {
            _trajectory.Add(_simulationLocomotion.GetPose());
        }

        public bool IsStationaryOnStep()
        {
            return ApproxEqual(SimulationLocomotion.RB.angularVelocity, Vector3.zero) &&
                   ApproxEqual(SimulationLocomotion.RB.linearVelocity, Vector3.zero);
        }

        private bool ApproxEqual(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                   Mathf.Approximately(a.y, b.y) &&
                   Mathf.Approximately(a.z, b.z);
        }

        public void ResetSimulationDice()
        {
            _trajectory.Clear();
            _simulationLocomotion.ResetDice(GetPose());
        }

        public void DestroySimulation()
        {
            if (!_simulationObject) return;
            Destroy(_simulationObject);
        }

        public int Outcome()
        {
            return FaceLookingUp(_simulationObject.transform.localToWorldMatrix);
        }

        public void ResetGraphicRotation()
        {
            if (_graphicTransform == null) return;
            _graphicTransform.localRotation = Quaternion.identity;
        }

        private void ChangeGraphicRotation(Quaternion rotation)
        {
            if (_graphicTransform == null) return;
            _graphicTransform.localRotation = rotation;
        }
    }

    [System.Serializable]
    public struct RollData
    {
        public const int RandomFace = -1;
        public static RollData Default => new RollData
        {
            faceValue = RandomFace,
            force = UnityEngine.Vector3.zero,
            torque = UnityEngine.Vector3.zero
        };
        /// <summary>
        /// -1 for random face
        /// </summary>
        public int faceValue;
        public UnityEngine.Vector3 force;
        public UnityEngine.Vector3 torque;
    }
}
