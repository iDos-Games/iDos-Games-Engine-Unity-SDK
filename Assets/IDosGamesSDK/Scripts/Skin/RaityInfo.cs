using System;
using UnityEngine;

namespace IDosGames
{
	[Serializable]
	public class RaityInfo
	{
		[SerializeField] private RarityType _rariryType;
		public RarityType RariryType => _rariryType;

		[SerializeField] private Sprite _backgroundImage;
		public Sprite BackgroundImage => _backgroundImage;
	}
}