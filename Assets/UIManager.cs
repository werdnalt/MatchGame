using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<GameObject> characterUIsGameObjects;
    private List<CharacterUI> _characterUIs = new List<CharacterUI>();

}
