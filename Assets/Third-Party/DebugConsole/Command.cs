using System;
using System.Collections.Generic;
using static Lowy.DebugConsole.DebugConsole._DC;

namespace Lowy.DebugConsole
{
    public class Command
    {
        public string _title;
        public Action _onClick;
        public List<CommandArg> _args;

        /// <summary>
        /// 不带参命令
        /// </summary>
        /// <param name="title"></param>
        /// <param name="norClick"></param>
        /// <param name="closeWindow">执行命令后关闭窗口</param>
        public Command(string title,Action norClick,bool closeWindow=false)
        {
            _title = title;
            _onClick = norClick;
            if (closeWindow)
                _onClick += CloseWindow;
        }
        /// <summary>
        /// 简单带参命令
        /// </summary>
        /// <param name="title"></param>
        /// <param name="complete"></param>
        /// <param name="args"></param>
        public Command(string title, Action<Dictionary<ushort,string>> completeCallback, params CommandArg[] args)
        {
            _title = title;
            _args = new List<CommandArg>(args.Length + 1);
            foreach (var arg in args)
            {
                if (arg.id == ushort.MaxValue)
                    DC.Warning("简单带参命令构造中，参数id不可为" + ushort.MaxValue);
            }
            _args.AddRange(args);
            var execution = new CommandArg(ushort.MaxValue, "Execution", CommandArgType.SendButton);
            execution.SendButtonCallBack += completeCallback;
            _args.Add(execution);
        }
        /// <summary>
        /// 带参命令
        /// </summary>
        /// <param name="title"></param>
        /// <param name="args"></param>
        public Command(string title, params CommandArg[] args)
        {
            this._title = title;
            bool hasSend = false;
            foreach (var arg in args)
            {
                if (arg.argType == CommandArgType.SendButton)
                    hasSend = true;
            }
            if(!hasSend)
                DC.Warning($"带参命令:{title},没有Execution按钮");
            _args = new List<CommandArg>(args);
        }

        private void CloseWindow()
        {
            DebugConsole.CloseWindow();
        }
    }

}
