using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Lowy.DebugConsole
{
    public partial class DebugConsole : MonoBehaviour
    {
        private static DebugConsole _ins;

        private static DebugConsole Ins
        {
            get
            {
                if (_ins == null)
                {
                    _ins = Instantiate(Resources.Load<GameObject>("DebugConsole/DebugConsole"))
                        .GetComponent<DebugConsole>();
                    AddCommand(new Command("Help",
                        () => Lowy.DebugConsole.DC.Print("Print Msa or Execution Command、ArgCommand")));
                }

                return _ins;
            }
        }

        #region UnityField

        private List<string> _testDiv = new List<string>()
        {
            "cbee5611-8fbc-43a7-a6c3-cdbff158e8b7",
            "1a9d5535-67c5-4f51-91ea-d4a725f39243",
            "800227a1-8345-4c51-ac1a-4b086303f176"
        };

        [SerializeField] private DebugButton _openBtn;
        [SerializeField] private Transform _commandContent;
        [SerializeField] private CommandMenu _menu;
        [SerializeField] private DebugOutput _output;
        [SerializeField] private Transform commandTitleBtn;
        [SerializeField] private bool enableTest;

        #endregion

        protected List<Command> _commands;

        private List<Command> Commands
        {
            get => _commands;
            set => _commands = value;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
#if !UNITY_EDITOR
            Application.logMessageReceived += UnityLogMessage;
#endif
            Commands = new List<Command>();

            _openBtn.Init();
            if (enableTest)
            {
                TestOpen();
                return;
            }

            var sup = Application.RequestAdvertisingIdentifierAsync(
                (string advertisingId, bool trackingEnabled, string error) =>
                {
                    if (_testDiv.Contains(advertisingId))
                        TestOpen();
                }
            );
            if (!sup)
            {
                Debug.Log("不支持广告ID，禁止打开调试按钮");
            }
        }

        private void UnityLogMessage(string condition, string stacktrace, LogType type)
        {
            switch (type)
            {
                case LogType.Exception:
                case LogType.Assert:
                case LogType.Error:
                    DC.Error($"{condition}\nstacktrace:{stacktrace}");
                    break;
                case LogType.Log:
                    DC.Print(condition);
                    break;
            }
        }

        private void TestOpen()
        {
            if (null != _openBtn)
                _openBtn.gameObject.SetActive(true);
        }

        private void InitContent()
        {
            ushort i = 0;
            ushort index = 0;
            for (; i < _commandContent.childCount; i++, index++)
            {
                var child = _commandContent.GetChild(i).GetComponent<CommandMsa>();
                if (child == null)
                {
                    _commandContent.GetChild(i).gameObject.SetActive(false);
                    index--;
                    continue;
                }

                if (index < Commands.Count)
                {
                    child.gameObject.SetActive(true);
                    child.id = index;
                    if (Commands[index]._args == null)
                        child.GetComponentInChildren<Text>().text = "-- " + Commands[index]._title;
                    else
                        child.GetComponentInChildren<Text>().text = "↘ " + Commands[index]._title;
                }
                else
                    child.gameObject.SetActive(false);
            }

            //因为总会有一个菜单被跳过
            i--;
            if (_commandContent.childCount - 1 < Commands.Count)
            {
                for (; i < Commands.Count; i++)
                {
                    var child = Instantiate(commandTitleBtn, _commandContent).gameObject.AddComponent<CommandMsa>();
                    child.GetComponent<Button>().onClick.AddListener(() => Dispatcher(child));
                    child.id = i;
                    if (Commands[i]._args == null)
                        child.GetComponentInChildren<Text>().text = "-- " + Commands[i]._title;
                    else
                        child.GetComponentInChildren<Text>().text = "↘ " + Commands[i]._title;
                }
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.BackQuote) && Input.GetKey(KeyCode.LeftShift))
            {
                OpenWindow();
            }

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                CloseWindow();
            }
#endif
        }

        public static void OpenWindow() => Ins._openBtn.OpenWindow();
        public static void CloseWindow() => Ins._openBtn.CloseWindow();

        private static void AddCommand(Command command)
        {
            if (Ins.Commands == null)
            {
                Ins.Commands = new List<Command>();
            }

            Ins.Commands.RemoveAll(c => c._title == command._title);
            Ins.Commands.Add(command);
            Ins.InitContent();
        }

        private static void Output(string content)
        {
            Ins._output.Output(content);
            Ins._openBtn.TipsNewMsa();
        }

        private static void Dispatcher(CommandMsa msa)
        {
            var command = Ins.Commands[msa.id];
            if (command._args == null)
                Ins.Commands[msa.id]._onClick?.Invoke();
            else
            {
                Ins._menu.Init(command);
                if (Ins._menu.transform.GetSiblingIndex() < msa.transform.GetSiblingIndex())
                    Ins._menu.transform.SetSiblingIndex(msa.transform.GetSiblingIndex());
                else
                    Ins._menu.transform.SetSiblingIndex(msa.transform.GetSiblingIndex() + 1);
                Ins._menu.gameObject.SetActive(true);
            }
        }

        public class _DC
        {
            protected void _OpenConsole() => Ins._openBtn.OpenWindow();
            protected void _CloseConsole() => Ins._openBtn.CloseWindow();

            protected void _AddCommands(params Command[] commands)
            {
                foreach (var c in commands)
                {
                    AddCommand(c);
                }
            }

            protected void _Print(string content)
            {
                Output(content);
            }

            protected void _Warning(string content)
            {
                Output($"<color=#faff00>{content}</color>");
            }

            protected void _Error(string content)
            {
                Output($"<color=#fa0000>{content}</color>");
            }
        }
    }

    public class DC : DebugConsole._DC
    {
        private static readonly DC Dc = new DC();
        public static void OpenConsole() => Dc._OpenConsole();
        public static void CloseConsole() => Dc._CloseConsole();

        public static void AddCommands(params Command[] commands) => Dc._AddCommands(commands);

        public static void Print(string content) => Dc._Print(content);
        public static void Warning(string content) => Dc._Warning(content);
        public static void Error(string content) => Dc._Error(content);
    }
}