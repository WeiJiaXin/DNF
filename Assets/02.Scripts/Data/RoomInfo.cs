using System.IO;
using UnityEngine;


public enum ERoomType
{
    None,
    Boss
}

public class RoomInfo
{
    private readonly DungeonsInfo _info;
    private readonly string _sceneStr;

    public int id => _info.rooms.GetLength(0) * Pos.y + Pos.x;

    public ERoomType type = ERoomType.None;

    public Vector2Int Pos { get; }
    public bool killed;

    public RoomInfo(DungeonsInfo info,string sceneStr, Vector2Int pos)
    {
        Pos = pos;
        _info = info;
        _sceneStr = sceneStr;
    }

    public string resPath => Path.Combine("Rooms", _info.areaStr, _info.mapStr, _sceneStr.ToString());
}