using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Character2DMovementState
{
    [SerializeField] public Vector3 position;
    [SerializeField] public Vector2 velocity;
    [SerializeField] public bool isGrounded;
    [SerializeField] public Collider2D groundContact;
    [SerializeField] public Vector2 moveInput;
    [SerializeField] public bool isReversing;
    [SerializeField] public ContactFilter2D colliderContactFilter;
    [SerializeField] public Collider2D[] overlapColliders;
}

[RequireComponent(typeof(Collider2D))]
public class Character2DMovementController : MonoBehaviour
{
    [SerializeField] private Character2DMovementSettings movementSettings;
    public Character2DMovementSettings Settings { get => movementSettings; }

    public Bounds worldBounds = new Bounds();
    public Transform respawn;

    [SerializeField] public Vector2 velocity = Vector2.zero;

    [SerializeField] public bool isGrounded = false;
    [SerializeField] public Collider2D groundContact = null;

    [SerializeField] public Vector2 moveInput = Vector2.zero;

    [SerializeField] public float lastDashDir = 0f;
    [SerializeField] public bool isReversing = false;
    [SerializeField] public ContactFilter2D colliderContactFilter;

    [SerializeField] public int hitCount = 0;
    [SerializeField] public Collider2D[] overlapColliders = new Collider2D[4];
    [SerializeField] public float[] hitAngles = new float[4];
    [SerializeField] public RaycastHit2D[] raycastHits = new RaycastHit2D[4];

    [SerializeField] public bool disableRespawn = false;
    [SerializeField] public float lastHitAngle = 0f;
    [SerializeField] public float lastSignedHitAngle = 0f;

    [SerializeField] public bool debugMovement = false;
    [SerializeField] public float debugMovementDuration = 1.5f;

    [SerializeField] public InputState inputState;

    [SerializeField] public List<Character2DMovementAbility> movementAbilities = new List<Character2DMovementAbility>();

    private new Transform transform;
    private new BoxCollider2D collider;
    private new Rigidbody2D rigidbody;

    public GameObject[] contactObjects = new GameObject[3];
    public int contactObjectCount = 0;

    private bool shouldRespawn = false;
    public UnityEvent respawnEvent = new UnityEvent();

    /**
     * Mechanic: Jump After Ledge
     * Rule: The player can jump if they are no longer grounded within a grace period
     *       The grace period only applies if the player did not jump to leave the grounded state
     *       
     * Mechanic: Hold to Jump Further
     * Rule: The player can hold down the jump button to keep an increase cap on their airborne velocity
     *       Releasing the jump button eases the max velocity down to the regular cap
     *       
     * Mechanic: Ground Dash
     *  Activate: Dash Button (Gamepad East / Keyboard Shift)
     *  Requires: Not already dashing, moving in a direction, on the ground
     *  Affords:  The player's target x velocity is increased
     *  Ends:     The player releases dash or leaves the ground
     * 
     * Mechanic: Air Dash
     *  Activate: Dash button (Gamepad East / Keyboard Shift)
     *  Requires: Not already dashing, moving in a direction, in the air
     *  Affords:  The player's velocity is boosted in the aimed direction
     *  Ends:     The player lands
     */

    /**
    * Ground Check Logic
    * 
    * apply gravity
    * set isGrounded false
    * 
    * project movement with velocity
    * for each hit
    *   if hit is below
    *       set isGrounded true
    *       set velocity y 0
    * 
    */

    /*
    * Movement Steps
    * - Update Input State
    * - Pre-Movement
    *       Do any pre-movement logic (none in core)
    *
    * - Apply Forces
    *       Update the projected velocity by adding forces
    * 
    * - Restrict Forces
    *       Update the projected velocity by applying restrictions
    * 
    * - Project for collisions
    *       Cast based on projected velocity
    *       React to collisions
    * 
    * - Apply final movement
    * - Post-movement
    *       
    *
    */

    public Vector2 MoveInput { get => moveInput; set => moveInput = value; }
    public float MoveInputX { get => moveInput.x; set => moveInput.x = value; }
    public float MoveInputY { get => moveInput.y; set => moveInput.y = value; }

    public void SetInputState(InputState input) {
        moveInput.x = input.moveInput.x;
        foreach (var ability in movementAbilities) {
            ability.SetInputState(input);
        }
    }

    public void TeleportTo(Vector3 position) {
        if (CheckOverlap(position, false) == false) {
            transform.position = position;
        }
    }

    public bool CheckOverlap(Vector3 position, bool useTriggers = false) {
        int count = Physics2D.OverlapBox(collider.bounds.center, collider.bounds.size, 0f, colliderContactFilter, overlapColliders);
        if (count > 1) {
            Debug.Log("CheckOverlap Collided with " + overlapColliders[0].name, this);
            return true;
        }

        return false;
    }

