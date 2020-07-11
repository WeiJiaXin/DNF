﻿using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _02.Scripts.Joystick
{
    public partial class Joystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform joystick;
        [SerializeField] private RectTransform center;

        private Vector2 originPos;
        private float maxLength;
        private Vector2 startPos;

        private Vector2 axis;

        private void Awake()
        {
            originPos = joystick.anchoredPosition;
        }

        private void OnEnable()
        {
            current = this;
            maxLength = joystick.rect.width / 2;
            joystick.anchoredPosition = originPos;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var pos = eventData.position;
            pos *= 1920f / Screen.width;
            joystick.anchoredPosition = pos;
            center.anchoredPosition = Vector2.zero;

            startPos = pos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var pos = eventData.position;
            pos *= 1920f / Screen.width;
            
            var delay = pos - startPos;
            if (delay.sqrMagnitude > maxLength * maxLength)
                center.anchoredPosition = delay.normalized * maxLength;
            else
                center.anchoredPosition = delay;
            if (delay.sqrMagnitude > 1)
                delay.Normalize();
            axis = delay;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            joystick.anchoredPosition = originPos;
            center.anchoredPosition = Vector2.zero;
            axis = Vector2.zero;
        }
    }
}