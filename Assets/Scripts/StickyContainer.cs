using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyContainer : MonoBehaviour
{
    [SerializeField] public float worldRotateSpeed = 12f;
    [SerializeField] public float groundAngle = 0f;
    [SerializeField] public float adjustedGroundAngle = 0f;
    [SerializeField] public float groundAnglePercent = 0f;
    [SerializeField] public float groundSlideRight = 0f;
    [SerializeField] public Vector2 quadrant = Vector2.zero;
    [SerializeField] public Vector2 gravityNormal = Vector2.up;
    [SerializeField] public Vector2 groundNormal = Vector2.up;

    private void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.forward, -worldRotateSpeed * Time.deltaTime);
        Vector2 normalizedPos = transform.position.normalized;

        groundNormal = transform.up;
        groundAngle = Vector2.Angle(gravityNormal, groundNormal);

        quadrant.Set(normalizedPos.x >= 0 ? 1f : -1f, normalizedPos.y >= 0 ? 1f : -1f);
    }
}
