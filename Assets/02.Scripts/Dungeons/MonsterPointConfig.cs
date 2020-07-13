#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
[CanEditMultipleObjects]
#endif
public class MonsterPointConfig : MonoBehaviour
{
    [SerializeField] private MonsterBase _monster;
    public MonsterBase Monster => _monster;
}