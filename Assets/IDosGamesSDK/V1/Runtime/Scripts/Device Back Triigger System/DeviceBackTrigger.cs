using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class DeviceBackTrigger : MonoBehaviour
	{
		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
		}

		private void OnEnable()
		{
			DeviceBackTriggerSystem.Stack.Push(_button);
		}

		private void OnDisable()
		{
			DeviceBackTriggerSystem.Stack.Pop();
		}
	}
}