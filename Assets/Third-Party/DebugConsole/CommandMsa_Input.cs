using System;
using UnityEngine.UI;

namespace Lowy.DebugConsole
{
    public class CommandMsa_Input : CommandMsa
    {
        private InputField _inputField;
        private Text info;

        private void Awake()
        {
            if(_inputField)
                return;
            _inputField = GetComponent<InputField>();
            _inputField.onValueChanged.AddListener(OnValueChanged);
            info = _inputField.placeholder as Text;
        }

        private void OnValueChanged(string arg0)
        {
            arg.data = arg0;
        }

        public override void SetArg(CommandArg commandArg)
        {
            base.SetArg(commandArg);
            if(_inputField==null)
                Awake();
            if (arg.data == null)
                arg.data = "";
            _inputField.text = arg.data;
            info.text = arg.desplayMsa;
        }
    }
}