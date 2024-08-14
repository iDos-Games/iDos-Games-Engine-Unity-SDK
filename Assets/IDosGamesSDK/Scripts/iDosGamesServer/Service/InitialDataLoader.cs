using UnityEngine;

namespace IDosGames
{
	public class InitialDataLoader : MonoBehaviour
	{
		private void Start()
		{
			LoadData();
		}

		private void LoadData()
		{
			if (IGSUserData.UserAllDataResult != null)
			{
                UserDataService.ProcessingAllData(IGSUserData.UserAllDataResult);
            }
		}
	}
}