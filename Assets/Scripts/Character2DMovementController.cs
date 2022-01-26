using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Collider2D))]
public class Character2DMovementController : GenericCharacter2DMovementController
{
    [SerializeField] public bool slopes = true;
    [SerializeField] public Vector2 velocity = Vector2.zero;
    [SerializeField] public Vector2 groundNormal = Vector2.up;

    [SerializeField] public float lastDashDir = 0f;
    [SerializeField] public bool isReversing = false;

    [SerializeField] public int horizontalHitCount = 0;
    [SerializeField] public float horizontalHitAngle = 0f;
    [SerializeField] public RaycastHit2D[] horizontalRaycastHits = new RaycastHit2D[2];

    [SerializeField] public int verticalHitCount = 0;
    [SerializeField] public float verticalHitAngle = 0f;
    [SerializeField] public RaycastHit2D[] verticalRaycastHits = new RaycastHit2D[2];

    [SerializeField] public Collider2D[] overlapColliders = new Collider2D[4];


    [SerializeField] public bool debugMovement = false;
    [SerializeField] public float debugMovementDuration = 1.5f;

    [SerializeField] public List<Character2DMovementAbility> movementAbilities = new List<Character2DMovementAbility>();

    public SquashStretchController squashStretchController;
    public GameObject[] contactObjects = new GameObject[3];
    public int contactObjectCount = 0;
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

    public override void SetInputState(InputState input)
    {
        base.SetInputState(input);
        foreach (var ability in movementAbilities)
        {
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

    public override void Awake() {
        base.Awake();
        foreach (var ability in movementAbilities) {
            ability.Controller = this;
        }
    }

    private void Start() {
        // sort movement abilities by sort order ???
        movementAbilities.Sort((a, b) => a.SortOrder - b.SortOrder);
    }

    public override void Update() {
        base.Update();

        if (shouldRespawn) {
            shouldRespawn = false;
            respawnEvent.Invoke();
            transform.position = respawn.position;
            velocity = Vector2.zero;
        } else {
            UpdateMovement(Time.deltaTime);
        }
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
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // horizontal move
        Vector2 horizontalDirection = Vector2.right * Mathf.Sign(move.x);
        horizontalHitCount = collider.Cast(horizontalDirection, colliderContactFilter, horizontalRaycastHits, Mathf.Abs(move.x));
        for (int i = 0; i < horizontalHitCount; i++)
        {
            if (horizontalRaycastHits[i].collider == collider) continue;
            horizontalHitAngle = Vector2.Angle(horizontalRaycastHits[i].normal, Vector2.up);
            Vector2 projectedPosition = horizontalDirection * horizontalRaycastHits[i].distance;

            bool newContact = false;
            if (horizontalRaycastHits[i].point.x > transform.position.x && velocity.x > 0f)
            {
                // Right side contact
                Debug.Log("Hit right wall");
                newContact = true;
            } else if (horizontalRaycastHits[i].point.x < transform.position.x && velocity.x < 0f)
            {
                Debug.Log("Hit left wall");
                newContact = true;
            }

            if (newContact)
            {
                contactObjects[contactObjectCount] = horizontalRaycastHits[i].collider.gameObject;
                contactObjectCount++;
                velocity.x = 0f;
                Debug.DrawRay(transform.position, horizontalRaycastHits[i].normal, Color.red);
            }

            Debug.DrawRay(verticalRaycastHits[i].point, verticalRaycastHits[i].normal, Color.blue);
        }

        // vertical move
        Vector2 verticalDirection = Vector2.up * Mathf.Sign(move.y);
        verticalHitCount = collider.Cast(verticalDirection, colliderContactFilter, verticalRaycastHits, Mathf.Abs(move.y));
        for (int i = 0; i < verticalHitCount; i++)
        {
            if (horizontalRaycastHits[i].collider == collider) continue;
            verticalHitAngle = Vector2.Angle(verticalRaycastHits[i].normal, Vector2.up);
            Vector2 projectedPosition = verticalDirection * horizontalRaycastHits[i].distance;

            bool newContact = false;
            if (verticalRaycastHits[i].point.y > transform.position.y && velocity.y > 0f)
            {
                Debug.Log("Hit ceiling");
                newContact = true;
            } else if (verticalRaycastHits[i].point.y < transform.position.y && velocity.y < 0f)
            {
                isGrounded = true;
                newContact = true;
                groundNormal = verticalRaycastHits[i].normal;
            }

            if (newContact)
            {
                contactObjects[contactObjectCount] = verticalRaycastHits[i].collider.gameObject;
                contactObjectCount++;
                velocity.y = 0f;
                Debug.DrawRay(transform.position, verticalRaycastHits[i].normal, Color.green);
            }

            Debug.DrawRay(verticalRaycastHits[i].point, verticalRaycastHits[i].normal, Color.blue);
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
        transform.position = position + (Vector3)velocity * deltaTime;
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
            groundNormal = groundNormal,
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
        groundNormal = state.groundNormal;
        moveInput = state.moveInput;
        isReversing = state.isReversing;
        colliderContactFilter = state.colliderContactFilter;
        overlapColliders = state.overlapColliders;
    }
}
