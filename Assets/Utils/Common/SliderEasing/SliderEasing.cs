using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/SliderEase", 34)]
public class SliderEasing : Slider
{
    private RectTransform fillEaseRect;
    private RectTransform fillEaseContainerRect;
    private Image fillEaseImage;

    private float easeValue;
    private Tweener doTween;

    private int axis
    {
        get { return direction == Direction.LeftToRight || direction == Direction.RightToLeft ? 0 : 1; }
    }

    private bool reverseValue
    {
        get { return direction == Direction.RightToLeft || direction == Direction.TopToBottom; }
    }

    /// <summary>
    ///   <para>The current value of the slider normalized into a value between 0 and 1.</para>
    /// </summary>
    public float easeNormalizedValue
    {
        get
        {
            return Mathf.Approximately(minValue, maxValue) ? 0.0f : Mathf.InverseLerp(minValue, maxValue, easeValue);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        easeValue = m_Value;
        Cache();
    }

    private void Cache()
    {
        if(!fillRect)
            return;
        fillEaseRect = fillRect.parent.GetChild(0).transform as RectTransform;
        fillEaseContainerRect = fillEaseRect.parent.transform as RectTransform;
        fillEaseImage = fillEaseRect.GetComponent<Image>();
    }

    protected override void Set(float input, bool sendCallback = true)
    {
        base.Set(input, sendCallback);
        Cache();
        if (!sendCallback)
        {
            doTween?.Kill();
            easeValue = m_Value;
            UpdateEaseVisuals();
            return;
        }
        doTween = DOTween.To(v => easeValue = v, easeValue, m_Value, 1.2f);
        doTween.onUpdate += UpdateEaseVisuals;
    }

    private void UpdateEaseVisuals()
    {
        if (fillEaseContainerRect != null)
        {
            Vector2 zero = Vector2.zero;
            Vector2 one = Vector2.one;
            if (fillEaseImage != null && fillEaseImage.type == Image.Type.Filled)
                fillEaseImage.fillAmount = easeNormalizedValue;
            else if (reverseValue)
                zero[axis] = 1f - easeNormalizedValue;
            else
                one[axis] = easeNormalizedValue;
            fillEaseRect.anchorMin = zero;
            fillEaseRect.anchorMax = one;
        }
    }

    private float ClampValue(float input)
    {
        float f = Mathf.Clamp(input, minValue, maxValue);
        if (wholeNumbers)
            f = Mathf.Round(f);
        return f;
    }
}