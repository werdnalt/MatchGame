using System;
using System.Collections;
using System.Collections.Generic;
using AllIn1SpringsToolkit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroChoice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image heroImage;
    [SerializeField] private HeroChoiceManager heroChoiceManager;
    [SerializeField] private TransformSpringComponent heroSpringComponent;
    [SerializeField] private Unit heroUnit;

    private Coroutine _greyscaleCoroutine;
    private bool _isChosen = false;
    private Vector3 _originalScale;
    private Material _heroMat;
    private const string GREYSCALE = "_GreyscaleBlend";

    private void Awake()
    {
        _originalScale = transform.localScale;
        _heroMat = new Material(heroImage.material);
        heroImage.material = _heroMat;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // punch scale up
        var newScale = new Vector3(_originalScale.x * 1.1f, _originalScale.y * 1.1f, _originalScale.z * 1.1f);
        heroSpringComponent.SetTargetScale(newScale);
    
        // show hero information
        heroChoiceManager.ShowTooltip(heroUnit);
    
        // if not a chosen hero, start flashing greyscale
        if (!_isChosen && heroChoiceManager.IsRoomForHero())
        {
            _greyscaleCoroutine = StartCoroutine(GreyscaleFlash());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        heroSpringComponent.SetTargetScale(_originalScale);
        heroChoiceManager.HideTooltip();
        
        // if hero is selected, return
        if (_isChosen) return;

        // Stop the greyscale flash when the pointer exits
        if (_greyscaleCoroutine != null)
        {
            StopCoroutine(_greyscaleCoroutine);
            _greyscaleCoroutine = null;
        }
        
        // turn to solid greyscale
        _heroMat.SetFloat(GREYSCALE, 1);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isChosen)
        {
            // remove hero from chosen heroes
            heroChoiceManager.RemoveHero(heroUnit);
            
            _heroMat.SetFloat(GREYSCALE, 1);

            _isChosen = false;
        }
        else
        {
            // try to add hero
            var ableToAddHero = heroChoiceManager.TryToAddHero(heroUnit);
            if (ableToAddHero)
            {
                // if added, change to full color
                _heroMat.SetFloat(GREYSCALE, 0); 
                
                _isChosen = true;
            }
        }
        
        StopCoroutine(_greyscaleCoroutine);
    }

    private IEnumerator GreyscaleFlash()
    {
        Debug.Log("Greyscale flash");
        {
            var timeToTransition = 0.5f; // Time to transition from full color to greyscale or vice versa
            float elapsedTime = 0f;
            bool isIncreasing = false; // Whether we are going towards full color or full greyscale

            while (true) // Continue while the cursor is on the GameObject
            {
                // While the user is still hovering, oscillate between greyscale and color
                elapsedTime += Time.deltaTime;

                // Oscillate the greyscale value over time
                float t = elapsedTime / timeToTransition;
                if (isIncreasing)
                {
                    _heroMat.SetFloat(GREYSCALE, Mathf.Lerp(0f, 1f, t)); // From full color (0) to full greyscale (1)
                }
                else
                {
                    _heroMat.SetFloat(GREYSCALE, Mathf.Lerp(1f, 0f, t)); // From full greyscale (1) to full color (0)
                }

                // If we have completed the transition
                if (t >= 1f)
                {
                    elapsedTime = 0f; // Reset elapsed time for the next transition
                    isIncreasing = !isIncreasing; // Switch direction (toggle between increasing and decreasing greyscale)
                }

                yield return null; // Wait for the next frame
            }
        }
    }
}
