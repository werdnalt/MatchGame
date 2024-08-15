using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public float duration = 0.05f; // Duration of the shake effect

        // Start the shake effect with the specified magnitude
        public void Shake(float magnitude)
        {
            StartCoroutine(ShakeCamera(magnitude));
        }

        private IEnumerator ShakeCamera(float magnitude)
        {
            Vector3 originalPosition = transform.localPosition;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(x, y, originalPosition.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPosition;
        }
}
