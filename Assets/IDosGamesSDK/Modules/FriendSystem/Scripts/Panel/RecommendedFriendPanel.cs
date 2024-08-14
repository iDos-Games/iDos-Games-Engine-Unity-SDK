using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace IDosGames.Friends
{
    public class RecommendedFriendPanel : MonoBehaviour
    {
        [SerializeField] private FriendWindow friendWindow;
        [SerializeField] private Transform content;
        [SerializeField] private RecomendedFriendItem prefab;
        [SerializeField] private TMP_Text _voidText;

        public bool IsNeedUpdate { get; set; } = true;

        private List<string> _friends = new();

        private void OnEnable()
        {
            if (IsNeedUpdate)
            {
                RefreshData();
            }
        }

        public void RefreshData()
        {
            if (string.IsNullOrEmpty(IGSUserData.RecommendedFriends))
            {
                Refresh();
            }
            else
            {
                string friendRequests = IGSUserData.RecommendedFriends;
                ProcessRequestResult(friendRequests);
                IsNeedUpdate = false;
            }
        }

        public async void Refresh()
        {
            var requestResult = await RequestData();
            ProcessRequestResult(requestResult);
            IsNeedUpdate = false;
        }

        private async Task<string> RequestData()
        {
            Loading.ShowTransparentPanel();

            var result = await FriendAzureService.GetRecommendedFriends();

            IGSUserData.RecommendedFriends = result;

            Loading.HideAllPanels();

            return result;
        }

        private void ProcessRequestResult(string result)
        {

            if (result == null)
            {
                Loading.HideAllPanels();
                Message.Show(MessageCode.FAILED_TO_LOAD_DATA);
            }

            var jObjectResult = JsonConvert.DeserializeObject<JObject>(result);

            if (jObjectResult.ContainsKey("Message"))
            {
                Loading.HideAllPanels();
                Message.Show(jObjectResult["Message"].ToString());
                return;
            }

            _friends = JsonConvert.DeserializeObject<List<string>>(jObjectResult[FriendKeys.RecommendedFriends.ToString()].ToString());

            if (_friends.Count == 0)
            {
                _voidText.gameObject.SetActive(true);
            }
            else
            {
                _voidText.gameObject.SetActive(false);
            }

            InstantiateItems();
        }

        private void InstantiateItems(bool destroyChildren = true)
        {
            if (destroyChildren)
            {
                foreach (Transform child in content)
                {
                    Destroy(child.gameObject);
                }

            }

            foreach (var item in _friends)
            {
                var offer = Instantiate(prefab, content);

                offer.Fill(async () => await AddFriend(item, offer), item);
            }
        }

        private async Task AddFriend(string playfabID, RecomendedFriendItem item)
        {
            Loading.ShowTransparentPanel();
            item.SetInactivebutton(false);

            IGSRequest addFriendRequest = new IGSRequest()
            {
                FriendID = AuthService.UserID
            };

            var result = await FriendAzureService.SendRequestToAdd(addFriendRequest);

            if (IDosGamesSDKSettings.Instance.DebugLogging)
            {
                Debug.Log(result);
            }

            if (result == null)
            {
                Message.Show(MessageCode.FAILED_TO_LOAD_DATA);
                Loading.HideAllPanels();
                item.SetInactivebutton(true);
                return;
            }

            var jObjectResult = JsonConvert.DeserializeObject<JObject>(result);

            if (jObjectResult.ContainsKey("Message"))
            {
                if (jObjectResult["Message"].ToString() != "SUCCESS")
                {
                    Message.Show(jObjectResult["Message"].ToString());
                    Loading.HideAllPanels();
                    item.SetInactivebutton(true);
                    return;
                }
                else
                {

                    Message.Show(jObjectResult["Message"].ToString());
                }
            }
        }
    }
}