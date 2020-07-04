using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
public class MonsterPointConfig : MonoBehaviour
{
    [SerializeField] private MonsterBase _monster;
    public MonsterBase Monster => _monster;
}