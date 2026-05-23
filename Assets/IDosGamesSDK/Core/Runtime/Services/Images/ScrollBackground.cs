using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScrollBackground : MonoBehaviour
{
    [SerializeField] private Vector2 speed = new Vector2(0.05f, 0.05f);

    private RawImage image;

    private void Awake()
    {
        image = GetComponent<RawImage>();
    }

    private void Update()
    {
        Rect uv = image.uvRect;

        uv.position += speed * Time.deltaTime;

        uv.x = Mathf.Repeat(uv.x, 1f);
        uv.y = Mathf.Repeat(uv.y, 1f);

        image.uvRect = uv;
    }
}
