using System.Collections;
using UnityEngine;

namespace IDosGames
{
	public class ChestRewardRoom : Room
	{
		private const float CHANGE_STATE_DELAY = 1.5f;
		[SerializeField] private Chest _chest;
		[SerializeField] private ChestRewardCard _rewardCard;
		[SerializeField] private ButtonContinue _buttonContinue;

		private ChestRoomWaitState _currentState;

		protected override void OnEnable()
		{
			base.OnEnable();
			SetInitialState();
			DropChest();
		}

		private void Start()
		{
			_buttonContinue.ResetListener(OnClickContinue);
		}

		public void ShowReward(ChestRarityType rarity, SkinCatalogItem item)
		{
			SetChest(rarity);
			SetReward(item);

			SetActiveRoom(true);
		}

		private void SetChest(ChestRarityType rarity)
		{
			_chest.SetMaterialByRarity(rarity);
		}

		private async void SetReward(SkinCatalogItem item)
		{
			var icon = await ImageLoader.GetSpriteAsync(item.ImagePath);

			_rewardCard.Set(item.Rarity, icon, item.DisplayName);
		}

		private void SetInitialState()
		{
			_currentState = ChestRoomWaitState.DropChest;
			_rewardCard.Animation.ResetAnimation();
			_chest.Animation.Disappear();
		}

		private void OnClickContinue()
		{
			switch (_currentState)
			{
				case ChestRoomWaitState.DropChest:
					DropChest();
					break;
				case ChestRoomWaitState.OpenChest:
					OpenChest();
					break;
				case ChestRoomWaitState.LeaveRoom:
					LeaveRoom();
					break;
			}
		}

		private void DropChest()
		{
			_chest.Animation.Drop();
			_chest.Sounds.PlayDropSound();
			StartCoroutine(ChangeToNextState());
		}

		private void OpenChest()
		{
			_chest.Animation.Open();
			_chest.Sounds.PlayOpeningSound();
			_rewardCard.Animation.StartAnimation();
			StartCoroutine(ChangeToNextState());
		}

		private void LeaveRoom()
		{
			SetActiveRoom(false);
		}

		private IEnumerator ChangeToNextState()
		{
			SetActiveContinueButton(false);

			yield return new WaitForSecondsRealtime(CHANGE_STATE_DELAY);

			_currentState++;

			SetActiveContinueButton(true);
		}

		private void SetActiveContinueButton(bool active)
		{
			_buttonContinue.SetActive(active);
		}
	}
}