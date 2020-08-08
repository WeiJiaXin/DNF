using System.Collections.Generic;
using UnityEngine;

public class DungeonsInfo
{
    public string areaStr;
    public string mapStr;
    public RoomInfo[,] rooms;
    public bool[,] passes;

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

    public void AddPass(int x1, int y1, int x2, int y2)
    {
        if(x1 == x2 && y1 == y2)
            return;
        passes[rooms.GetLength(0) * y1 + x1,
            rooms.GetLength(0) * y2 + x2] = true;
        passes[rooms.GetLength(0) * y2 + x2,
            rooms.GetLength(0) * y1 + x1] = true;
    }

    public bool HasRoom(int x, int y)
    {
        if (x < 0 || x >= rooms.GetLength(0))
            return false;
        if (y < 0 || y >= rooms.GetLength(1))
            return false;
        return rooms[x, y] != null;
    }

    public EDirection DoorsByRoom(RoomInfo room)
    {
        var result = EDirection.none;
        var id = room.id;
        for (int i = 0; i < passes.GetLength(0); i++)
        {
            if (passes[i, id] == false)
                continue;
            int y = i / rooms.GetLength(0);
            int x = i % rooms.GetLength(0);
            if (room.Pos.x == x)
                result |= y > room.Pos.y ? EDirection.up : EDirection.down;
            if (room.Pos.y == y)
                result |= x > room.Pos.x ? EDirection.right : EDirection.left;
        }
        return result;
    }

    public List<RoomInfo> AboutRooms(RoomInfo room) => AboutRooms(room.id);
    public List<RoomInfo> AboutRooms(int roomId)
    {
        var result = new List<RoomInfo>();
        for (int i = 0; i < passes.GetLength(0); i++)
        {
            if (passes[i, roomId] == false)
                continue;
            int y = i / rooms.GetLength(0);
            int x = i % rooms.GetLength(0);
            result.Add(rooms[x,y]);
        }
        return result;
    }

    public void InitPos(int x, int y)
    {
        _x = x;
        _y = y;
        lastX = -1;
        lastY = -1;
    }
}