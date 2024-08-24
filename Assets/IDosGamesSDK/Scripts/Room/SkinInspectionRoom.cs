using IDosGames.UserProfile;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class SkinInspectionRoom : Room
    {
        [SerializeField] private ObjectInspection _objectInspectionData;
        [SerializeField] private Transform _root;
        [SerializeField] private SkinInspectionRoomRotationHandler _rotationHandler;
        [SerializeField] private InspectAvatar _avatar;
        [SerializeField] private Camera _camera;
        [SerializeField] private Vector3 _cameraDefaultPosition;
        [SerializeField] private Quaternion _cameraDefaultRotation;
        [SerializeField] private Vector3 _bodyPosition;
        [SerializeField] private Vector3 _headPosition;
        [SerializeField] private Vector3 _pantsPosition;

        [Header("UI")]
        [Space(5)]
        [SerializeField] private TMP_Text _displayName;
        [SerializeField] private Image _autoRotationCheckmark;

        private void Awake()
        {
            UpdateAuthoRotationCheckmark();
        }

        public void OpenRoom(string itemID)
        {
            foreach (Transform child in _root)
            {
                Destroy(child.gameObject);
            }
            _camera.transform.SetLocalPositionAndRotation(_cameraDefaultPosition, _cameraDefaultRotation);
            var skinItem = UserDataService.GetCachedSkinItem(itemID);
            if (skinItem != null)
            {
                var inspectionData = _objectInspectionData.GetInspectionData(skinItem.ObjectType);

                if (inspectionData != null)
                {
                    var model = Instantiate(inspectionData.Model, _root);

                    SetModelTransform(model.transform, inspectionData);
                    //SetMaterialToModel(model);
                    SetInspectionMaterialTexture(skinItem.TexturePath, inspectionData.ModelMaterial);

                    if (inspectionData.AdditionalObject != null)
                    {
                        var additionalObject = Instantiate(inspectionData.AdditionalObject, _root);
                        additionalObject.Set(skinItem.ItemID);
                    }
                }

                SetDisplayName(skinItem.DisplayName);
                SetActiveRoom(true);
            }

            else if (skinItem == null)
            {
                var avatarItem = UserDataService.GetAvatarSkinItem(itemID);
                if (avatarItem != null)
                {
                    SetCameraPosition(avatarItem.ClothingType);
                    _avatar.InspectAvatarSkin(itemID);
                    SetDisplayName(avatarItem.DisplayName);
                    SetActiveRoom(true);
                }
            }
        }

        private void SetCameraPosition(ClothingType type)
        {
            switch (type)
            {
                case ClothingType.Body:
                case ClothingType.Hands:
                case ClothingType.Torso:
                    _camera.transform.SetLocalPositionAndRotation(_bodyPosition, _cameraDefaultRotation); break;
                case ClothingType.Hat:
                case ClothingType.Mask:
                case ClothingType.Glasses:
                    _camera.transform.SetLocalPositionAndRotation(_headPosition, _cameraDefaultRotation); break;
                case ClothingType.Pants:
                case ClothingType.Shoes:
                    _camera.transform.SetLocalPositionAndRotation(_pantsPosition, _cameraDefaultRotation); break;
            }
        }

        public void OpenAvatarRoom(string itemID)
        {
            var avatarItem = UserDataService.GetAvatarSkinItem(itemID);
        }
        public void SwitchAutoRotation()
        {
            _rotationHandler.SwitchAutoRotation();
            UpdateAuthoRotationCheckmark();
        }

        private void SetDisplayName(string displayName)
        {
            _displayName.text = displayName;
        }

        private void UpdateAuthoRotationCheckmark()
        {
            _autoRotationCheckmark.gameObject.SetActive(_rotationHandler.AutoRotation);
        }
        /*
        private void SetMaterialToModel(MeshRenderer model)
        {
            model.materials = new[] { _objectInspectionData.InspectionMaterial };
        }
        */
        private void SetModelTransform(Transform transform, ObjectInspectionData data)
        {
            transform.SetLocalPositionAndRotation(data.Position, Quaternion.Euler(data.Rotation));
            transform.localScale = data.Scale;
        }

        private void SetInspectionMaterialTexture(string texturePath, Material material)
        {
            material.mainTexture = Resources.Load<Texture2D>(texturePath);
            //_objectInspectionData.InspectionMaterial.mainTexture = Resources.Load<Texture2D>(texturePath);
        }
    }
}
