using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFXController : MonoBehaviour
{
    public WorldSpawnManager spawnManager;
    public Vector3 landOffset = new Vector3(0f, -0.15f, 0f);
    public float landingDuration = 0.5f;
    public float landingCooldown = 0.1f;
    public bool canPlayLanding = true;

    public Vector3 moveDustOffset = new Vector3(0f, -0.4f, 0f);
    public float moveDustDuration = 0.9f;
    public int lastXDir = 0;

    public void PlayLanding()
    {
        if (canPlayLanding)
        {
            GameObject fx = spawnManager.landFxRecycler.GetOne();
            fx.transform.position = transform.position + landOffset;
            canPlayLanding = false;
            StartCoroutine(SetTimeout(landingDuration, () => spawnManager.landFxRecycler.ReturnOne(fx)));
            StartCoroutine(SetTimeout(landingCooldown, () => canPlayLanding = true));
        }
    }

    IEnumerator SetTimeout(float seconds, System.Action action)
    {
        yield return new WaitForSeconds(seconds);
        action.Invoke();
    }

    public void PlayMove(float xInput)
    {


        int sign = 0;
        if (xInput > 0) sign = 1;
        else if (xInput < 0) sign = -1;

        if (sign != lastXDir && sign != 0)
        {
            Debug.Log($"{sign} - {xInput}");
            GameObject fx = spawnManager.moveDustRecycler.GetOne();
            fx.transform.position = transform.position + moveDustOffset;
            fx.transform.localScale = new Vector3(sign, 1f, 1f);
            StartCoroutine(SetTimeout(moveDustDuration, () => spawnManager.moveDustRecycler.ReturnOne(fx)));
        }

        lastXDir = sign;
    }
}
