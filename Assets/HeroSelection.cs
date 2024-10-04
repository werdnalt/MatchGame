using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HeroSelection : MonoBehaviour
{
    public Vector3[] heroPositions;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TestPath());
    }
    

    private IEnumerator TestPath()
    {
        yield return new WaitForSeconds(2f);
        transform.DOPath(heroPositions, 2f, PathType.CubicBezier, PathMode.TopDown2D).SetEase(Ease.OutQuad);
    }
}
