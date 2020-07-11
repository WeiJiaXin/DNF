using System;
using GameTimer;
using UnityEngine;

public class DungeonsRoomDoor : MonoForDebug
{
    [SerializeField] private EDirection dir = EDirection.center;
    [SerializeField] private Transform generatePoint;
    [SerializeField] private GameObject closeDoor;
    [SerializeField] private GameObject openLight;

    private Collider door;
    //
    private TimeHandle cd;

    public Transform GeneratePoint => generatePoint;

    private void Awake()
    {
        door = GetComponent<Collider>();
        cd = Timer.Start(1);
        cd.onEnd += h => { cd = null; };
    }

    private void OnTriggerEnter(Collider other)
    {
        if(cd != null)
            return;
        if (!other.CompareTag("Player"))
            return;
        DungeonsMagr.Instance.NextRoom(dir);
    }

    public void Open()
    {
        closeDoor.SetActive(false);
        openLight.SetActive(true);
        door.enabled = true;
    }

    public void Close()
    {
        closeDoor.SetActive(true);
        openLight.SetActive(false);
        door.enabled = false;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
    
    
}