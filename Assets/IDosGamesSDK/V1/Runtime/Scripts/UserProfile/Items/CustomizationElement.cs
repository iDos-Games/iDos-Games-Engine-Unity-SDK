using UnityEngine;

namespace IDosGames.UserProfile
{
    public class CustomizationElement : MonoBehaviour
    {
        [SerializeField] private ClothingType _type;
        public ClothingType Type => _type;

        [SerializeField] private string _avatarMeshVersion;

        public string AvatarMeshVersion => _avatarMeshVersion;
        public void Activate()
        {
            gameObject.SetActive(true);
        }
        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
        public void SetTexture(string texturePath)
        {
            var skinnedMesh = GetComponent<SkinnedMeshRenderer>();
            skinnedMesh.materials[0].mainTexture = Resources.Load<Texture2D>(texturePath);


        }
    }
}