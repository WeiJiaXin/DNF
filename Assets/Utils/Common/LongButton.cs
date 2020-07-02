using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LongButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler,
    IPointerEnterHandler
{

    // 延迟时间
    [SerializeField] private float delay = 0.2f;
    [SerializeField] private bool byPointUp = false;
    [SerializeField] private UnityEvent pointDown;
    [SerializeField] private UnityEvent longClick;
    [SerializeField] private UnityEvent click;
    [SerializeField] private UnityEvent pointUp;
    
    public UnityEvent OnPointDown => pointDown;
    public UnityEvent OnLongClick => longClick;
    public UnityEvent OnClick => click;
    public UnityEvent OnPointUp => pointUp;

    // 按钮是否是按下状态
    private bool isDown = false;
    private bool isInSelf = false;

    private bool isLongClick = false;
    private bool isCallLongClick = false;

    // 按钮最后一次是被按住状态时候的时间
    private float lastIsDownTime;

    void Update()
    {
        // 如果按钮是被按下状态
        if (isDown)
        {
            // 当前时间 -  按钮最后一次被按下的时间 > 延迟时间0.2秒
            if (Time.time - lastIsDownTime > delay)
            {
                // 触发长按方法  
                isDown = false;
                isLongClick = true;
                isCallLongClick = LongClick(false);
            }
        }
    }

    private bool LongClick(bool pointUp)
    {
        if (!isInSelf)
            return false;
        if (byPointUp == pointUp)
        {
            longClick?.Invoke();
            return true;
        }

        return false;
    }

    private void Click()
    {
        if (!isInSelf)
            return;
        click?.Invoke();
    }

    // 当按钮被按下后系统自动调用此方法  
    public void OnPointerDown(PointerEventData eventData)
    {
        isLongClick = false;
        isCallLongClick = false;
        isDown = true;
        isInSelf = true;
        lastIsDownTime = Time.time;
        pointDown?.Invoke();
    }

    // 当按钮抬起的时候自动调用此方法  
    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
        //调用过长按，就不调用了
        if (!isCallLongClick)
        {
            //没有的话就判断可能会是click或longClick
            if (isLongClick)
                LongClick(true);
            else
                Click();
        }
        pointUp?.Invoke();
    }

    // 当鼠标从按钮上离开的时候自动调用此方法  
    public void OnPointerExit(PointerEventData eventData)
    {
        isInSelf = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isInSelf = true;
    }
}