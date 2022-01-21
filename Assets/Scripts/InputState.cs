using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InputState
{
    [SerializeField] public Vector2 moveInput;
    [SerializeField] public Vector2 rawMoveInput;
    [SerializeField] public bool jumpIsDown;
    [SerializeField] public bool dashIsDown;
}
