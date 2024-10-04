using UnityEngine;
using DG.Tweening;

public class PopEffect : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject; // The GameObject you want to "pop" into view.
    [SerializeField]
    private float popDuration = 0.2f; // Duration of the pop effect.

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = targetObject.transform.localScale;
    }

    public void EnableAndPop()
    {
        targetObject.SetActive(true); // Ensure the object is active.

        // Set the initial scale of the object slightly smaller relative to its original scale.
        targetObject.transform.localScale = originalScale * 0.8f;

        // Create the pop animation.
        Sequence popSequence = DOTween.Sequence();
        popSequence.Append(targetObject.transform.DOScale(originalScale * 1.1f, popDuration).SetEase(Ease.OutQuad)); // Scale up quickly relative to its original scale.
        popSequence.Append(targetObject.transform.DOScale(originalScale, popDuration).SetEase(Ease.OutQuad));   // Then scale back to its original size.
    }

    public void DisableAndPop()
    {
        // Create the pop animation.
        Sequence popSequence = DOTween.Sequence(); // Scale up quickly relative to its original scale.
        popSequence.Append(targetObject.transform.DOScale(originalScale * 1.5f, popDuration).SetEase(Ease.OutQuad)).OnComplete(()=>{targetObject.SetActive(false);});   // Then scale back to its original size.
    }

    public void DisableObject()
    {
        targetObject.SetActive(false);
    }
}