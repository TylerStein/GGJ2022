using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpawnManager : MonoBehaviour
{
    public PrefabRecycler bubbleRecycler = new PrefabRecycler();
    public PrefabRecycler landFxRecycler = new PrefabRecycler();
    public PrefabRecycler moveDustRecycler = new PrefabRecycler();

    public void Awake()
    {
        bubbleRecycler.Initialize();
        landFxRecycler.Initialize();
        moveDustRecycler.Initialize();
    }
}