    private void Awake() {
        transform = GetComponent<Transform>();
        collider = GetComponent<BoxCollider2D>();

        // colliderLayerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
        contactFilter.useTriggers = false;
        colliderContactFilter = contactFilter;

        foreach (var ability in movementAbilities) {
            ability.Controller = this;
        }
    }

    private void Start() {
        // sort movement abilities by sort order ???
        movementAbilities.Sort((a, b) => a.SortOrder - b.SortOrder);
    }

    private void Update() {
        if (worldBounds.Contains(transform.position) == false && !disableRespawn) {
            shouldRespawn = true;
        }

        if (shouldRespawn) {
            shouldRespawn = false;
            respawnEvent.Invoke();
            transform.position = respawn.position;
            velocity = Vector2.zero;
        } else {
            UpdateMovement(Time.deltaTime);
        }
    }

    public void Respawn() {
        if (!disableRespawn) shouldRespawn = true;
    }

    public void UpdateMovement(float deltaTime) {
        UpdatePreMovement(deltaTime);
        
        Vector3 lastPos = transform.position;
        Vector2 vel = velocity;
        Vector3 pos = transform.position;
        Vector2 targetVelocity = Vector2.zero;
        Vector2 changeSpeed = Vector2.zero;
        Vector2 minVelocity = Vector2.negativeInfinity;
        Vector2 maxVelocity = Vector2.positiveInfinity;

        UpdateTargetVelocities(deltaTime, ref vel, ref targetVelocity, ref changeSpeed, ref minVelocity, ref maxVelocity);

        UpdateVelocity(deltaTime, ref vel, targetVelocity, changeSpeed);
        ClampVelocity(deltaTime, ref vel, ref minVelocity, ref maxVelocity);
        UpdateCollision(deltaTime, ref vel, ref pos);

        UpdateTransform(deltaTime, pos, vel);
        UpdatePostMovement(deltaTime);

        for (int i = 0; i < contactObjectCount; i++) {
            contactObjects[i].SendMessage("UpdateContact", this, SendMessageOptions.DontRequireReceiver);
        }

        if (debugMovement) {
            Debug.DrawLine(lastPos, transform.position, Color.yellow, debugMovementDuration);
        }
    }

    private void UpdatePreMovement(float deltaTime) {
        foreach (var ability in movementAbilities) {
            ability.UpdatePreMovement(deltaTime);
        }
    }

    private void UpdateTargetVelocities(float deltaTime, ref Vector2 currentVelocity, ref Vector2 targetVelocity, ref Vector2 changeSpeed, ref Vector2 minVelocity, ref Vector2 maxVelocity) {
        // default gravity target
        targetVelocity.y = movementSettings.minVelocityY;
        changeSpeed -= movementSettings.gravity * Time.deltaTime;

        // default min
        minVelocity.y = movementSettings.minVelocityY;
        minVelocity.x = isGrounded ? -movementSettings.groundMaxVelocityX : -movementSettings.airMaxVelocityX;

        // default max
        maxVelocity.y = Mathf.Infinity;
        maxVelocity.x = isGrounded ? movementSettings.groundMaxVelocityX : movementSettings.airMaxVelocityX;

        // movement targets
        float brakeForce = isGrounded ? movementSettings.groundStopDecelerationX : movementSettings.airStopDecelerationX;
        if (moveInput.x != 0) {
            float maxTargetVelocity = isGrounded ? movementSettings.groundMaxVelocityX : movementSettings.airMaxVelocityX;
            float reverseForce = isGrounded ? movementSettings.groundReverseAccelerationX : movementSettings.airReverseAccelerationX;
            float targetMove = maxTargetVelocity * moveInput.x;
            isReversing = Mathf.Sign(moveInput.x) != Mathf.Sign(velocity.x);

            float moveSpeed = isReversing ? reverseForce : brakeForce;
            targetVelocity.x += targetMove;
            changeSpeed.x += moveSpeed * Time.deltaTime;
        } else {
            changeSpeed.x = brakeForce * Time.deltaTime;
            targetVelocity.x = 0f;
            isReversing = false;
        }


        foreach (var ability in movementAbilities) {
            ability.UpdateTargetVelocities(deltaTime, ref currentVelocity, ref targetVelocity, ref changeSpeed, ref minVelocity, ref maxVelocity);
        }
    }

