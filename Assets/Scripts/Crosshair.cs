using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField] RectTransform crosshair;

    [Header("Size")]
    [SerializeField] float hipSize = 80f;
    [SerializeField] float aimSize = 6f;

    [Header("Smooth")]
    [SerializeField] float smooth = 14f;

    float targetSize;

    void Awake()
    {
        targetSize = hipSize;
    }

    void Update()
    {
        float size = Mathf.Lerp(
            crosshair.sizeDelta.x,
            targetSize,
            Time.deltaTime * smooth
        );

        crosshair.sizeDelta = new Vector2(size, size);
    }

    public void SetAiming(bool aiming)
    {
        targetSize = aiming ? aimSize : hipSize;
    }
}
