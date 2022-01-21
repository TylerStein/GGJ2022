using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDashAbility : Character2DMovementAbility
{
    public override int SortOrder { get => sortOrder; }
    public override Character2DMovementController Controller { get => controller; set => controller = value; }

    [SerializeField] public int sortOrder = 0;
    [SerializeField] private Character2DMovementController controller;

    [SerializeField] private Vector2 moveInput = Vector2.zero;
    [SerializeField] private bool dashInput = false;
    [SerializeField] private bool dashInputDown = false;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private bool hasCharge = true;
    [SerializeField] private float dashDirection = 0;

    private void Start() {
        hasCharge = true;   
    }

    public override void SetInputState(InputState inputState) {
        moveInput = inputState.moveInput;

        if (dashInput) {
            dashInputDown = false;
        }

        if (inputState.dashIsDown) {
            if (!dashInput) {
                dashInputDown = true;
            }

            dashInput = true;
        } else {
            dashInputDown = false;
            dashInput = false;
        }
    }

    public override void UpdatePreMovement(float deltaTime) {
        if (dashInputDown && hasCharge && controller.isGrounded && moveInput.x != 0f) {
            isDashing = true;
            hasCharge = false;
            dashDirection = Mathf.Sign(moveInput.x);
        } else if (!dashInput) {
            isDashing = false;
            hasCharge = true;
        } else if (Mathf.Sign(moveInput.x) != dashDirection || moveInput.x == 0f) {
            isDashing = false;
            hasCharge = true;
        }
    }

    public override void UpdateTargetVelocities(float deltaTime, ref Vector2 currentVelocity, ref Vector2 targetVelocity, ref Vector2 changeSpeed, ref Vector2 minVelocity, ref Vector2 maxVelocity) {
        if (isDashing) {
            maxVelocity.x = controller.Settings.groundDashMaxVelocityX;
            minVelocity.x = -controller.Settings.groundDashMaxVelocityX;
            changeSpeed.x = controller.Settings.groundDashAccelerationX;
            targetVelocity.x = moveInput.x * controller.Settings.groundDashMaxVelocityX;
        }
    }

    public override void OnGrounded(float deltaTime, ref Vector2 velocity, ref Vector3 position) {
        isDashing = false;
        hasCharge = true;
    }

    public override void OnFalling(float deltaTime, ref Vector2 velocity, ref Vector3 position) {
        //
    }
}
