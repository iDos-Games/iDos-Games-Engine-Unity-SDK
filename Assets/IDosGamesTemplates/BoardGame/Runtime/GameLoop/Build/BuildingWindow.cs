using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;

namespace IDosGames
{
    public class BuildingWindow : MonoBehaviour
    {
        [Header("Cards")]
        [SerializeField] private List<BuildingCard> cards;

        [Header("Stage")]
        [SerializeField] private TextMeshProUGUI stageNameText;
        [SerializeField] private Image stageImage;
        [SerializeField] private List<StageBuilding> stageBuildings;

        private string _loadedStageImagePath;

        private void OnEnable()
        {
            if (BoardGameManager.Instance != null)
            {
                BoardGameManager.Instance.OnBoardReady += Refresh;
                BoardGameManager.Instance.OnDataUpdated += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (BoardGameManager.Instance != null)
            {
                BoardGameManager.Instance.OnBoardReady -= Refresh;
                BoardGameManager.Instance.OnDataUpdated -= Refresh;
            }
        }

        public void Refresh()
        {
            if (BoardGameManager.Instance == null || !BoardGameManager.Instance.IsBoardReady)
                return;

            var data = BoardGameManager.Instance;
            var stage = data.CurrentStage;
            var state = data.BoardState;

            if (stage?.Buildings == null || state?.BuildingStates == null)
                return;

            if (stageNameText != null)
                stageNameText.text = stage.Name;

            string stageImagePath = ResolveStageImagePath(stage.AssetPaths);

            if (stageImagePath != _loadedStageImagePath)
            {
                _loadedStageImagePath = stageImagePath;
                LoadStageImage(stageImagePath);
            }

            int slotCount = 0;
            if (stage.Buildings.Count > 0)
                slotCount = stage.Buildings.Max(x => x?.SlotIndex ?? -1) + 1;

            if (cards != null)
            {
                for (int slotIndex = 0; slotIndex < cards.Count; slotIndex++)
                {
                    var card = cards[slotIndex];
                    if (card == null)
                        continue;

                    var definition = GetDefinitionBySlotIndex(stage, slotIndex);
                    var buildingState = GetStateBySlotIndex(state, slotIndex);

                    if (slotIndex < slotCount && definition != null && buildingState != null)
                    {
                        card.gameObject.SetActive(true);
                        card.Initialize(slotIndex);
                        card.UpdateView(
                            definition: definition,
                            currentLevel: buildingState.Level,
                            isDamaged: buildingState.IsDamaged,
                            maxRewardClaimed: buildingState.MaxLevelRewardClaimed,
                            costGrowthFactor: stage.CostGrowthFactor
                        );
                    }
                    else
                    {
                        card.gameObject.SetActive(false);
                    }
                }
            }

            if (stageBuildings != null)
            {
                for (int slotIndex = 0; slotIndex < stageBuildings.Count; slotIndex++)
                {
                    var stageBuilding = stageBuildings[slotIndex];
                    if (stageBuilding == null)
                        continue;

                    var definition = GetDefinitionBySlotIndex(stage, slotIndex);
                    var buildingState = GetStateBySlotIndex(state, slotIndex);

                    if (slotIndex < slotCount && definition != null && buildingState != null)
                    {
                        stageBuilding.gameObject.SetActive(true);
                        stageBuilding.Initialize(slotIndex);
                        stageBuilding.UpdateView(
                            definition: definition,
                            currentLevel: buildingState.Level,
                            isDamaged: buildingState.IsDamaged
                        );
                    }
                    else
                    {
                        stageBuilding.gameObject.SetActive(false);
                    }
                }
            }
        }

        private async void LoadStageImage(string imagePath)
        {
            var sprite = await ImageLoader.GetSpriteAsync(imagePath);

            if (sprite != null && this != null && stageImage != null)
            {
                stageImage.sprite = sprite;
            }
        }

        private static BuildingDefinition GetDefinitionBySlotIndex(BoardStageDefinition stage, int slotIndex)
        {
            return stage?.Buildings?.Find(x => x != null && x.SlotIndex == slotIndex);
        }

        private static string ResolveStageImagePath(Dictionary<string, string> assetPaths)
        {
            const string fallback = "Sprites/Background/Default";

            if (assetPaths == null || assetPaths.Count == 0)
                return fallback;

            if (assetPaths.TryGetValue("icon", out var icon) && !string.IsNullOrWhiteSpace(icon))
                return icon;

            foreach (var value in assetPaths.Values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return fallback;
        }

        private static BuildingState GetStateBySlotIndex(BoardLoopState state, int slotIndex)
        {
            return state?.BuildingStates?.Find(x => x != null && x.SlotIndex == slotIndex);
        }
    }
}
