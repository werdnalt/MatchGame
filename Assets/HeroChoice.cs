using AllIn1SpringsToolkit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroChoice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private HeroChoiceManager heroChoiceManager;
    [SerializeField] private Unit heroUnit;
    [SerializeField] private Image heroPortrait;

    private Coroutine _greyscaleCoroutine;
    private bool _isChosen = false;
    private Vector3 _originalScale;
    private TransformSpringComponent _heroSpringComponent;
    private Transform _originalSpringTransformTarget;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _heroSpringComponent = GetComponent<TransformSpringComponent>();
        _originalSpringTransformTarget = _heroSpringComponent.targetTransform;
        heroPortrait.sprite = heroUnit.unitSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // punch scale up
        var newScale = new Vector3(_originalScale.x * 1.1f, _originalScale.y * 1.1f, _originalScale.z * 1.1f);
        _heroSpringComponent.SetCurrentValueScale(newScale);
        
        UIManager.Instance.ShowUnitPanel(unit: heroUnit);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _heroSpringComponent.SetTargetScale(_originalScale);
        
        // if hero is selected, return
        if (_isChosen) return;

        // Stop the greyscale flash when the pointer exits
        if (_greyscaleCoroutine != null)
        {
            StopCoroutine(_greyscaleCoroutine);
            _greyscaleCoroutine = null;
        }
        
        UIManager.Instance.HideUnitPanel();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isChosen)
        {
            // remove hero from chosen heroes
            heroChoiceManager.RemoveHero(heroUnit);
            ResetToOriginalTargetTransform();

            _isChosen = false;
            UIManager.Instance.HideUnitPanel();
        }
        else
        {
            // try to add hero
            var ableToAddHero = heroChoiceManager.TryToAddHero(heroUnit, _heroSpringComponent);
            if (ableToAddHero)
            {
                UIManager.Instance.HideUnitPanel();
                _isChosen = true;
            }
        }
    }

    public void ResetToOriginalTargetTransform()
    {
        _heroSpringComponent.targetTransform = _originalSpringTransformTarget;
    }
}
