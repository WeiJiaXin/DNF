using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// 导航到item组件,挂载在<see cref="ScrollRect"/>后,调用<see cref="Nevigate"/>即可
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollViewNevigation : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        viewport = scrollRect.viewport;
        content = scrollRect.content;
    }

    public void Nevigate(RectTransform item)
    {
        Vector3 itemCurrentLocalPostion = scrollRect.GetComponent<RectTransform>()
            .InverseTransformVector(ConvertLocalPosToWorldPos(item));
        Vector3 itemTargetLocalPos = scrollRect.GetComponent<RectTransform>()
            .InverseTransformVector(ConvertLocalPosToWorldPos(viewport));

        Vector3 diff = itemTargetLocalPos - itemCurrentLocalPostion;
        diff.z = 0.0f;

        var newNormalizedPosition = new Vector2(
            diff.x / (content.rect.width - viewport.rect.width),
            diff.y / (content.rect.height - viewport.rect.height)
        );

        newNormalizedPosition = scrollRect.GetComponent<ScrollRect>().normalizedPosition - newNormalizedPosition;

        newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
        newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);

        scrollRect.normalizedPosition = newNormalizedPosition;
        // DOTween.To(() => scrollRect.normalizedPosition,
        //     x => scrollRect.normalizedPosition = x, newNormalizedPosition, 0.8f);
    }

    private Vector3 ConvertLocalPosToWorldPos(RectTransform target)
    {
        var pivotOffset = new Vector3(
            (0.5f - target.pivot.x) * target.rect.size.x,
            (1 - target.pivot.y) * target.rect.size.y,
            0f);

        var localPosition = target.localPosition + pivotOffset;

        return target.parent.TransformPoint(localPosition);
    }

    private void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
        viewport = scrollRect.viewport;
        content = scrollRect.content;
    }
}