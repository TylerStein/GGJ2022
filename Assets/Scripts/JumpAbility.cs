using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAbility : Character2DMovementAbility
{
    public override int SortOrder { get => sortOrder; }
    public override Character2DMovementController Controller { get => controller; set => controller = value; }

    [SerializeField] public int sortOrder = 0;
    [SerializeField] private Character2DMovementController controller;

    [SerializeField] public bool wallJump = true;

    [SerializeField] private bool jumpInput = false;
    [SerializeField] private bool jumpInputDown = false;
    [SerializeField] private bool isBoosting = false;
    [SerializeField] private bool isJumping = false;

    [SerializeField] private Vector2 moveInput = Vector2.zero;

    public override void SetInputState(InputState inputState) {
        moveInput = inputState.moveInput;

        if (jumpInput) {
            jumpInputDown = false;
        }

        if (inputState.jumpIsDown) {
            if (!jumpInput) {
                jumpInputDown = true;
            }

            jumpInput = true;
        } else {
            jumpInputDown = false;
            jumpInput = false;
            isBoosting = false;
        }
    }

    public override void UpdatePreMovement(float deltaTime) {
        if (controller.isGrounded) {
            isBoosting = false;
            isJumping = false;
        }
    }

    public override void UpdateTargetVelocities(float deltaTime, ref Vector2 currentVelocity, ref Vector2 targetVelocity, ref Vector2 changeSpeed, ref Vector2 minVelocity, ref Vector2 maxVelocity) {
        if (jumpInputDown) {
            if (controller.isGrounded && isJumping == false) {
                isJumping = true;
                isBoosting = true;
                changeSpeed.y = controller.Settings.jumpAccelerationY;
                targetVelocity.y = controller.Settings.jumpBoostMaxVelocityY;

                return;
            } else {
                for (int i = 0; i < controller.hitCount; i++) {
                    if (controller.hitAngles[i] == 90f) {
                        // to the left or right
                        isJumping = true;
                        isBoosting = true;

                        float xDir = controller.raycastHits[i].normal.x;

                        currentVelocity.x = controller.Settings.airDashAccelerationXY * xDir;
                        currentVelocity.y = controller.Settings.airDashAccelerationXY;

                        changeSpeed.y = controller.Settings.airDashAccelerationXY;
                        changeSpeed.x = controller.Settings.airDashAccelerationXY * xDir;

                        targetVelocity.y = controller.Settings.airDashAccelerationXY;
                        targetVelocity.x = controller.Settings.airDashAccelerationXY * xDir;

                        return;
                    }
                }

            }
        }
        
        if (isJumping) {
            if (isBoosting) {
                maxVelocity.y = controller.Settings.jumpBoostMaxVelocityY;
            } else {
                maxVelocity.y = controller.Settings.jumpMaxVelocityY;
            }
            //targetVelocity.x = moveInput.x * launchVelocityX;
        }
    }

    public override void OnGrounded(float deltaTime, ref Vector2 velocity, ref Vector3 position) {
        isJumping = false;
        isBoosting = false;
    }
}
