using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableSurface : MonoBehaviour
{
    [SerializeField] public float destroyAfterStayTime = 3f;
    [SerializeField] public string expectedTag = "Player";
    [SerializeField] public GameObject destroyObject;
    [SerializeField] private float stayTimer = 0f;
    [SerializeField] private bool expectedIsStaying = false;

    public void Update()
    {
        if (expectedIsStaying)
        {
            stayTimer += Time.deltaTime;
            if (stayTimer >= destroyAfterStayTime)
            {
                Destroy(destroyObject);
            }
        }

        expectedIsStaying = false;
    }

    public void UpdateContact(Character2DMovementController controller)
    {
        expectedIsStaying = true;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("OnCollisionEnter " + collision.gameObject.name);
        if (collision.gameObject.tag == expectedTag)
        {
            expectedIsStaying = true;
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("OnCollisionExit " + collision.gameObject.name);
        if (collision.gameObject.tag == expectedTag)
        {
            expectedIsStaying = false;
            stayTimer = 0f;
        }
    }
}
