using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class CopyPrivateKey : MonoBehaviour
    {
        public TextMeshProUGUI privateKeyText;

        private void OnDisable()
        {
            privateKeyText.text = null;
        }

        public void CopyKey()
        {
            GUIUtility.systemCopyBuffer = privateKeyText.text;

#if UNITY_WEBGL && !UNITY_EDITOR
            WebSDK.CopyTextToClipboard(privateKeyText.text);
#endif

            StartCoroutine(ClosePanel());
        }

        private IEnumerator ClosePanel()
        {
            yield return null;
            this.gameObject.SetActive(false);
        }
    }
}
