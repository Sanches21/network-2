using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    [Header("Настройки зума")]
    [SerializeField] private int zoomSpeed = 1;
    [SerializeField] private float minZoom = 0.1f;
    [SerializeField] private float maxZoom = 5f;
    [SerializeField] private int spritesPPU = 100;

    private Camera cam;
    private PixelPerfectCamera ppcam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ppcam = GetComponent<PixelPerfectCamera>();
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int newSize = ppcam.assetsPPU + (int)(scroll * 10 * zoomSpeed) * 10;
            ppcam.assetsPPU = Mathf.Clamp(newSize, (int)(minZoom * spritesPPU), (int)(maxZoom * spritesPPU));
        }
    }
}
