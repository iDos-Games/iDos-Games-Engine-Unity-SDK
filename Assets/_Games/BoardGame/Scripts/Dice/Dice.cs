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
        private GameObject _simulationObject;

        private readonly List<Pose> _trajectory = new();
        public List<Pose> Trajectory => _trajectory;

        public UnityAction OnSimulationStationary = delegate { };

        private RollData _rollData = RollData.Default;
        public RollData RollData => _rollData;

        public UnityEvent OnRollStart = new();
        public UnityEvent<int> OnRollEnd = new();

        private Dice _simulationDice;
        public Collider SimulationCollider => _simulationDice.Collider;
        public Dice SimulationDice => _simulationDice;
        private Rigidbody _rb;
        public Rigidbody RB => _rb;

        private Collider _collider;
        public Collider Collider
        {
            get
            {
                if (_collider == null)
                    _collider = GetComponent<Collider>();

                return _collider;
            }
        }

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

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
        }

        private void Start()
        {
            if (_isClone) return;
            CreateGfx();
            DestroyRenderComponents(gameObject);
            SetupProjection();
        }

        private void OnDestroy()
        {
            if (_isClone) return;
            if (_diceManager) _diceManager.RemoveDice(this);
            DestroySimulation();
        }

        private void SetClone(out GameObject obj, bool destroyDiceComponent)
        {
            enabled = false;
            _isClone = true;
            obj = gameObject;

            foreach (Transform childs in transform)
            {
                if (childs.gameObject != gameObject) DestroyImmediate(childs.gameObject);
            }

            if (destroyDiceComponent) DestroyImmediate(this);
        }

        private void CreateGfx()
        {
            Instantiate(this, transform, true).SetClone(out var createdGfx, true);
            if (hideGraphicObject) createdGfx.hideFlags = HideFlags.HideInHierarchy;
            else createdGfx.hideFlags = HideFlags.NotEditable;

            DestroyNonRenderComponents(createdGfx.gameObject);
            _graphicTransform = createdGfx.transform;
        }

        private void SetupProjection()
        {
            var transformCache = transform;
            Instantiate(this, transformCache.position, transformCache.rotation)
                .SetClone(out var createdSimulation, false);

            _simulationObject = createdSimulation;
            _simulationDice = createdSimulation.GetComponent<Dice>();

            Physics.IgnoreCollision(Collider, _simulationDice.Collider);
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
            if (!enabled) return;
            if (_rollData.faceValue != RollData.RandomFace) ChangeOutcome(_rollData.faceValue);

            OnRollStart?.Invoke();

            if (_usePlaybackTime) _play = PlayInTime(Trajectory, () => OnRollEnd?.Invoke(_rollData.faceValue), PlaybackTime);
            else _play = Play(Trajectory, () => OnRollEnd?.Invoke(_rollData.faceValue));

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
            _simulationDice.Roll(force, torque);
        }

        public void AddPoseToTrajectory()
        {
            _trajectory.Add(_simulationDice.GetPose());
        }

        public bool IsStationaryOnStep()
        {
            return ApproxEqual(_simulationDice.RB.angularVelocity, Vector3.zero) && ApproxEqual(_simulationDice.RB.linearVelocity, Vector3.zero);
        }

        public void ResetSimulationDice()
        {
            _trajectory.Clear();
            _simulationDice.ResetDice(GetPose());
        }

        private bool ApproxEqual(Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) &&
                   Mathf.Approximately(a.y, b.y) &&
                   Mathf.Approximately(a.z, b.z);
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

        public IEnumerator Play(List<Pose> projectory, UnityAction onComplete)
        {
            Pose[] poses = new Pose[projectory.Count];
            projectory.CopyTo(poses);
            var wait = new WaitForFixedUpdate();

            foreach (var pose in poses)
            {
                _rb.MovePosition(pose.position);
                _rb.MoveRotation(pose.rotation);
                yield return wait;
            }

            onComplete?.Invoke();
        }

        public IEnumerator PlayInTime(List<Pose> projectory, UnityAction onComplete, float completeTime = 1f)
        {
            Pose[] poses = new Pose[projectory.Count];
            projectory.CopyTo(poses);

            int currentPoint = 0;
            int targetPoint = poses.Length - 1;

            float elapsedTime = 0f;
            while (elapsedTime < completeTime)
            {
                float t = elapsedTime / completeTime;
                currentPoint = Mathf.Min((int)(t * targetPoint), targetPoint);

                var pose = poses[currentPoint];
                _rb.MovePosition(pose.position);
                _rb.MoveRotation(pose.rotation);

                elapsedTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            onComplete?.Invoke();
        }

        public void ResetDice(Pose pose)
        {
            _rb.isKinematic = true;
            _rb.position = pose.position;
            _rb.rotation = pose.rotation;
        }

        public void Roll(Vector3 force, Vector3 torque)
        {
            _rb.isKinematic = false;
            _rb.AddForce(force, ForceMode.Impulse);
            _rb.AddTorque(torque, ForceMode.Impulse);
        }

        public Pose GetPose() => new(_rb.position, _rb.rotation);
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
