using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lowy.DebugConsole
{
    public class DebugButton : Button, IDragHandler
    {
        private GameObject root;
        private Vector2 offset;
        private bool moved = false;

        public void Init()
        {
            root = transform.parent.Find("Root").gameObject;
        }

        protected override void Start()
        {
            base.Start();
            onClick.AddListener(OpenOrCloseWindow);
        }

        public void TipsNewMsa()
        {
            if (!gameObject.activeInHierarchy)
                return;
            if (root.activeInHierarchy)
                return;
            // transform.DOScale(Vector3.one * 1.3f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        }
        
        private void OpenOrCloseWindow()
        {
            root.SetActive(!root.activeInHierarchy);
            transform.localScale=Vector3.one;
            // transform.DOKill();
        }

        public void OpenWindow()
        {
            if(root.activeInHierarchy)
                return;
            OpenOrCloseWindow();
            root.SetActive(true);
        }
        
        public void CloseWindow()
        {
            if(!root.activeInHierarchy)
                return;
            OpenOrCloseWindow();
            root.SetActive(false);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            moved = false;
            offset = transform.position - (Vector3) eventData.position;
            base.OnPointerDown(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.selectedObject != gameObject)
                return;
            moved = true;
            transform.position = eventData.position + offset;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (moved)
                return;
            base.OnPointerClick(eventData);
        }
    }
}