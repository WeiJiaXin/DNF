using System;
using System.Collections.Generic;
using Lowy.Event;
using UnityEngine;

public class RoomMagr : MonoBehaviour
{
    public static RoomMagr Current => DungeonsMagr.Instance.room;
    
    [SerializeField] private Transform doorUp;
    [SerializeField] private Transform doorDown;
    [SerializeField] private Transform doorLeft;
    [SerializeField] private Transform doorRight;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform monsterGeneratePoint;

    private List<Transform> doors;
    public List<MonsterBase> monsters { get; private set; }
    public List<MonsterBase> bosses { get; private set; }

    private RoomInfo info;

    public Transform generatePoint
    {
        get
        {
            switch (DungeonsMagr.Info.MoveDir)
            {
                case EDirection.left: return doorRight.GetChild(0);
                case EDirection.up: return doorDown.GetChild(0);
                case EDirection.right: return doorLeft.GetChild(0);
                case EDirection.down: return doorUp.GetChild(0);
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
        int count = monsterGeneratePoint.childCount;
        for (int i = 0; i < count; i++)
        {
            var point = monsterGeneratePoint.GetChild(i);
            var monster = Resources.Load<MonsterBase>("Role/02.Monster/GasolineTank");
            monster = Instantiate(monster, point);
            monster.name = i.ToString();
            monster.InitData(new RoleData());
        }
    }

    private void InitDoors()
    {
        doors = new List<Transform>();
        var pos = info.Pos;
        if (!DungeonsMagr.Info.HasRoom(pos.x - 1, pos.y))
            doorLeft.gameObject.SetActive(false);
        else
            doors.Add(doorLeft);
        if (!DungeonsMagr.Info.HasRoom(pos.x + 1, pos.y))
            doorRight.gameObject.SetActive(false);
        else
            doors.Add(doorRight);
        if (!DungeonsMagr.Info.HasRoom(pos.x, pos.y - 1))
            doorDown.gameObject.SetActive(false);
        else
            doors.Add(doorDown);
        if (!DungeonsMagr.Info.HasRoom(pos.x, pos.y + 1))
            doorUp.gameObject.SetActive(false);
        else
            doors.Add(doorUp);
        //
        if (info.killed)
        {
            foreach (var door in doors)
            {
                // door.open
            }
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
    }
}

[Flags]
public enum EDirection
{
    left = 1,
    up = 2,
    right = 4,
    down = 8,
    center = 16
}