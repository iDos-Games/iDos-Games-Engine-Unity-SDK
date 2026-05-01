using UnityEngine;
using UnityEngine.EventSystems;

namespace IDosGames
{
	public class SkinInspectionRoomRotationHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
	{
		[SerializeField] private Transform _root;
		[SerializeField][Range(1, 30)] private float _rotationSpeed = 10.0f;
		[SerializeField][Range(1, 30)] private float _autoRotationSpeed = 20.0f;
		public bool AutoRotation { get; private set; } = true;

		private bool _canAutoRotate = false;
		private const int ROTATION_SPEED_MULTIPLIER = 10;

		private void OnEnable()
		{
			_canAutoRotate = true;
		}

		private void Update()
		{
			if (!AutoRotation)
			{
				return;
			}

			if (!_canAutoRotate)
			{
				return;
			}

			_root.Rotate(Vector3.down, _autoRotationSpeed * Time.deltaTime);
		}

		public void SwitchAutoRotation()
		{
			AutoRotation = !AutoRotation;
		}

		public void OnDrag(PointerEventData eventData)
		{
			_root.Rotate(Vector3.down, Input.GetAxis("Mouse X") * _rotationSpeed * ROTATION_SPEED_MULTIPLIER * Time.deltaTime);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_canAutoRotate = true;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_canAutoRotate = false;
		}
	}
}
