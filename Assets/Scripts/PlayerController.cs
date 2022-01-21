using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public enum EInputSnap
{
    NONE = 0,
    DPAD = 1,
    GEN = 2,
}

public class PlayerController : MonoBehaviour
{
    public Animator squashStretchAnimator;
    public SquashStretchController squashStretchController;
    public SpriteRenderer spriteRenderer;
    public Character2DMovementController movement;
    public Animator playerAnimator;
    public InputState lastInputState;
    public EInputSnap inputSnapMode;

    public bool groundedLastFrame = true;
    public float inputAngle = 0f;
    public Vector2 adjustedInput = Vector2.zero;

    private void Update() {
        playerAnimator.SetBool("moving", movement.MoveInputX != 0);
        movement.SetInputState(lastInputState);
        if (movement.MoveInputX != 0) {
            spriteRenderer.flipX = movement.MoveInputX < 0f;
        }

        if (groundedLastFrame != movement.isGrounded) {
            if (movement.isGrounded) {
                squashStretchAnimator.SetBool("fall", false);
                //squashStretchController.ResetScale();
                //squashStretchController.BounceVertSquash();
            } else {
                squashStretchAnimator.SetBool("fall", true);
                //squashStretchController.ResetScale();
                //squashStretchController.HorizSquash();
            }
        }

        groundedLastFrame = movement.isGrounded;
    }

    public void OnMove(InputAction.CallbackContext value) {
        value.
        lastInputState.rawMoveInput = value.ReadValue<Vector2>();
        lastInputState.moveInput = GetSnapInput(lastInputState.rawMoveInput);
    }

    public Vector2 GetSnapInput(Vector2 source) {
        float mag = source.magnitude;
        inputAngle = Vector2.SignedAngle(Vector2.up, lastInputState.rawMoveInput);
        switch (inputSnapMode) {
            case EInputSnap.NONE: return source;
            case EInputSnap.DPAD:
                inputAngle = Mathf.Ceil(inputAngle / 22.5f) * 22.5f;
                adjustedInput = new Vector2(-Mathf.Sin(inputAngle * Mathf.Deg2Rad), Mathf.Cos(inputAngle * Mathf.Deg2Rad));
                return adjustedInput * mag;
            case EInputSnap.GEN:
                inputAngle = Mathf.Ceil(inputAngle / 15f) * 15f;
                adjustedInput = new Vector2(-Mathf.Sin(inputAngle * Mathf.Deg2Rad), Mathf.Cos(inputAngle * Mathf.Deg2Rad));
                return adjustedInput * mag;
            default: return source;
        }
    }

    public void OnJump(InputAction.CallbackContext value) {
        if (value.started) {
            lastInputState.jumpIsDown = true;
        } else if (value.canceled) {
            playerAnimator.SetBool("jumping", false);
            if (value.canceled) {
                lastInputState.jumpIsDown = false;
            }
        }
    }

    public void OnDash(InputAction.CallbackContext value) {
        if (value.started) {
            lastInputState.dashIsDown = true;
        } else if (value.canceled) {
            lastInputState.dashIsDown = false;
        }
    }
}
