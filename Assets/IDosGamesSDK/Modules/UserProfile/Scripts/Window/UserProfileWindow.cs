using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;


namespace IDosGames.UserProfile
{
    public class UserProfileWindow : MonoBehaviour
    {
        [SerializeField] private AvatarItemsPanel avatarItemsPanel;
        [SerializeField] private UserInfoPanel userInfoPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _refreshButton;
        [SerializeField] private UserAvatar _userAvatar;
        [SerializeField] private UserProfileRoom _profileRoom;
        [SerializeField] private PopUpSaveChanges _popUpChanges;

        [SerializeField] private CameraMovement _cameraMovement;

        [Header("Position")]
        [SerializeField] private Vector3 _bodyPosition;
        [SerializeField] private Vector3 _glassesPosition;
        [SerializeField] private Vector3 _handsPosition;
        [SerializeField] private Vector3 _hatPosition;
        [SerializeField] private Vector3 _maskPosition;
        [SerializeField] private Vector3 _pantsPosition;
        [SerializeField] private Vector3 _shoesPosition;
        [SerializeField] private Vector3 _torsoPosition;
        [SerializeField] private Vector3 _cameraPositionOnOtherPlayer;


        [SerializeField] private ScrollRect _tabScroll;
        [SerializeField] private float rotationSpeed = 20.0f;

        private const int ROTATION_SPEED_MULTIPLIER = 10;
        private string _userID;

        public UserAvatar UserAvatar => _userAvatar;
        public AvatarItemsPanel AvatarItemsPanel => avatarItemsPanel;


        private void OnEnable()
        {
            _closeButton.onClick.AddListener(TryCLoseRoom);
            UserDataService.ClientModifyCustomUserDataUpdated += OnCustomUpdated;
            _userAvatar.OnEquippedAvatarSkin += OnEquipSkin;
            _userAvatar.OnUnequippedAvatarSkin += OnUnequipSkin;
            _userAvatar.OnInspectAvatarSkin += OnSkinInspect;
            _refreshButton.onClick.AddListener(RefreshAvatar);
        }
        private void OnDisable()
        {
            _closeButton.onClick.RemoveAllListeners();
            UserDataService.ClientModifyCustomUserDataUpdated -= OnCustomUpdated;
            _userAvatar.OnEquippedAvatarSkin -= OnEquipSkin;
            _userAvatar.OnUnequippedAvatarSkin -= OnUnequipSkin;
            _userAvatar.OnInspectAvatarSkin -= OnSkinInspect;
            _refreshButton.onClick.RemoveAllListeners();

        }

        private void Update()
        {
            bool isScrolling = Mathf.Abs(_tabScroll.velocity.x) > 0.1f;
            if (!isScrolling)
            {
                if (Input.GetMouseButton(0)) // Левая кнопка мыши
                {
                    _userAvatar.transform.Rotate(Vector3.down, Input.GetAxis("Mouse X") * rotationSpeed * ROTATION_SPEED_MULTIPLIER * Time.deltaTime);
                }
            }
        }

        private void OnCustomUpdated(string key, CustomUpdateResult result)
        {
            if (key == CustomUserDataKey.equipped_avatar_skins.ToString() || result == CustomUpdateResult.SUCCESS)
            {
                _popUpChanges.gameObject.SetActive(false);
                Loading.HideAllPanels();
                _profileRoom.CloseRoom();
            }

        }

        public void RefreshAvatar()
        {
            _userAvatar.RefreshAvatar();
        }

        private void TryCLoseRoom()
        {
            if (_userID == AuthService.UserID)
            {


                if (_userAvatar.AreChanges())
                {
                    _popUpChanges.gameObject.SetActive(true);
                }
                else
                {
                    _userID = null;
                    _profileRoom.CloseRoom();
                }
            }
            else
            {
                _userID = null;
                _profileRoom.CloseRoom();
            }

        }
        public void DontSaveChanges()
        {
            _popUpChanges.gameObject.SetActive(false);
            _userID = null;
            _profileRoom.CloseRoom();
        }

        public void SaveChanges()
        {
            Loading.ShowTransparentPanel();
            var saveData = _userAvatar.GetUpdateData();
            UserProfileRoom._equipedAvatarSkins = saveData;
            UserDataService.UpdateCustomUserData(CustomUserDataKey.equipped_avatar_skins.ToString(), saveData);
        }

        public void MoveCameraTo(ClothingType clothingType)
        {
            switch (clothingType)
            {
                case ClothingType.Body:
                    _cameraMovement.SetTarget(_bodyPosition);
                    break;
                case ClothingType.Glasses:
                    _cameraMovement.SetTarget(_glassesPosition);
                    break;
                case ClothingType.Mask:
                    _cameraMovement.SetTarget(_maskPosition);
                    break;
                case ClothingType.Pants:
                    _cameraMovement.SetTarget(_pantsPosition);
                    break;
                case ClothingType.Hands:
                    _cameraMovement.SetTarget(_handsPosition);
                    break;
                case ClothingType.Hat:
                    _cameraMovement.SetTarget(_hatPosition);
                    break;
                case ClothingType.Shoes:
                    _cameraMovement.SetTarget(_shoesPosition);
                    break;
                case ClothingType.Torso:
                    _cameraMovement.SetTarget(_torsoPosition);
                    break;
            }
        }

        public void Init(string playfabID, DefaultAvatarSkin data)
        {
            _cameraMovement.SetTarget(_cameraPositionOnOtherPlayer);
            if (playfabID == AuthService.UserID)
            {
                userInfoPanel.gameObject.SetActive(false);
                avatarItemsPanel.gameObject.SetActive(true);
                _refreshButton.gameObject.SetActive(true);
            }
            else
            {
                userInfoPanel.gameObject.SetActive(true);
                avatarItemsPanel.gameObject.SetActive(false);
                _refreshButton.gameObject.SetActive(false);
            }
            _userID = playfabID;

            _userAvatar.Init(data);
        }

        public void ChangeGender(Gender gender)
        {
            _userAvatar.ChangeAvatarGender(gender);
        }

        public void EquipSkin(string itemID)
        {
            _userAvatar.EquipSkin(itemID);

        }

        public void UnequipSkin(string itemID)
        {
            _userAvatar.UnequipSkin(itemID);

        }

        public void InspectSkin(string itemID)
        {
            _userAvatar.InspectSkin(itemID);
        }

        public void OnEquipSkin(string itemID)
        {
            avatarItemsPanel.RefreshItem(itemID);
        }
        public void OnUnequipSkin(string itemID)
        {
            avatarItemsPanel.RefreshItem(itemID);
        }
        public void OnSkinInspect(string itemID)
        {
            avatarItemsPanel.RefreshItem(itemID);
        }

    }
}
