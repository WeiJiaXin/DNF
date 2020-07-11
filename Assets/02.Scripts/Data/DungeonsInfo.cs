using UnityEngine;

public class DungeonsInfo
{
    public string areaStr;
    public string mapStr;
    public RoomInfo[,] rooms;

    private int _x = -1;
    public int X
    {
        get => _x;
        set
        {
            lastX = _x;
            lastY = _y;
            _x = value;
        }
    }

    private int _y = -1;
    public int Y
    {
        get => _y;
        set
        {
            lastX = _x;
            lastY = _y;
            _y = value;
        }
    }

    private int lastX, lastY;

    public RoomInfo currentRoom
    {
        get { return rooms[X, Y]; }
    }

    public EDirection MoveDir
    {
        get
        {
            if (lastX < 0)
                return EDirection.center;
            if (lastX == _x)
                return lastY < _y ? EDirection.up : EDirection.down;
            return lastX < _x ? EDirection.right : EDirection.left;
        }
    }

    public bool HasRoom(int x,int y)
    {
        if (x < 0 || x >= rooms.GetLength(0))
            return false;
        if (y < 0 || y >= rooms.GetLength(1))
            return false;
        return rooms[x, y] != null;
    }

    public void InitPos(int x, int y)
    {
        _x = x;
        _y = y;
        lastX = -1;
        lastY = -1;
    }
}