    private void UpdateVelocity(float deltaTime, ref Vector2 velocity, Vector2 targetVelocity, Vector2 changeSpeed) {
        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocity.x, changeSpeed.x);
        velocity.y = Mathf.MoveTowards(velocity.y, targetVelocity.y, changeSpeed.y);

        //foreach (var ability in movementAbilities) {
        //    ability.UpdateVelocity(deltaTime, ref velocity, ref minVelocity, ref maxVelocity);
        //}
    }

    private void ClampVelocity(float deltaTime, ref Vector2 velocity, ref Vector2 minVelocity, ref Vector2 maxVelocity) {
        velocity.x = Mathf.Clamp(velocity.x, minVelocity.x, maxVelocity.x);
        velocity.y = Mathf.Clamp(velocity.y, minVelocity.y, maxVelocity.y);
    }

    private void UpdateCollision(float deltaTime, ref Vector2 velocity, ref Vector3 position) {
        contactObjectCount = 0;
        Vector3 move = velocity * Time.deltaTime;
        hitCount = collider.Cast(move.normalized, colliderContactFilter, raycastHits, move.magnitude);

        bool wasGrounded = isGrounded;
        isGrounded = false;
        for (int i = 0; i < hitCount; i++) {
            if (raycastHits[i].collider == collider) continue;

            hitAngles[i] = Vector2.Angle(raycastHits[i].normal, Vector2.up);
            lastHitAngle = hitAngles[i];

            // Move up through ground
            if (raycastHits[i].collider is EdgeCollider2D) {
                if (hitAngles[i] > 90f && velocity.y > 0f) {
                    continue;
                }
            }

            // Hit something solid
            position = raycastHits[i].centroid;
            if (hitAngles[i] == 180f || hitAngles[i] == 0f) {
                // Ground or ceiling
                if (raycastHits[i].collider is EdgeCollider2D) {
                    // Is edge, allow passthrough
                    if (velocity.y <= 0f) {
                        isGrounded = true;
                        contactObjects[contactObjectCount] = raycastHits[i].collider.gameObject;
                        contactObjectCount++;
                        velocity.y = 0f;
                    }
                } else {
                    if (velocity.y <= 0f && raycastHits[i].point.y < transform.position.y) {
                        // Is falling and below
                        isGrounded = true;
                        contactObjects[contactObjectCount] = raycastHits[i].collider.gameObject;
                        contactObjectCount++;
                        velocity.y = 0f;
                    } else if (velocity.y >= 0f && raycastHits[i].point.y > transform.position.y) {
                        // Is rising and above
                        velocity.y = 0f;
                        contactObjects[contactObjectCount] = raycastHits[i].collider.gameObject;
                        contactObjectCount++;
                    }
                }
            } else if (hitAngles[i] == 90f) {
                if (raycastHits[i].point.x > transform.position.x && velocity.x > 0f) {
                    // to the right
                    velocity.x = 0f;
                    contactObjects[contactObjectCount] = raycastHits[i].collider.gameObject;
                    contactObjectCount++;
                } else if (raycastHits[i].point.x < transform.position.x && velocity.x < 0f) {
                    // to the left
                    velocity.x = 0f;
                    contactObjects[contactObjectCount] = raycastHits[i].collider.gameObject;
                    contactObjectCount++;
                }
            }
        }

        if (!isGrounded && wasGrounded) {
            foreach (var ability in movementAbilities) {
                ability.OnFalling(deltaTime, ref velocity, ref position);
            }
        } else if (isGrounded && !wasGrounded) {
            foreach (var ability in movementAbilities) {
                ability.OnGrounded(deltaTime, ref velocity, ref position);
            }
        }
    }

    private void UpdateTransform(float deltaTime, Vector3 position, Vector2 velocity) {
        transform.position = position + (Vector3)(velocity * deltaTime);
        this.velocity = velocity;
    }

    private void UpdatePostMovement(float deltaTime) {
        foreach (var ability in movementAbilities) {
            ability.UpdatePostMovement(deltaTime);
        }
    }

    public Character2DMovementState ToState() {
        return new Character2DMovementState() {
            position = transform.position,
            velocity = velocity,
            isGrounded = isGrounded,
            groundContact = groundContact,
            moveInput = moveInput,
            isReversing = isReversing,
            colliderContactFilter = colliderContactFilter,
            overlapColliders = overlapColliders,
        };
    }

    public void FromState(Character2DMovementState state) {
        transform.position = state.position;
        velocity = state.velocity;
        isGrounded = state.isGrounded;
        groundContact = state.groundContact;
        moveInput = state.moveInput;
        isReversing = state.isReversing;
        colliderContactFilter = state.colliderContactFilter;
        overlapColliders = state.overlapColliders;
    }

    public void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
    }
}
