using UnityEngine;

namespace IDosGames
{
	public class TouchBlocker : MonoBehaviour
	{
		[SerializeField] private GameObject _block;

		private void Start()
		{
			Unblock();
		}

		public void Block()
		{
			SetActivateBlock(true);
		}

		public void Unblock()
		{
			SetActivateBlock(false);
		}

		private void SetActivateBlock(bool active)
		{
			_block.SetActive(active);
		}
	}
}