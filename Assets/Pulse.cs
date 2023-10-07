using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Pulse : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [Range(0.5f, 2f)]
    [SerializeField] private float maxPulseScale = 1.1f; // Maximum scale during pulse. 1.1 means 10% increase.
    [Range(1f, 60f)]
    [SerializeField] private float rotationDuration = 10f; // Duration in seconds to complete a full rotation.

    private void Start()
    {
        PulseImage(targetImage, 1f); // Example: pulses the image over 1.5 seconds.
        RotateImage(targetImage, rotationDuration); // This will rotate the image indefinitely.
    }

    public void PulseImage(Image image, float speed)
    {
        if (image == null) return;

        Vector3 originalScale = image.rectTransform.localScale;
        Vector3 targetScale = originalScale * maxPulseScale;

        image.rectTransform.DOScale(targetScale, speed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo); // Yoyo looping means it will go to the target value and then back to the original value, creating a smooth in and out effect.
    }

    public void RotateImage(Image image, float duration)
    {
        if (image == null) return;

        image.rectTransform.DORotate(new Vector3(0, 0, 360f), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear) // Linear rotation for constant speed
            .SetLoops(-1, LoopType.Restart); // Restart looping means it will always restart the rotation from 0 degrees, making it seem continuous.
    }
}