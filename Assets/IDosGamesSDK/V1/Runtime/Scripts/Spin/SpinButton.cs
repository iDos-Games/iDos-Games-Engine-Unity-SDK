using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class SpinButton : MonoBehaviour
	{
		[SerializeField] private SpinTicketType _spinType;
		[SerializeField] private Button _button;
		public Button Button => _button;
	}
}