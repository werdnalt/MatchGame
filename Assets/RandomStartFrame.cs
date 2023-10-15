using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomStartFrame : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing!");
            return;
        }

        // Assuming the animation is on layer 0 and is the default animation
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float randomTime = Random.Range(0f, state.length);

        animator.Play(state.fullPathHash, -1, randomTime);
    }
}
