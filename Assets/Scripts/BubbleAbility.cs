using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleAbility : Character2DMovementAbility
{
    public enum EBubbleState
    {
        NONE,
        ENTER,
        STAY,
        LEAVE
    }

    public override int SortOrder { get => sortOrder; }
    public override Character2DMovementController Controller { get => controller; set => controller = value; }

    [SerializeField] public int sortOrder = 0;
    [SerializeField] private Character2DMovementController controller;

    [SerializeField] private bool jumpInput = false;
    [SerializeField] private Vector2 moveInput = Vector2.zero;

    [SerializeField] public Bubble activeBubble;
    [SerializeField] public EBubbleState state = EBubbleState.NONE;

    [SerializeField] public float enterDelay = 0.2f;
    [SerializeField] public float enterTimer = 0f;

    [SerializeField] public float leaveDelay = 0.2f;
    [SerializeField] private float leaveTimer = 0f;


    public override void SetInputState(InputState inputState) {
        moveInput = inputState.moveInput;
        jumpInput = inputState.jumpIsDown;
    }

    public override void UpdatePreMovement(float deltaTime)
    {
        if (state == EBubbleState.NONE)
        {
            controller.ignorePush = false;
        } else
        {
            controller.ignorePush = true;
        }
    }

    private void UpdateEnteringBubble(float deltaTime)
    {
        if (!activeBubble)
        {
            state = EBubbleState.LEAVE;
        }

        if (enterTimer == 0)
        {
            Debug.Log("Bubble Enter Start");
        }

        enterTimer += deltaTime;
        if (enterTimer >= enterDelay)
        {
            enterTimer = 0f;
            state = EBubbleState.STAY;
            Debug.Log("Bubble Enter End");
        }
    }

    private void UpdateInBubble(float deltaTime)
    {
        
        if (moveInput.magnitude > 0.1f) leaveTimer += deltaTime;
        else leaveTimer = 0f;

        if (leaveTimer >= leaveDelay || jumpInput || !activeBubble)
        {
            leaveTimer = 0f;
            Controller.isGrounded = true;
            state = EBubbleState.LEAVE;
            Debug.Log("Bubble Leave Start");
        }
        else
        {
            Controller.transform.position = activeBubble.transform.position;
        }
    }

    private void UpdateLeavingBubble(float deltaTime)
    {
        if (!activeBubble)
        {
            Debug.Log("Bubble Leave End");
            state = EBubbleState.NONE;
        }
    }

    public override void UpdatePostMovement(float deltaTime) {
        switch (state)
        {
            case EBubbleState.ENTER:
                UpdateEnteringBubble(deltaTime);
                break;
            case EBubbleState.STAY:
                UpdateInBubble(deltaTime);
                break;
            case EBubbleState.LEAVE:
                UpdateLeavingBubble(deltaTime);
                break;
            case EBubbleState.NONE:
                if (activeBubble) state = EBubbleState.ENTER;
                break;
        }
    }

    public override void UpdateTargetVelocities(float deltaTime, ref Vector2 currentVelocity, ref Vector2 targetVelocity, ref Vector2 changeSpeed, ref Vector2 minVelocity, ref Vector2 maxVelocity) {
        if (state == EBubbleState.STAY)
        {
            currentVelocity = Vector2.zero;
            targetVelocity = Vector2.zero;
            changeSpeed = Vector2.zero;
            minVelocity = Vector2.zero;
            maxVelocity = Vector2.zero;
        } else if (state == EBubbleState.LEAVE)
        {
            targetVelocity = controller.Settings.airMaxVelocityX * moveInput;
            changeSpeed = controller.Settings.airMoveAccelerationX * moveInput;
            minVelocity = Vector2.zero;
            maxVelocity = controller.Settings.airMaxVelocityX * moveInput;
        }
    }
}
