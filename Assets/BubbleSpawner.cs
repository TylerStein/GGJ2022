using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    public ECharacterType forCharacter = ECharacterType.LAX;
    public GameObject bubblePrefab;
    public float spawnEverySeconds = 2f;
    public float despawnAfter = 10f;
    public float spawnUpOffset = 0.5f;
    public float bubbleSpeed = 2f;
    public float spawnTimerOffset = 0f;
    public bool spawnEnabled = true;

    private float spawnTimer = 0f;

    private void Awake()
    {
        spawnTimer = spawnTimerOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnEnabled)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnEverySeconds)
            {
                spawnTimer = 0f;
                if (spawnEnabled)
                {
                    Spawn();
                }
            }
        } else {
            spawnTimer = 0f;
        }
    }

    public void Spawn()
    {
        GameObject bubble = Instantiate(bubblePrefab, transform.position + (transform.up * spawnUpOffset), Quaternion.identity);
        bubble.transform.parent = transform;

        Bubble script = bubble.GetComponent<Bubble>();
        script.SetSpawnData(transform.up, bubbleSpeed, despawnAfter);
        script.SetCharacter(forCharacter);
    }
}
