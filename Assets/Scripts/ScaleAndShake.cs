using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScaleAndShake : MonoBehaviour
{
    public Image myImage; // Reference to your UIImage
    private Vector3 originalScale; // To store the original scale of the UIImage
    private Quaternion _originalRotation;

    private Sequence sequence;

    private void OnEnable()
    {
        originalScale = myImage.transform.localScale; // Store the original scale
        _originalRotation = transform.rotation;
    }

    public void Animate()
    {
        ShakeAndScale();
    }

    public void StopAnimation()
    {
        sequence.Kill();
        transform.rotation = _originalRotation;
    }

    void ShakeAndScale()
    {
        sequence = DOTween.Sequence();

        // Scale up to 1.25x of its original size
        sequence.Append(myImage.transform.DOScale(originalScale * 1.25f, 0.5f)).SetEase(Ease.OutBounce);

        // Shake 4 times
        // Rotate to 2 o'clock (which is equivalent to -30 degrees in Unity's coordinate system)
        sequence.Append(myImage.transform.DORotate(new Vector3(0, 0, -30f), 0.2f, RotateMode.Fast)).SetEase(Ease.OutQuad);
        
        // Rotate to 10 o'clock (which is equivalent to 30 degrees)
        sequence.Append(myImage.transform.DORotate(new Vector3(0, 0, 30f), 0.2f, RotateMode.Fast)).SetEase(Ease.OutQuad);
        
        // Repeat the rotations
        sequence.Append(myImage.transform.DORotate(new Vector3(0, 0, -30f), 0.2f, RotateMode.Fast)).SetEase(Ease.OutQuad);
        sequence.Append(myImage.transform.DORotate(new Vector3(0, 0, 30f), 0.2f, RotateMode.Fast)).SetEase(Ease.OutQuad);

        // Return to the original rotation (0 degrees)
        sequence.Append(myImage.transform.DORotate(new Vector3(0, 0, 0f), 0.2f, RotateMode.Fast)).SetEase(Ease.OutQuad);

        // Scale back to its original size
        sequence.Append(myImage.transform.DOScale(originalScale, 0.5f));
        
        // Repeat the sequence
        sequence.SetLoops(-1); // -1 means infinite loops

        // Start the sequence
        sequence.Play();
    }
}