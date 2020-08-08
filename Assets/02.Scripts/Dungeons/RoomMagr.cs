using System;
using System.Collections.Generic;
using _02.Scripts.UI.MiniMap;
using Lowy.Event;
using UnityEngine;

public class RoomMagr : MonoForDebug
{
    public static RoomMagr Current => DungeonsMagr.Instance ? DungeonsMagr.Instance.room : null;

    [SerializeField] private DungeonsRoomDoor doorUp;
    [SerializeField] private DungeonsRoomDoor doorDown;
    [SerializeField] private DungeonsRoomDoor doorLeft;
    [SerializeField] private DungeonsRoomDoor doorRight;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform monsterGeneratePointParent;

    public Rect Bord => _bord;
    [SerializeField] private Rect _bord;

    private List<DungeonsRoomDoor> doors;
    public List<MonsterBase> monsters { get; private set; }
    public List<MonsterBase> bosses { get; private set; }

    private RoomInfo info;

    public Transform generatePoint
    {
        get
        {
            switch (DungeonsMagr.Info.MoveDir)
            {
                case EDirection.left: return doorRight.GeneratePoint;
                case EDirection.up: return doorDown.GeneratePoint;
                case EDirection.right: return doorLeft.GeneratePoint;
                case EDirection.down: return doorUp.GeneratePoint;
                case EDirection.center: return centerPoint;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public void InIt(RoomInfo roomInfo)
    {
        info = roomInfo;
        InitMonster();
        InitDoors();
    }

    private void InitMonster()
    {
        if (info.killed)
            return;
        var cs = monsterGeneratePointParent.GetComponentsInChildren<MonsterPointConfig>();
        int count = cs.Length;
        for (int i = 0; i < count; i++)
        {
            var point = cs[i];
            var monster = Instantiate(point.Monster, point.transform);
            monster.name = i.ToString();
            monster.InitData(new RoleData());
        }
    }

    private void InitDoors()
    {
        doors = new List<DungeonsRoomDoor>();
        var dir = DungeonsMagr.Info.DoorsByRoom(info);
        if (!dir.HasFlag(EDirection.left))
            doorLeft.Disable();
        else
            doors.Add(doorLeft);
        if (!dir.HasFlag(EDirection.right))
            doorRight.Disable();
        else
            doors.Add(doorRight);
        if (!dir.HasFlag(EDirection.down))
            doorDown.Disable();
        else
            doors.Add(doorDown);
        if (!dir.HasFlag(EDirection.up))
            doorUp.Disable();
        else
            doors.Add(doorUp);
        //
        foreach (var door in doors)
        {
            if (info.killed)
                door.Open();
            else
                door.Close();
        }
    }

    private void Awake()
    {
        monsters = new List<MonsterBase>();
        bosses = new List<MonsterBase>();
    }

    private void OnEnable()
    {
        EventManager.AddListener<MonsterInitedEve>(OnMonsterInit);
        EventManager.AddListener<MonsterDieEve>(OnMonsterDie);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<MonsterInitedEve>(OnMonsterInit);
        EventManager.RemoveListener<MonsterDieEve>(OnMonsterDie);
    }

    private void OnMonsterInit(MonsterInitedEve e)
    {
        monsters.Add(e.monster);
    }

    private void OnMonsterDie(MonsterDieEve e)
    {
        monsters.Remove(e.monster);
        if (monsters.Count <= 0)
            FinishRoom();
    }

    private void FinishRoom()
    {
        info.killed = true;

        foreach (var door in doors)
        {
            door.Open();
        }
        
        MiniMapMagr.Instance.DisplayUnknownRoomIco();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        var bord = Rect.zero;
        bord.xMin = _bord.xMin - 5 * 1920f / 1080f;
        bord.xMax = _bord.xMax + 5 * 1920f / 1080f;
        bord.yMin = _bord.yMin - 5 * 1.414f;
        bord.yMax = _bord.yMax + 5 * 1.414f;
        Gizmos.DrawLine(new Vector3(bord.xMin, 0, bord.yMin), new Vector3(bord.xMin, 0, bord.yMax));
        Gizmos.DrawLine(new Vector3(bord.xMin, 0, bord.yMin), new Vector3(bord.xMax, 0, bord.yMin));
        Gizmos.DrawLine(new Vector3(bord.xMax, 0, bord.yMax), new Vector3(bord.xMin, 0, bord.yMax));
        Gizmos.DrawLine(new Vector3(bord.xMax, 0, bord.yMax), new Vector3(bord.xMax, 0, bord.yMin));
        Gizmos.color = Color.white;
    }
}

[Flags]
public enum EDirection
{
    none = 0,
    left = 1,
    up = 2,
    right = 4,
    down = 8,
    center = 16
}