using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastTester : MonoBehaviour
{
    public Camera mainCamera;
    public ContactFilter2D contactFilter;
    public Vector2 lastMouse = Vector2.zero;

    public Transform targetTransform;
    public BoxCollider2D targetCollider;

    public float respawnTimer = 0f;
    public float respawnDuration = 2.5f;

    public Vector2 targetForce = new Vector2(0f, -150f);

    public Vector2 targetMaxVelocity = new Vector2(50f, 50f);
    public Vector2 targetMinVelocity = new Vector2(-50f, -50f);

    public Vector2 targetCurrentVelocity = Vector2.zero;
    public RaycastHit2D[] raycastHits = new RaycastHit2D[3];

    public bool lastHitOk = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;
    }

    public void OnPoint(InputAction.CallbackContext value) {
        lastMouse = value.ReadValue<Vector2>();
    }

    void UpdateVelocity(float dt) {
        targetCurrentVelocity.x = Mathf.MoveTowards(targetCurrentVelocity.x, targetMaxVelocity.x, dt * targetForce.x);
        targetCurrentVelocity.y = Mathf.MoveTowards(targetCurrentVelocity.y, targetMaxVelocity.y, dt * targetForce.y);
    }

    void ClampVelocity() {
        targetCurrentVelocity.x = Mathf.Clamp(targetCurrentVelocity.x, targetMinVelocity.x, targetMaxVelocity.x);
        targetCurrentVelocity.y = Mathf.Clamp(targetCurrentVelocity.y, targetMinVelocity.y, targetMaxVelocity.y);
    }

    void Respawn() {
        targetCurrentVelocity = Vector2.zero;
        targetTransform.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(lastMouse);
        mouseWorld.z = 0f;
        transform.position = mouseWorld;

        respawnTimer += Time.deltaTime;
        if (respawnTimer >= respawnDuration) {
            respawnTimer = 0;
            Respawn();
            return;
        }

        UpdateVelocity(Time.deltaTime);
        UpdateCollision(Time.deltaTime);
    }
    
    void UpdateCollision(float dt) {
        Vector3 move = targetCurrentVelocity * Time.deltaTime;
        int hitCount = targetCollider.Cast(move.normalized, contactFilter, raycastHits, move.magnitude);
        lastHitOk = false;
        for (int i = 0; i < hitCount; i++) {
            if (raycastHits[i].collider == targetCollider) continue;

            transform.position = raycastHits[i].centroid;
            float angle = Vector2.Angle(raycastHits[i].normal, Vector2.up);
            if (angle < 90f) {
                targetCurrentVelocity.y = 0f;
                move.y = 0f;
                lastHitOk = true;
            }
        }

        targetTransform.position += move;
    }

    private void OnDrawGizmos() {
        Gizmos.color = lastHitOk ? Color.green : Color.red;
        Gizmos.DrawCube(targetTransform.position, targetCollider.size);
        //if (testCollider) {
        //    Gizmos.color = boundsOk ? Color.green : Color.red;
        //    Gizmos.DrawCube(testSubject.transform.position, testCollider.size);
        //}
        //for (int i = 0; i < drawBounds.Count; i++) {
        //    Gizmos.DrawCube(drawBounds[i].center, drawBounds[i].size);
        //}
    }
}
