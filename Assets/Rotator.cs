using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{

    [SerializeField] public float rate = 2f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward, rate * Time.deltaTime);
    }
}
