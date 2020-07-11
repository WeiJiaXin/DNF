using System;
using System.Collections;
using System.Collections.Generic;
using _02.Scripts.Role.Variables;
using BehaviorDesigner.Runtime;
using UnityEngine;

public class DungeonsMagr : MonoSingleton<DungeonsMagr>
{
    public static DungeonsInfo Info { get; set; }
    //
    public RoleBase player { get; private set; }
    public RoomMagr room { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Info = new DungeonsInfo {areaStr = "A", mapStr = "a", rooms = new RoomInfo[3, 3]};
        Info.rooms[1, 1] = new RoomInfo(Info, 1, new Vector2Int(1, 1));
        Info.rooms[1, 0] = new RoomInfo(Info, 1, new Vector2Int(1, 0));
        Info.rooms[1, 2] = new RoomInfo(Info, 1, new Vector2Int(1, 2));
        Info.rooms[0, 1] = new RoomInfo(Info, 1, new Vector2Int(0, 1));
        Info.rooms[2, 1] = new RoomInfo(Info, 1, new Vector2Int(2, 1));
        Info.InitPos(0, 1);
    }

    private void Start()
    {
        InitRoom(Info.currentRoom);
    }

    private void InitRoom(RoomInfo roomInfo)
    {
        var roomPrefab = Resources.Load<RoomMagr>(roomInfo.resPath);
        room = Instantiate(roomPrefab, transform.parent);
        room.InIt(roomInfo);
        player = Role.LoadPlayer(room.generatePoint);
    }

    public void NextRoom(EDirection dir)
    {
        if(dir == EDirection.center)
            throw new ArgumentException("方向不能为center");
        switch (dir)
        {
            case EDirection.left:
                Info.X--;
                break;
            case EDirection.up:
                Info.Y++;
                break;
            case EDirection.right:
                Info.X++;
                break;
            case EDirection.down:
                Info.Y--;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
        }
        BloodBarMagr.DestroyAllBar();
        Destroy(room.gameObject);
        InitRoom(Info.currentRoom);
    }
}