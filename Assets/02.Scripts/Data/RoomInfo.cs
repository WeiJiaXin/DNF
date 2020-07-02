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
    public ushort id;
    public ERoomType type = ERoomType.None;
    
    public Vector2Int Pos { get; }
    public bool killed;

    public RoomInfo(DungeonsInfo info, ushort id, Vector2Int pos)
    {
        Pos = pos;
        _info = info;
        this.id = id;
    }

    public string resPath => Path.Combine("Rooms",_info.areaStr, _info.mapStr, id.ToString());
}