using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace IDosGames
{
    public class DiceManager : MonoBehaviour
    {
        public static DiceManager Instance { get; private set; }
        [SerializeField] private GameObject[] _simulationObjects;
        private readonly Dictionary<int, GameObject> _addedCollisionObjects = new();
        private readonly float _simulationSpeed = 1;
        private const int maxIterations = 300;
        private const string simulationSceneName = "SimulationScene";
        private Dictionary<GameObject, Scene> _previousSceneOfObject = new();
        private List<Dice> _diceList = new();
        private bool _isSimulating;
        private Scene _simulationScene;
        private PhysicsScene _simulationPhysicsScene;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadScene();
                AddToScene(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetSimulationObjects(GameObject[] collisionObjects)
        {
            _simulationObjects = collisionObjects;
        }

        private void LoadScene()
        {
            if (_simulationScene.IsValid()) return;
            CreatePhysicsScene();
        }

        private void CreatePhysicsScene()
        {
            _simulationScene = SceneManager.CreateScene(simulationSceneName, new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            _simulationPhysicsScene = _simulationScene.GetPhysicsScene();
            AddScenePhysicsObjects(_simulationObjects);
        }

        public void AddDice(Dice dice)
        {
            if (_diceList.Contains(dice)) return;
            _diceList.Add(dice);
            SceneManager.MoveGameObjectToScene(dice.SimulationDice.gameObject, _simulationScene);
        }

        public void RemoveDice(Dice dice)
        {
            if (!_diceList.Contains(dice)) return;
            _diceList.Remove(dice);
        }

        public void UnloadScene(UnityAction callback = null)
        {
            PutToOriginalScene(gameObject);
            foreach (var addedCollisionObject in _addedCollisionObjects) PutToOriginalScene(addedCollisionObject.Value);
            var task = SceneManager.UnloadSceneAsync(_simulationScene);
            task.completed += operation => callback?.Invoke();
        }

        public void Simulate()
        {
            if (_isSimulating) return;
            _isSimulating = true;
            const int maxKinematicSteps = 4;
            Dictionary<Dice, int> kinematicIterations = new();
            for (int i = 0; i < maxIterations; i++)
            {
                foreach (var dice in _diceList)
                {
                    if (dice.IsStationaryOnStep())
                    {
                        if (kinematicIterations.ContainsKey(dice) && kinematicIterations[dice] > maxKinematicSteps) continue;
                        if (!kinematicIterations.TryAdd(dice, 1))
                        {
                            kinematicIterations[dice]++;
                            if (kinematicIterations[dice] > maxKinematicSteps) dice.OnSimulationStationary?.Invoke();
                        }
                    }
                    dice.AddPoseToTrajectory();
                }
                _simulationPhysicsScene.Simulate(Time.fixedDeltaTime * _simulationSpeed);
            }
            _isSimulating = false;
        }

        private void AddScenePhysicsObjects(GameObject[] targets)
        {
            if (targets == null) return;
            foreach (GameObject target in targets)
            {
                if (target.transform.parent != null)
                {
                    if (target.transform.parent == transform) continue;
                    continue;
                }
                AddToScene(target);
            }
        }

        private void AddToScene(GameObject target)
        {
            if (_addedCollisionObjects.ContainsKey(target.GetHashCode())) return;
            _previousSceneOfObject.Add(target, target.scene);
            _addedCollisionObjects.Add(target.GetHashCode(), target);
            SceneManager.MoveGameObjectToScene(target, _simulationScene);
        }

        private void PutToOriginalScene(GameObject target)
        {
            if (!_addedCollisionObjects.ContainsKey(target.GetHashCode())) return;
            SceneManager.MoveGameObjectToScene(target, _previousSceneOfObject[target]);
            _addedCollisionObjects.Remove(target.GetHashCode());
            _previousSceneOfObject.Remove(target);
        }
    }
}
