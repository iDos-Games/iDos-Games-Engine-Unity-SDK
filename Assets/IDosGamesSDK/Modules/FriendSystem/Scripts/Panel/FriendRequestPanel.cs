using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace IDosGames.Friends
{
    public class FriendRequestPanel : MonoBehaviour
    {
        [SerializeField] private FriendWindow friendWindow;
        [SerializeField] private Transform content;
        [SerializeField] private FriendRequestItem prefab;
        [SerializeField] private TMP_Text _voidText;

        public static Action FriendAdded;
        public bool IsNeedUpdate { get; set; } = true;

        private List<string> _friends = new();

        //private string readOnlyKey = "friend_requests";
        private int _maxCountFriend = 0;

        private void OnEnable()
        {
            if (IsNeedUpdate)
            {
                RefreshData();
            }
        }

        private void Start()
        {
            var TitleDataRaw = UserDataService.GetCachedTitlePublicConfig("friends");
            if (!string.IsNullOrEmpty(TitleDataRaw))
            {
                var friendsData = JsonConvert.DeserializeObject<JObject>(TitleDataRaw);
                _maxCountFriend = friendsData.GetValue("max_count").Value<int>();
            }
        }

        private void OnDisable()
        {
        }

        public void RefreshData()
        {
            if (IGSUserData.FriendRequests == null)
            {
                Refresh();
            }
            else
            {
                List<string> friendRequests = IGSUserData.FriendRequests;
                ProcessRequestResult(friendRequests);
                IsNeedUpdate = false;
            }
        }

        public async void Refresh()
        {
            var requestResult = await RequestData();
            //Debug.Log(requestResult);
            ProcessRequestResult(requestResult);
            IsNeedUpdate = false;
        }

        private async Task<List<string>> RequestData()
        {
            Loading.ShowTransparentPanel();

            var result = await FriendAzureService.GetPendingFriendRequests();

            IGSUserData.FriendRequests = result;

            Loading.HideAllPanels();

            return result;
        }

        private void ProcessRequestResult(List<string> result)
        {
            if (result == null)
            {
                Loading.HideAllPanels();
                Message.Show(MessageCode.FAILED_TO_LOAD_DATA);
            }

            _friends = result;

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

                offer.Fill(async () => await AcceptFriend(item, offer), async () => await RejectFriend(item, offer), item);
            }
        }

        private async Task AcceptFriend(string playfabID, FriendRequestItem item)
        {
            if (_friends.Count + 1 > _maxCountFriend)
            {
                Message.Show(MessageCode.HAVE_LIMIT_MAX_COUNT);
                return;
            }

            Loading.ShowTransparentPanel();

            IGSRequest friendRequest = new IGSRequest()
            {
                FriendID = AuthService.UserID
            };

            var result = await FriendAzureService.AcceptRequest(friendRequest);

            if (result == null)
            {
                Message.Show(MessageCode.FAILED_TO_LOAD_DATA);
                Loading.HideAllPanels();
                return;
            }

            var jObjectResult = JsonConvert.DeserializeObject<JObject>(result);

            if (jObjectResult.ContainsKey("Message"))
            {
                if (jObjectResult["Message"].ToString() != "SUCCESS")
                {
                    Message.Show(jObjectResult["Message"].ToString());
                    Loading.HideAllPanels();
                    return;
                }
                else
                {
                    item.gameObject.SetActive(false);
                    Message.Show(jObjectResult["Message"].ToString());
                    UserDataService.RequestUserAllData();
                    IsNeedUpdate = true;
                    FriendAdded?.Invoke();
                }
            }
        }

        private async Task RejectFriend(string playfabID, FriendRequestItem item)
        {
            Loading.ShowTransparentPanel();

            IGSRequest friendRequest = new IGSRequest()
            {
                FriendID = AuthService.UserID
            };

            var result = await FriendAzureService.RejectRequest(friendRequest);

            if (result == null)
            {
                Message.Show(MessageCode.FAILED_TO_LOAD_DATA);
                Loading.HideAllPanels();
                return;
            }

            var jObjectResult = JsonConvert.DeserializeObject<JObject>(result);

            if (jObjectResult.ContainsKey("Message"))
            {
                if (jObjectResult["Message"].ToString() != "SUCCESS")
                {
                    Message.Show(jObjectResult["Message"].ToString());
                    Loading.HideAllPanels();
                    return;
                }
                else
                {
                    item.gameObject.SetActive(false);
                    Message.Show(jObjectResult["Message"].ToString());
                    UserDataService.RequestUserAllData();
                    IsNeedUpdate = true;
                }
            }
        }
    }
}