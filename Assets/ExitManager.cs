using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitManager : MonoBehaviour
{
    public string nextLevelName = "";
    public Exit exit1;
    public Exit exit2;

    // Update is called once per frame
    void Update()
    {
        if (exit1.hasContact && exit2.hasContact)
        {
            Debug.Log("Exit Scene");
            SceneManager.LoadScene(nextLevelName);
        }
    }
}
