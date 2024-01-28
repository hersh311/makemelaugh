using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoofyScript : MonoBehaviour
{
    private float rotationValue = 0;
    public float scaledValue = 0.5f;

    private void Start()
    {
        //scaledValue = 0.5f;
    }

    void Update()
    {
        rotationValue = Mathf.Clamp(rotationValue, -0.8f, 0.15f);
        scaledValue = Mathf.Clamp01(scaledValue);
        rotationValue = Mathf.Lerp(-0.8f, 0.15f, scaledValue);
        transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y, rotationValue, transform.localRotation.w);
    }

}