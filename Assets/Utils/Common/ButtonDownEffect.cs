using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonDownEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler,
    IPointerEnterHandler
{
    private bool isDownButton;
    private Tweener tweener;

    public bool isPlayPressSound = true;
    public bool isZoomIn = false;
    public bool isPlaySound = true;

    public Vector3 zoomInScale = new Vector3(1.5f, 1.5f, 1.5f);
    public Vector3 zoomOutScale = new Vector3(0.9f, 0.9f, 0.9f);

    private Vector3 originalScale;
    private Vector3 targetScale;
    private Button selfButton;

    private void Awake()
    {
        originalScale = transform.localScale;
        selfButton = transform.GetComponent<Button>();
        if (selfButton != null && isPlaySound)
        {
            selfButton.onClick.AddListener(() =>
            {
//                SoundHelper.Button();
            });
        }
    }

    private void OnDestroy()
    {
        tweener.Kill();
        tweener = null;
        if (selfButton != null)
        {
            selfButton.onClick.RemoveAllListeners();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selfButton != null && selfButton.interactable)
        {
            tweener.Kill();
            tweener = null;

            isDownButton = true;

            targetScale = isZoomIn
                ? CalculationTool(zoomInScale, originalScale)
                : CalculationTool(zoomOutScale, originalScale);

            tweener = transform.DOScale(targetScale, 0.1f);
            if (isZoomIn)
            {
                transform.SetAsLastSibling();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (selfButton != null && selfButton.interactable)
        {
            tweener.Kill();
            tweener = null;

            isDownButton = false;
            tweener = transform.DOScale(originalScale, 0.1f);
//            if (isPlayPressSound && isPlaySound)
//            {
//                SoundHelper.Button();
//            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (selfButton != null && selfButton.interactable && isDownButton)
        {
            tweener.Kill();
            tweener = null;

            tweener = transform.DOScale(originalScale, 0.1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selfButton != null && selfButton.interactable && isDownButton)
        {
            tweener.Kill();
            tweener = null;

            targetScale = isZoomIn
                ? CalculationTool(zoomInScale, originalScale)
                : CalculationTool(zoomOutScale, originalScale);

            tweener = transform.DOScale(targetScale, 0.1f);
            if (isZoomIn)
            {
                transform.SetAsLastSibling();
            }
        }
    }

    private Vector3 CalculationTool(Vector3 zoomScale, Vector3 origScale)
    {
        return new Vector3(zoomScale.x * origScale.x, zoomScale.y * origScale.y, zoomScale.z * origScale.z);
    }
}