using System;
using UnityEngine;

namespace Lowy.DebugConsole
{
    public class CommandMsa : MonoBehaviour
    {
        public ushort id;
        public CommandArg arg;
        public CommandMenu menu;
        public virtual void SetArg(CommandArg commandArg)
        {
            arg = commandArg;
            id = commandArg.id;
            transform.SetSiblingIndex(arg.id);
        }
    }
}