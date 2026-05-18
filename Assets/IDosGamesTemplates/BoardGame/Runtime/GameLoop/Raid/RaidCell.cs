using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class RaidCell : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image frontImage;   // ������
        [SerializeField] private Image backImage;    // ������� / ���� �������
        [SerializeField] private GameObject glowEffect;
        [SerializeField] private float flipDuration = 0.25f;

        public bool IsOpened { get; private set; }

        private int _digIndex;
        private Action<int> _onSelected;

        // -----------------------------------------------------------------------
        public void Bind(int digIndex, bool alreadyOpened, HeistSymbol symbol,
                         Sprite symbolSprite, Sprite hiddenSprite, Action<int> onSelected)
        {
            _digIndex = digIndex;
            _onSelected = onSelected;
            IsOpened = alreadyOpened;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onSelected?.Invoke(_digIndex));

            if (alreadyOpened)
            {
                frontImage.sprite = symbolSprite;
                ShowFront();
                button.interactable = false;
            }
            else
            {
                backImage.sprite = hiddenSprite;
                ShowBack();
                button.interactable = true;
            }

            if (glowEffect != null)
                glowEffect.SetActive(false);
        }

        public void SetInteractable(bool value) => button.interactable = value;

        /// <summary>������������� �������� ����� ������ �������.</summary>
        public IEnumerator RevealRoutine(Sprite revealedSprite)
        {
            IsOpened = true;
            button.interactable = false;

            // �������� ���������� � ������� �� X
            float t = 0f;
            while (t < flipDuration)
            {
                t += Time.unscaledDeltaTime;
                float scaleX = 1f - Mathf.Clamp01(t / flipDuration);
                transform.localScale = new Vector3(scaleX, 1f, 1f);
                yield return null;
            }

            // ������ ��������
            frontImage.sprite = revealedSprite;
            ShowFront();

            // ������ �������� � ����������
            t = 0f;
            while (t < flipDuration)
            {
                t += Time.unscaledDeltaTime;
                float scaleX = Mathf.Clamp01(t / flipDuration);
                transform.localScale = new Vector3(scaleX, 1f, 1f);
                yield return null;
            }

            transform.localScale = Vector3.one;

            // ������ ���������
            if (glowEffect != null)
            {
                glowEffect.SetActive(true);
                yield return new WaitForSeconds(0.5f);
                glowEffect.SetActive(false);
            }
        }

        /// <summary>���������� ��������� ��� ������ ���������� ���������.</summary>
        public void ForceReveal(Sprite sprite)
        {
            IsOpened = true;
            frontImage.sprite = sprite;
            ShowFront();
            button.interactable = false;
        }

        private void ShowFront()
        {
            frontImage.gameObject.SetActive(true);
            backImage.gameObject.SetActive(false);
        }

        private void ShowBack()
        {
            frontImage.gameObject.SetActive(false);
            backImage.gameObject.SetActive(true);
        }
    }
}
