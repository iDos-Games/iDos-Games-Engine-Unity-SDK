using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
	public class Rarity
	{
		private static readonly Color32 WhiteColor = new(255, 255, 255, 255);
		private static readonly Color32 GreenColor = new(25, 255, 0, 255);
		private static readonly Color32 BlueColor = new(0, 150, 255, 255);
		private static readonly Color32 PurpleColor = new(255, 0, 255, 255);
		private static readonly Color32 OrangeColor = new(255, 150, 0, 255);

		private static readonly Dictionary<RarityType, Color> ColorsDictionary = new()
		{
			{ RarityType.Common, WhiteColor },
			{ RarityType.Uncommon, GreenColor },
			{ RarityType.Rare, BlueColor },
			{ RarityType.Epic, PurpleColor },
			{ RarityType.Legendary, OrangeColor }
		};

		public static Color32 GetColor(RarityType rarity)
		{
			return ColorsDictionary[rarity];
		}
	}
}