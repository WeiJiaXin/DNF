using System;
using UnityEngine.UI;

namespace Lowy.DebugConsole
{
    public class CommandMsa_Toggle : CommandMsa
    {
        private Toggle toggle;
        private Text info;

        private void Awake()
        {
            if(toggle)
                return;
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(OnValueChanged);
            info = GetComponentInChildren<Text>();
        }

        private void OnValueChanged(bool arg0)
        {
            arg.data = arg0.ToString();
        }

        public override void SetArg(CommandArg commandArg)
        {
            base.SetArg(commandArg);
            if(toggle==null)
                Awake();
            bool.TryParse(arg.data, out var b);
            toggle.isOn = b;
            arg.data = toggle.isOn.ToString();
            info.text = arg.desplayMsa;
        }
    }
}