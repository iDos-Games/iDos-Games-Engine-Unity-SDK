using UnityEngine;

namespace IDosGames
{
	public class SpinSectorItem : RewardItem
	{
		[Range(1, 8)]
		[SerializeField] private int _sectorIndex;
		public int SectorIndex => _sectorIndex;
	}
}