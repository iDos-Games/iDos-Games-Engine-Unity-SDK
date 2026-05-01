using QRCoder;
using QRCoder.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class PanelReferralQR : MonoBehaviour
	{
		[SerializeField] private Image _qrImage;

		private void Awake()
		{
			GenerateQrCode();
		}

		private void GenerateQrCode()
		{
			var url = ReferralSystem.ReferralLink;

			if (url == null)
			{
				return;
			}

			QRCodeGenerator qrGenerator = new();

			QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);
			UnityQRCode qrCode = new(qrCodeData);

			GUIUtility.systemCopyBuffer = url;
			Texture2D qrCodeAsTexture2D = qrCode.GetGraphic(pixelsPerModule: 10);

			qrCodeAsTexture2D.filterMode = FilterMode.Point;

			var qrCodeSprite = Sprite.Create(qrCodeAsTexture2D, new Rect(0, 0, qrCodeAsTexture2D.width, qrCodeAsTexture2D.height),
				new Vector2(0.5f, 0.5f), 100f);

			_qrImage.sprite = qrCodeSprite;
		}
	}
}
