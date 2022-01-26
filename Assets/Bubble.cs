using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.up;
    public float moveSpeed = 5f;
    public float despawnAfter = 10f;

    private float despawnTimer = 0f;

    public void Update()
    {
        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnAfter)
        {
            Destroy(gameObject);
        } else
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }
}
