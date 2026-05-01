
using IDosGames.ClientModels;

namespace IDosGames
{
	public abstract class ItemFromCatalog
	{
		public string ItemID => _itemID;
		public string DisplayName => _displayName;

		private readonly string _itemID;
		private readonly string _displayName;

		public ItemFromCatalog(CatalogItem item)
		{
			_itemID = item.ItemId;
			_displayName = item.DisplayName;
		}
	}
}