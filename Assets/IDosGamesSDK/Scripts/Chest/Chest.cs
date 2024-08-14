using System.Linq;
using UnityEngine;

namespace IDosGames
{
	public class Chest : MonoBehaviour
	{
		[SerializeField] private ChestAnimation _animation;
		[SerializeField] private ChestSounds _sounds;

		public ChestAnimation Animation => _animation;
		public ChestSounds Sounds => _sounds;

		[SerializeField] private SkinnedMeshRenderer _mesh;

		public ChestMaterial[] Materials;

		public void SetMaterialByRarity(ChestRarityType rarity)
		{
			var material = Materials.FirstOrDefault(x => x.rarity == rarity).material;
			_mesh.material = material;
		}
	}
}