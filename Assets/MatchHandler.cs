using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchHandler : MonoBehaviour
{
    public static MatchHandler Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ResolveEffect(Block block, int runLength)
    {
        switch (block.blockType)
        {
            case Block.Type.Sword:
                Sword(runLength);
                break;

            case Block.Type.Arrow:
                break;

            case Block.Type.Shield:
                break;

            case Block.Type.Fire:
                break;

            case Block.Type.Water:
                break;

            case Block.Type.Potion:
                break;
        }
    }

    private void Sword(int runLength)
    {
        switch (runLength)
        {
            case 3:
                break;

            case 4:
                break;

            case 5:
                break;
        }
    }
}
