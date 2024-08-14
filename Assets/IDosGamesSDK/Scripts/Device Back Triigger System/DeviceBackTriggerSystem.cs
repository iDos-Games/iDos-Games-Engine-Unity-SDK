using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class DeviceBackTriggerSystem : MonoBehaviour
	{
		private static Stack<Button> _stack = new();
		public static Stack<Button> Stack => _stack;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				_stack.Peek().onClick.Invoke();
			}
		}
	}
}