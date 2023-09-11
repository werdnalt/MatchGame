using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public List<Unit> allHeroes;
    private List<Unit> _benchedHeroes;
    private List<Unit> _activateHeroes;
    private List<Unit> _deadHeroes;
}
