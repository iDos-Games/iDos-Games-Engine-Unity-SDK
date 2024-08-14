using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class ApplicationVersionBar : MonoBehaviour
	{
		[SerializeField] private TMP_Text _version;

		private void Start()
		{
			_version.text = Application.version;
		}
	}
}
