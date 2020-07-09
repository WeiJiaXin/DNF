using UnityEngine;

public class DungeonsRoomDoor : MonoForDebug
{
    [SerializeField] private Transform generatePoint;
    [SerializeField] private GameObject closeDoor;
    [SerializeField] private GameObject openLight;

    public Transform GeneratePoint => generatePoint;
    

    public void Open()
    {
        closeDoor.SetActive(false);
        openLight.SetActive(true);
    }

    public void Close()
    {
        closeDoor.SetActive(true);
        openLight.SetActive(false);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}