using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirDashAbility : Character2DMovementAbility
{
    public override int SortOrder { get => sortOrder; }
    public override Character2DMovementController Controller { get => controller; set => controller = value; }

    [SerializeField] public int sortOrder = 0;
    [SerializeField] private Character2DMovementController controller;

    [SerializeField] private Vector2 moveInput = Vector2.zero;
    [SerializeField] private bool dashInput = false;
    [SerializeField] private bool dashInputDown = false;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private bool hasDashCharge = true;

    [SerializeField] private Vector2 currentMaxVelocity = Vector2.zero;

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

    public override void UpdateTargetVelocities(float deltaTime, ref Vector2 currentVelocity, ref Vector2 targetVelocity, ref Vector2 changeSpeed, ref Vector2 minVelocity, ref Vector2 maxVelocity) {
        if (dashInputDown && controller.isGrounded == false && isDashing == false && hasDashCharge) {
            isDashing = true;
            hasDashCharge = false;
            currentMaxVelocity = new Vector2(controller.Settings.airDashMaxVelocityXY, controller.Settings.airDashMaxVelocityXY);

            targetVelocity = moveInput.normalized * controller.Settings.airDashMaxVelocityXY;

            if (controller.Settings.airDashIgnoreVelY) {
                currentVelocity.y = 0f;
            }

            if (controller.Settings.airDashIgnoreVelX) {
                currentVelocity.x = 0f;
            }

            changeSpeed.x = controller.Settings.airDashAccelerationXY;
            changeSpeed.y = controller.Settings.airDashAccelerationXY;
        }
        
        if (isDashing) {
            maxVelocity.x = currentMaxVelocity.x;
            maxVelocity.y = currentMaxVelocity.y;
            minVelocity.x = -currentMaxVelocity.x;
            minVelocity.y = -currentMaxVelocity.y;
        }
    }

    public override void UpdatePostMovement(float deltaTime) {
        if (isDashing) {
            currentMaxVelocity.x = Mathf.MoveTowards(currentMaxVelocity.x, controller.Settings.airMaxVelocityX, controller.Settings.airDashDecayXY * deltaTime);
            if (Mathf.Abs(currentMaxVelocity.x) <= controller.Settings.airMaxVelocityX) {
                isDashing = false;
            }
        }
    }

    public override void OnGrounded(float deltaTime, ref Vector2 velocity, ref Vector3 position) {
        isDashing = false;
        hasDashCharge = true;
    }
}
