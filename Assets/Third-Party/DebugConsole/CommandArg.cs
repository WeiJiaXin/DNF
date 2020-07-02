using System;
using System.Collections.Generic;

namespace Lowy.DebugConsole
{
    public class CommandArg
    {
        public ushort id;
        public string desplayMsa;
        public CommandArgType argType;
        public bool hasData;
        public string data;


        private event Action _buttonCallBack;
        public event Action ButtonCallBack
        {
            add
            {
                if (argType == CommandArgType.Button)
                    _buttonCallBack += value;
                else
                    throw new Exception($"命令参数为: {argType}，注册事件失败。");
            }
            remove
            {
                _buttonCallBack -= value;
            }
        }
        private event Action< Dictionary<ushort, string>> _sendButtonCallBack;
        public event Action< Dictionary<ushort, string>> SendButtonCallBack
        {
            add
            {
                if (argType == CommandArgType.SendButton)
                    _sendButtonCallBack += value;
                else
                    throw new Exception($"命令参数为: {argType}，注册事件失败。");
            }
            remove
            {
                _sendButtonCallBack -= value;
            }
        }

        public CommandArg(ushort id, string desplayMsa, CommandArgType argType = CommandArgType.Button)
        {
            this.id = id;
            this.desplayMsa = desplayMsa;
            this.argType = argType;
        }

        public void CallBack()
        {
            _buttonCallBack?.Invoke();
        }
        public void CallBack(Dictionary<ushort, string> args)
        {
            _sendButtonCallBack?.Invoke(args);
        }
    }

}
