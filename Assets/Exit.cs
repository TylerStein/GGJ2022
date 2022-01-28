using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Exit : MonoBehaviour
{
    public string playerTag = "Player";
    public bool hasContact = false;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == playerTag)
        {
            hasContact = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (hasContact && collision.gameObject.tag == playerTag)
        {
            hasContact = false;
        }
    }
}
