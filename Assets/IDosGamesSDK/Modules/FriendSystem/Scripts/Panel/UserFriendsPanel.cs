using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace IDosGames.Friends
{

    public class UserFriendsPanel : MonoBehaviour
    {
        [SerializeField] private FriendWindow friendWindow;
        [SerializeField] private Transform content;
        [SerializeField] private UserFriendItem prefab;
        [SerializeField] private TMP_Text _voidText;
        [SerializeField] private TMP_Text _countText;

        public static Action FriendRemoved;
        private int _maxCountFriend = 0;
        private Action<string> action;

        public bool IsNeedUpdate { get; set; } = true;

        private List<string> _friends = new();

        public void Initialize()
        {
            if (IsNeedUpdate)
            {
                //Refresh();
                RefreshData();
            }
        }

        public void Initialize(Action<string> action)
        {
            this.action = action;
            if (IsNeedUpdate)
            {
                //Refresh();
                RefreshData();
            }
        }

        private void Awake()
        {
            var TitleDataRaw = UserDataService.GetCachedTitlePublicConfig("friends");
            if (!string.IsNullOrEmpty(TitleDataRaw))
            {
                var friendsData = JsonConvert.DeserializeObject<JObject>(TitleDataRaw);
                _maxCountFriend = friendsData.GetValue("max_count").Value<int>();
            }
        }

        public void RefreshData()
        {
            if(IGSUserData.Friends == null)
            {
                Refresh();
            }
            else
            {
                List<string> friends = IGSUserData.Friends;
                ProcessRequestResult(friends);
                IsNeedUpdate = false;
            }
        }

        public async void Refresh()
        {
            var requestResult = await RequestData();
            //Debug.Log(requestResult.ToString());
            ProcessRequestResult(requestResult);
            IsNeedUpdate = false;
        }

        private async Task<List<string>> RequestData()
        {
            Loading.ShowTransparentPanel();

            //var request = new IGSRequest();

            var result = await FriendAzureService.GetMyFriend();

            IGSUserData.Friends = result;

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
            _countText.text = $"{_friends.Count} / {_maxCountFriend}";
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

            foreach (var friend in _friends)
            {
                var item = Instantiate(prefab, content);


                if (action != null)
                {
                    item.Fill(() => action?.Invoke(friend), () => DeleteFriend(friend), friend);
                }
                else
                {
                    item.Fill(() => DeleteFriend(friend), friend);
                }
            }
            Loading.HideAllPanels();
        }

        private async void DeleteFriend(string friendID)
        {
            Loading.ShowTransparentPanel();

            IGSRequest friendRequest = new IGSRequest()
            {
                FriendID = friendID
            };

            var result = await FriendAzureService.DeleteFriend(friendRequest);

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
                    FriendRemoved?.Invoke();
                    Message.Show(jObjectResult["Message"].ToString());
                    Refresh();
                }
            }
        }
    }
}
