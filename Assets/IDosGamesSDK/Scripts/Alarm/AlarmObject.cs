using UnityEngine;

namespace IDosGames
{
	public class AlarmObject : MonoBehaviour
	{
		[SerializeField] private AlarmType _alarmType;
		[SerializeField] private GameObject _root;

		public AlarmType AlarmType => _alarmType;

		private void OnEnable()
		{
			if (AlarmSystem.Instance != null)
			{
				SetActivateRoot(AlarmSystem.Instance.GetAlarmState(_alarmType));
			}
		}

		private void Start()
		{
			AlarmSystem.Instance.AddAlarmObject(this);
		}

		public void SetActivateRoot(bool active)
		{
			_root.SetActive(active);
		}
	}
}