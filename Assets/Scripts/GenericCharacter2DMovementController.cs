using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Character2DMovementState
{
    [SerializeField] public Vector3 position;
    [SerializeField] public Vector2 velocity;
    [SerializeField] public bool isGrounded;
    [SerializeField] public Vector2 groundNormal;
    [SerializeField] public Collider2D groundContact;
    [SerializeField] public Vector2 moveInput;
    [SerializeField] public bool isReversing;
    [SerializeField] public ContactFilter2D colliderContactFilter;
    [SerializeField] public Collider2D[] overlapColliders;
}

public class GenericCharacter2DMovementController : MonoBehaviour
{

    [SerializeField] public Vector2 moveInput = Vector2.zero;

    [SerializeField] public bool isGrounded = false;
    [SerializeField] public Transform respawn;
    [SerializeField] public Bounds worldBounds = new Bounds();
    [SerializeField] public bool disableRespawn = false;
    [SerializeField] protected bool shouldRespawn = false;
    [SerializeField] public ContactFilter2D colliderContactFilter;
    [SerializeField] protected Character2DMovementSettings movementSettings;

    public Character2DMovementSettings Settings { get => movementSettings; }

    public Vector2 MoveInput { get => moveInput; set => moveInput = value; }
    public float MoveInputX { get => moveInput.x; set => moveInput.x = value; }
    public float MoveInputY { get => moveInput.y; set => moveInput.y = value; }


    protected new Transform transform;
    protected new BoxCollider2D collider;
    protected new Rigidbody2D rigidbody;


    public virtual void Awake()
    {
        transform = GetComponent<Transform>();
        collider = GetComponent<BoxCollider2D>();

        // colliderLayerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
        contactFilter.useTriggers = false;
        colliderContactFilter = contactFilter;
    }

    public virtual void Update()
    {
        if (worldBounds.Contains(transform.position) == false && !disableRespawn)
        {
            shouldRespawn = true;
        }
    }

    public virtual void SetInputState(InputState input)
    {
        moveInput.x = input.moveInput.x;
    }

    public virtual void Respawn()
    {
        if (!disableRespawn) shouldRespawn = true;
    }

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(worldBounds.center, worldBounds.size);
    }
}