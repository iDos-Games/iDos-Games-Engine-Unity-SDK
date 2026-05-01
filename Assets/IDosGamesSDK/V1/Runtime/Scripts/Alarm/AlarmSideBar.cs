using UnityEngine;

namespace IDosGames
{
	public class AlarmSideBar : MonoBehaviour
	{
		[SerializeField] private GameObject _root;

		private void OnEnable()
		{
			if (AlarmSystem.Instance != null)
			{
				int alarmsCount = AlarmSystem.Instance.GetActiveAlarmsAmountOnSideBar();

				_root.SetActive(alarmsCount > 0);
			}
		}
	}
}