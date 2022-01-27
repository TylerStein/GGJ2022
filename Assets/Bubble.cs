using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bubble : MonoBehaviour
{
    public string playerTag = "Player";
    public ECharacterType forCharacter = ECharacterType.SQUISH;
    public string blockSquishLayerName = "BlocksLight";
    public string blockLaxLayerName = "BlocksDark";
    public string lightEntityLayerName = "LightEntity";
    public string darkEntityLayerName = "DarkEntity";
    public GameObject platformObject;

    private Vector3 moveDirection = Vector3.up;
    private float moveSpeed = 5f;
    private float despawnAfter = 10f;
    private float despawnTimer = 0f;

    private BubbleAbility activeBubbleAbility;

    public void SetSpawnData(Vector2 dir, float speed, float despawn)
    {
        moveDirection = dir;
        moveSpeed = speed;
        despawnAfter = despawn;
    }

    public void SetCharacter(ECharacterType type)
    {
        forCharacter = type;
        if (forCharacter == ECharacterType.LAX)
        {
            platformObject.layer = LayerMask.NameToLayer(blockSquishLayerName);
        }
        else
        {
            platformObject.layer = LayerMask.NameToLayer(blockLaxLayerName);
        }
    }


    public void Awake()
    {
        SetCharacter(forCharacter);
    }

    public void Update()
    {
        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnAfter)
        {
            Destroy(gameObject);
        } else
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == playerTag)
        {
            if ((forCharacter == ECharacterType.LAX && collision.gameObject.layer == LayerMask.NameToLayer(darkEntityLayerName))
                || (forCharacter == ECharacterType.SQUISH && collision.gameObject.layer == LayerMask.NameToLayer(lightEntityLayerName)))
            {
                activeBubbleAbility = collision.gameObject.GetComponent<BubbleAbility>();
                activeBubbleAbility.activeBubble = this;
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (activeBubbleAbility && collision.gameObject == activeBubbleAbility.gameObject)
        {
            activeBubbleAbility.activeBubble = null;
            activeBubbleAbility = null;
        }
    }
}