using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SquashStretchController : MonoBehaviour
{
    public GameObject target;
    public float animTime = 0.15f;

    public Vector2 initialSize = Vector2.zero;

    public Vector2 squashSize = new Vector2(0.15f, 0.15f);
    public Vector2 stretchSize = new Vector2(1.15f, 1.15f);

    // Start is called before the first frame update
    void Awake()
    {
        if (!target) target = gameObject;
        initialSize = gameObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BounceVertSquash() {
        VertSquash().setOnComplete(_ => ReleaseVert());
    }

    public void ResetScale() {
        LeanTween.cancel(target);
        target.transform.localScale = initialSize;
    }

    public LTDescr VertSquash() {
        if (LeanTween.isTweening(target)) {
            LeanTween.cancel(target);
        }

        LeanTween.moveLocalY(target, squashSize.y * -0.5f, animTime);
        return LeanTween.scaleY(target, squashSize.y, animTime);
    }

    public LTDescr HorizSquash() {
        if (LeanTween.isTweening(target)) {
            LeanTween.cancel(target);
        }

        return LeanTween.scaleX(target, squashSize.x, animTime);
    }

    public LTDescr ReleaseHoriz() {
        if (LeanTween.isTweening(target)) {
            LeanTween.cancel(target);
        }

        return LeanTween.scaleX(target, initialSize.x, animTime);
    }

    public LTDescr ReleaseVert() {
        if (LeanTween.isTweening(target)) {
            LeanTween.cancel(target);
        }

        LeanTween.moveLocalY(target, 0, animTime);
        return LeanTween.scaleY(target, initialSize.y, animTime);
    }
}
