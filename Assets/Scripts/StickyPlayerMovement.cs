using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StickyPlayerMovement : GenericCharacter2DMovementController
{
    [SerializeField] public StickyContainer container;
    [SerializeField] public float slideForce = 1f;
    [SerializeField] public Vector2 velocity = Vector2.zero;
    [SerializeField] public float slideStartAngle = 40f;
    [SerializeField] public float relativeSlideForce = 1f;
    [SerializeField] public float gravityDotProduct = 0f;
    [SerializeField] public Vector3 lastPosition = Vector3.zero;
    [SerializeField] private float debugLineDuration = 60f;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        float dot = Vector2.Dot(Vector2.down, container.groundNormal);
        gravityDotProduct = 1 - Mathf.Abs(dot);
        Debug.DrawRay(transform.position, Vector2.down * gravityDotProduct, Color.yellow);

        if (container.quadrant.y > 0f)
        {
            velocity += Vector2.right * gravityDotProduct * slideForce * Time.deltaTime;
        } else
        {
            velocity += Vector2.left * gravityDotProduct * slideForce * Time.deltaTime;
        }

        transform.position += transform.right * velocity.x * Time.deltaTime;

        //if (lookNormal.x < 0f)
        //{
        //    if (lookNormal.y > 0f)
        //    {
        //        // 0 to 90
        //        groundSlideRight = 1f;
        //        adjustedGroundAngle = groundAngle;
        //        Debug.Log("Top-Left");
        //    } else
        //    {
        //        // 270 to 360
        //        groundSlideRight = -1f;
        //        adjustedGroundAngle = groundAngle;
        //        Debug.Log("Bottom-Left");
        //    }
        //} else
        //{
        //    if (lookNormal.y < 0f)
        //    {
        //        // 180 to 270
        //        groundSlideRight = -1f;
        //        adjustedGroundAngle = 180f - groundAngle;
        //        Debug.Log("Bottom-Right");
        //    } else
        //    {
        //        // 90 to 180
        //        groundSlideRight = 1f;
        //        adjustedGroundAngle = 180f - groundAngle;
        //        Debug.Log("Top-Right");
        //    }
        //}

        //if (adjustedGroundAngle > slideStartAngle)
        //{
        //    groundAnglePercent = Mathf.Clamp01((adjustedGroundAngle - slideStartAngle) / (90f - slideStartAngle));
        //    transform.position += transform.right * groundSlideRight * (groundAnglePercent * relativeSlideForce) * Time.deltaTime;
        //}

        Debug.DrawLine(transform.position, lastPosition, Color.red, debugLineDuration);
        lastPosition = transform.position;
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
}
