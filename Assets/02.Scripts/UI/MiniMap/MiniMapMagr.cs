using UnityEngine;
using UnityEngine.UI;

namespace _02.Scripts.UI.MiniMap
{
    public class MiniMapMagr : MonoSingleton<MiniMapMagr>
    {
        [SerializeField] private GridLayoutGroup layout;
        [SerializeField] private MiniMapRoom item;

        private RectTransform _content => layout.transform as RectTransform;

        private float _roomItemSize;
        private DungeonsInfo _info;
        private MiniMapRoom[,] _rooms;
        private MiniMapRoom _curRoom;

        protected override void Awake()
        {
            base.Awake();
            _roomItemSize = layout.cellSize.x;
        }

        public void InitInfo(DungeonsInfo info)
        {
            _info = info;
            if (_rooms != null)
            {
                for (int i = 0; i < _rooms.GetLength(0); i++)
                for (int j = 0; j < _rooms.GetLength(1); j++)
                    Destroy(_rooms[i, j].gameObject);
            }

            var infoRooms = info.rooms;
            var x = infoRooms.GetLength(0);
            var y = infoRooms.GetLength(1);
            _rooms = new MiniMapRoom[x, y];
            SetContentSize(x, y);
            GenerateRoom();
            MoveTo(_info.currentRoom);
        }

        private void SetContentSize(int xCount, int yCount)
        {
            layout.constraintCount = xCount;
            //
            var contentSize = _content.rect.size;
            var xSize = contentSize.x / xCount;
            var ySize = contentSize.y / yCount;
            var size = Mathf.Min(xSize, ySize, _roomItemSize);
            layout.cellSize = Vector2.one * size;
        }

        private void GenerateRoom()
        {
            var infoRooms = _info.rooms;
            for (int i = 0; i < infoRooms.GetLength(0); i++)
            for (int j = 0; j < infoRooms.GetLength(1); j++)
            {
                var info = infoRooms[i, j];
                var room = Instantiate(item, _content);
                room.SetInfo(_info, info);
                //
                _rooms[i, j] = room;
            }
        }

        public void MoveTo(RoomInfo room)
        {
            if (_curRoom != null)
            {
                //关闭角色 Ico
                _curRoom.RemoveIcon(MiniMapRoom.EIcon.Player);
                DisableUnknownRoomIco();
            }

            _curRoom = _rooms[room.Pos.x, room.Pos.y];
            _curRoom.ShowMap();
            //打开角色ico
            _curRoom.SetIco(MiniMapRoom.EIcon.Player);
            DisplayUnknownRoomIco();
        }

        public void DisplayUnknownRoomIco()
        {
            if(!_curRoom.Room.killed)
                return;
            var rooms = _info.AboutRooms(_curRoom.Room);
            foreach (var room in rooms)
            {
                var r = _rooms[room.Pos.x, room.Pos.y];
                if (!r.Room.killed)
                    r.SetIco(MiniMapRoom.EIcon.UnknownRoom);
            }
        }

        public void DisableUnknownRoomIco()
        {
            var rooms = _info.AboutRooms(_curRoom.Room);
            foreach (var room in rooms)
            {
                var r = _rooms[room.Pos.x, room.Pos.y];
                r.RemoveIcon(MiniMapRoom.EIcon.UnknownRoom);
            }
        }
    }
}