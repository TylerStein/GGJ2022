using UnityEngine;

[CreateAssetMenu(fileName = "New Character2DMovementSettings", menuName = "2D Movement Settings", order = 1)]
public class Character2DMovementSettings : ScriptableObject
{
    [Header("Gravity")]
    public Vector2 gravity = new Vector2(0f, -150);
    public float minVelocityY = -50f;

    [Header("Ground Control")]
    public float groundMoveAccelerationX = 20f;
    public float groundReverseAccelerationX = 400f;
    public float groundStopDecelerationX = 250f;
    public float groundMaxVelocityX = 10f;

    [Header("Air Control")]
    public float airMoveAccelerationX = 24f;
    public float airReverseAccelerationX = 400f;
    public float airStopDecelerationX = 250f;
    public float airMaxVelocityX = 12f;

    [Header("Ability: Jump")]
    public float jumpMaxVelocityY = 15f;
    public float jumpBoostMaxVelocityY = 50f;
    public float jumpAccelerationY = 50f;
    public float jumpBoostDecayY = 40f;

    [Header("Ability: Air Dash")]
    public float airDashMaxVelocityXY = 30f;
    public float airDashAccelerationXY = 30f;
    public float airDashDecayXY = 15f;
    public bool airDashIgnoreVelX = true;
    public bool airDashIgnoreVelY = true;

    [Header("Ability: Wall Jump")]
    public float wallJumpForceXY = 60f;

    [Header("Ability: Ground Dash")]
    public float groundDashMaxVelocityX = 30f;
    public float groundDashAccelerationX = 30f;
}
