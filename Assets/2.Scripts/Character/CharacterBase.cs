using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    protected CharacterAnimBase anim;

    protected virtual void Awake()
    {
        anim = GetComponent<CharacterAnimBase>();
    }
}
