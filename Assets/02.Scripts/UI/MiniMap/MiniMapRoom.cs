using System;
using UnityEngine;
using UnityEngine.UI;

namespace _02.Scripts.UI.MiniMap
{
    public class MiniMapRoom : MonoForDebug
    {
        public enum EIcon
        {
            None,
            UnknownRoom,
            Player,
            Boss,
        }

        [SerializeField] private Image map;
        [SerializeField] private Image icon;

        private DungeonsInfo _dungeonsInfo;
        private RoomInfo _info;
        private EIcon _eIcon = EIcon.None;

        public RoomInfo Room => _info;

        public void SetInfo(DungeonsInfo dungeonsInfo, RoomInfo info)
        {
            if (info == null)
            {
                return;
            }

            _dungeonsInfo = dungeonsInfo;
            _info = info;

            var doors = dungeonsInfo.DoorsByRoom(info);
            map.sprite = Resources.Load<Sprite>("Sprites/MiniMap/" + doors.ToString().Replace(" ", ""));
            //
            // SetIco
            if (info.type == ERoomType.Boss)
                SetIco(EIcon.Boss);
        }

        public void ShowMap()
        {
            map.SetActive(true);
        }

        public void SetIco(EIcon eIcon)
        {
            if (eIcon <= _eIcon)
                return;

            icon.sprite = Resources.Load<Sprite>($"Sprites/MiniMap/Icons/{eIcon}");
            icon.SetActive(true);
            _eIcon = eIcon;
        }

        public void RemoveIcon(EIcon eIcon)
        {
            if (eIcon != _eIcon)
                return;
            icon.SetActive(false);
            _eIcon = EIcon.None;
        }
    }
}