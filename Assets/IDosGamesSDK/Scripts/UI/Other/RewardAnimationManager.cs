using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class RewardAnimationManager : MonoBehaviour
    {
        public static RewardAnimationManager inst;

        [Header("UI")]
        [SerializeField] GameObject rewardPrefab;

        [SerializeField] RectTransform StartPositionReward;
        [SerializeField] RectTransform MiddlePositionReward;
        [SerializeField] RectTransform FinishPositionReward;

        [Space]
        [Header("Available rewards")]
        [SerializeField] public int MaxReward;
        [SerializeField] Queue<GameObject> RewardQueue = new Queue<GameObject>();

        [SerializeField] float spread;

        public AudioSource audioS;
        public AudioClip[] FX;

        float scalingRewards = 1f;
        int idReward = 0;
        //private bool CurrentMove = false;
        int MoneysRestant = 0;

        private RectTransform PosAmount;
        private GameObject PrefabAmount;

        bool PlayAudioFlux = false;

        void Awake()
        {
            if (inst == null)
            {
                inst = this;
            }
        }

        void Start()
        {
            PrepareRewards();
        }

        void PrepareRewards()
        {
            GameObject _reward;
            for (int i = 0; i < MaxReward; i++)
            {
                _reward = Instantiate(rewardPrefab);
                RectTransform moneyRectTransform = _reward.GetComponent<RectTransform>();
                moneyRectTransform.SetParent(StartPositionReward);
                moneyRectTransform.localScale = Vector3.one * scalingRewards;
                moneyRectTransform.position = StartPositionReward.position;
                _reward.GetComponent<Canvas>().overrideSorting = true;
                _reward.GetComponent<Canvas>().sortingOrder = 6;
                _reward.SetActive(false);

                RewardQueue.Enqueue(_reward);
            }

            MoneysRestant = MaxReward;
        }

        public void AddReward(int idRewards)
        {
            int amount = Random.Range(10, 100);

            idReward = idRewards;
            //CurrentMove = true;
            MoneysRestant = MaxReward;
            AnimateRewards(amount);
            audioS.PlayOneShot(FX[0]);
            PlayAudioFlux = false;
        }

        void AnimateRewards(int amount)
        {
            if (idReward >= 0 && idReward <= 2)
            {
                for (int i = 0; i < MaxReward; i++)
                {
                    if (RewardQueue.Count > 0)
                    {
                        GameObject reward = RewardQueue.Dequeue();
                        reward.SetActive(true);

                        RectTransform moneyRectTransform = reward.GetComponent<RectTransform>();

                        StartCoroutine(ScaleAndMoveOverTime(moneyRectTransform, amount));
                    }
                }
            }
        }

        IEnumerator ScaleAndMoveOverTime(RectTransform transform, int amount)
        {
            float duration = 0.2f;

            Vector3 targetMiddlePosition = MiddlePositionReward.position + new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), 0f);
            Vector3 targetFinishPosition = FinishPositionReward.position;

            StartCoroutine(ScaleOverTime(transform, Vector3.one, duration));

            StartCoroutine(MoveOverTime(transform, targetMiddlePosition, 0.5f));

            yield return new WaitForSeconds(0.5f);
            transform.position = transform.position;

            if (!PlayAudioFlux)
            {
                audioS.PlayOneShot(FX[1]);
                PlayAudioFlux = true;
            }

            float duration2 = 0.3f;

            yield return new WaitForSeconds(0.5f);
            StartCoroutine(MoveOverTime(transform, targetFinishPosition, duration2));

            StartCoroutine(DisableMoney(transform.gameObject));
        }

        IEnumerator ScaleOverTime(RectTransform transform, Vector3 toScale, float duration)
        {
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, toScale, elapsedTime / duration);
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            transform.localScale = toScale;
        }

        IEnumerator MoveOverTime(RectTransform transform, Vector3 toPosition, float duration)
        {
            Vector3 startPosition = transform.position;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startPosition, toPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            transform.position = toPosition;
            
        }

        IEnumerator DisableMoney(GameObject money)
        {
            yield return new WaitForSeconds(0.55f);

            money.SetActive(false);
            money.GetComponent<RectTransform>().position = StartPositionReward.position;
            money.GetComponent<RectTransform>().localScale = Vector3.zero;
            RewardQueue.Enqueue(money);
            MoneysRestant--;

            //audioS.PlayOneShot(FX[2]);
        }

        public void AddAmount(int amount)
        {
            GameObject amountText = Instantiate(PrefabAmount);
            RectTransform amountRectTransform = amountText.GetComponent<RectTransform>();
            amountRectTransform.SetParent(PosAmount.GetComponent<RectTransform>());
            amountRectTransform.localScale = Vector3.one;
            amountRectTransform.localPosition = PosAmount.GetComponent<RectTransform>().localPosition;
            amountText.GetComponent<TextMeshProUGUI>().text = "+" + amount.ToString();
        }

        public IEnumerator UpdaterReward(TextMeshProUGUI SelectedText, int CurrentCoins, int newAmount, int idMoney)
        {
            const float seconds = 0.3f;
            float elapsedTime = 0;

            while (elapsedTime < seconds)
            {
                SelectedText.text = Mathf.Floor(Mathf.Lerp(CurrentCoins, newAmount, (elapsedTime / seconds))).ToString();
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            if (idMoney >= 0 && idMoney <= 2)
            {
                SelectedText.text = newAmount.ToString("0");
            }

            StopCoroutine("UpdaterReward");
        }
    }
}
