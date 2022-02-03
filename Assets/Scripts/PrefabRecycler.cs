using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabRecycler
{
    public GameObject prefab;
    public int initialBuffer = 10;
    public int capacityBuffer = 10;
    public Transform container;
    private Queue<GameObject> queue = new Queue<GameObject>();

    public void Initialize()
    {
        Instantiate(initialBuffer);
    }

    private void Instantiate(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject gameObject = GameObject.Instantiate(prefab);
            ReturnOne(gameObject);
        }
    }

    public GameObject GetOne()
    {
        if (queue.Count < 1)
        {
            Instantiate(capacityBuffer);
        }

        GameObject gameObject = queue.Dequeue();
        gameObject.SetActive(true);
        return gameObject;
    }

    public void GetMany(int count, List<GameObject> gameObjects)
    {
        for (int i = 0; i < count; i++)
        {
            gameObjects.Add(GetOne());
        }
    }

    public void ReturnOne(GameObject gameObject)
    {
        gameObject.transform.parent = container;
        gameObject.SetActive(false);
        queue.Enqueue(gameObject);
    }

    public void ReturnMany(List<GameObject> gameObjects)
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            ReturnOne(gameObjects[i]);
        }
    }
}