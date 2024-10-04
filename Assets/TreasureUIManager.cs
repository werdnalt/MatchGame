using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureUIManager : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject treasureText;
    [SerializeField] private GameObject treasureDescription;
    
    public void HideUI()
    {
        background.SetActive(false);
        treasureText.SetActive(false);
        treasureDescription.SetActive(false);
    }

    public void EnableUI()
    {
        background.SetActive(true);
        treasureText.SetActive(true);
        treasureDescription.SetActive(true);
    }
}
