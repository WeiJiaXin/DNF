using System.Collections.Generic;
using UnityEngine.UI;

namespace Lowy.DebugConsole
{
    public class CommandMsa_SendButton : CommandMsa
    {
        private Button _button;
        private Text info;

        private void Awake()
        {
            _button = GetComponent<Button>();
            info = GetComponentInChildren<Text>();
        }

        public override void SetArg(CommandArg commandArg)
        {
            base.SetArg(commandArg);
            if(_button==null)
                Awake();
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => menu.SendCallBack(arg));
            info.text = arg.desplayMsa;
        }
    }
}