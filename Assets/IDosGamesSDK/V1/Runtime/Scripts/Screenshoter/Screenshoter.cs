using UnityEngine;

namespace IDosGames
{
	public class Screenshoter : MonoBehaviour
	{
		[Range(1, 10)]
		[SerializeField] private int _scale = 4;
		private const string SCREENSHOTS_FOLDER_PATH = "Assets/Screenshots/";

		private void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				TakeScreenShot();

				Debug.Log("Screenshoted");
			}
		}

		private void TakeScreenShot()
		{
			if (!System.IO.Directory.Exists(SCREENSHOTS_FOLDER_PATH))
			{
				System.IO.Directory.CreateDirectory(SCREENSHOTS_FOLDER_PATH);
			}

			var screenshotName = $"Screenshot_{System.DateTime.UtcNow.ToString("dd-MM-yyyy-HH-mm-ss")}.png";
			ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(SCREENSHOTS_FOLDER_PATH, screenshotName), _scale);
		}

	}
}