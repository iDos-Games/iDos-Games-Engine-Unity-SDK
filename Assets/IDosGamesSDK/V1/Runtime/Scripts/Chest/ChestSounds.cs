using UnityEngine;

namespace IDosGames
{
	public class ChestSounds : MonoBehaviour
	{
		[SerializeField] private AudioClip _dropSound;
		[SerializeField][Range(0, 1)] private float _dropSoundVolume = 1;

		[SerializeField] private AudioClip _openingSound;
		[SerializeField][Range(0, 1)] private float _openingSoundVolume = 1;

		public void PlayDropSound()
		{
			if (_dropSound == null)
			{
				return;
			}

			AudioSource.PlayClipAtPoint(_dropSound, transform.position, _dropSoundVolume);
		}

		public void PlayOpeningSound()
		{
			if (_openingSound == null)
			{
				return;
			}

			AudioSource.PlayClipAtPoint(_openingSound, transform.position, _openingSoundVolume);
		}
	}
}
