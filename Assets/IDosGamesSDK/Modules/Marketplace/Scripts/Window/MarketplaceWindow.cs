using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
	public class MarketplaceWindow : MonoBehaviour
	{
		public const int MIN_OFFER_PRICE = 100;

		[SerializeField] private PanelGroupedOffers _panelGropedOffers;
		[SerializeField] private PanelOffersByItemID _panelOffersByItemID;
		[SerializeField] private PanelPlayerOffers _panelPlayerOffers;
		[SerializeField] private PanelMarketplaceCreateOffer _panelCreateOffer;
		[SerializeField] private SkinInspectionRoom _skinInspectionRoom;

		public static int CompanyCommission { get; private set; }
		public static int ReferralCommission { get; private set; }
		public static int AuthorCommission { get; private set; }
		public static int SumOfAllCommissions { get; private set; }

#if IDOSGAMES_MARKETPLACE
		private void OnEnable()
		{
			PanelOffersByItemID.OfferBuyed += OnOfferBuyed;
			PanelPlayerActiveOffers.OfferChanged += OnOfferChanged;
			PanelMarketplaceCreateOffer.OfferCreated += OnOfferCreated;
		}

		private void OnDisable()
		{
			PanelOffersByItemID.OfferBuyed -= OnOfferBuyed;
			PanelPlayerActiveOffers.OfferChanged -= OnOfferChanged;
			PanelMarketplaceCreateOffer.OfferCreated -= OnOfferCreated;
		}

		private void Start()
		{
			SetCommissions();
			OpenAllOffersPanel();
		}

        private void SetCommissions()
        {
            var commission = IDosGamesData.Config.TitlePublicConfiguration?.CommissionRoyaltyPercentage;

            if (commission == null)
            {
                return;
            }

            CompanyCommission = commission.Company > 0 ? commission.Company : 0;
            ReferralCommission = commission.Referral > 0 ? commission.Referral : 0;
            AuthorCommission = commission.Author > 0 ? commission.Author : 0;
            SumOfAllCommissions = CompanyCommission + ReferralCommission + AuthorCommission;
        }

        public void InspectSkin(string itemID)
		{
			_skinInspectionRoom.OpenRoom(itemID);
		}

		public async Task OpenBuySkinByItemID(string itemID)
		{
			await _panelOffersByItemID.OpenPanel(itemID);
			_panelGropedOffers.gameObject.SetActive(false);
		}

		public void OpenAllOffersPanel()
		{
			_panelCreateOffer.gameObject.SetActive(false);
			_panelPlayerOffers.gameObject.SetActive(false);
			_panelOffersByItemID.gameObject.SetActive(false);
			_panelGropedOffers.gameObject.SetActive(true);
		}

		public void OpenMyOffersPanel()
		{
			_panelCreateOffer.gameObject.SetActive(false);
			_panelGropedOffers.gameObject.SetActive(false);
			_panelOffersByItemID.gameObject.SetActive(false);
			_panelPlayerOffers.gameObject.SetActive(true);
		}

		public void OpenCreateOfferPanel()
		{
			_panelGropedOffers.gameObject.SetActive(false);
			_panelOffersByItemID.gameObject.SetActive(false);
			_panelPlayerOffers.gameObject.SetActive(false);
			_panelCreateOffer.gameObject.SetActive(true);
		}

		private void OnOfferBuyed()
		{
			_panelGropedOffers.IsNeedUpdate = true;
			_panelPlayerOffers.ActiveOffersPanel.IsNeedUpdate = true;
			_panelPlayerOffers.HistoryPanel.IsNeedUpdate = true;
		}

		private void OnOfferCreated()
		{
			_panelGropedOffers.IsNeedUpdate = true;
			_panelPlayerOffers.ActiveOffersPanel.IsNeedUpdate = true;
		}

		private void OnOfferChanged()
		{
			_panelGropedOffers.IsNeedUpdate = true;
		}
#endif
    }
}
