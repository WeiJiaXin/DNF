using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : Toggle
{
    private RectTransform animObj;
    private bool isH;
    private float originV;
    protected override void Awake()
    {
        base.Awake();
        animObj = graphic.transform.GetChild(0) as RectTransform;
        if(animObj==null)
            return;
        var pos = animObj.localPosition;
        isH = Mathf.Abs(pos.x) > Mathf.Abs(pos.y);
        originV = isH ? pos.x : pos.y;
        if (!isOn)
            originV = -originV;
        onValueChanged.AddListener(PlayAnimEffect);
    }

    private void PlayAnimEffect(bool arg0)
    {
        if (isH)
        {
            animObj.DOLocalMoveX(arg0 ? -originV : originV, 0);
            animObj.DOLocalMoveX(arg0 ? originV : -originV, 0.1f);
        }
        else
        {
            animObj.DOLocalMoveY(arg0 ? -originV : originV, 0);
            animObj.DOLocalMoveY(arg0 ? originV : -originV, 0.1f);
        }
    }
}