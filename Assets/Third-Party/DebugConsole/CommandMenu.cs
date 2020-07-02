using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lowy.DebugConsole
{
    public class CommandMenu : MonoBehaviour
    {
        #region UnityField
        [SerializeField] Transform _content;
        [SerializeField] Toggle toggle;
        [SerializeField] InputField input;
        [SerializeField] Button button;
        [SerializeField] Button sendButton;
        #endregion

        #region Pool
        private List<Toggle> _offlineToggles;
        private List<Toggle> _workToggles;
        private List<InputField> _offlineInputs;
        private List<InputField> _workInputs;
        private List<Button> _offlineButtons;
        private List<Button> _workButtons;
        private Button _sendButton;
        #endregion

        private Command _command;

        private void Awake()
        {
            if (_offlineToggles != null)
                return;
            _offlineToggles = new List<Toggle>();
            _workToggles = new List<Toggle>();
            _offlineInputs = new List<InputField>();
            _workInputs = new List<InputField>();
            _offlineButtons = new List<Button>();
            _workButtons = new List<Button>();
        }
        public void Init(Command command)
        {
            _command = command;
            if (_offlineToggles == null)
                Awake();
            _offlineToggles.AddRange(_workToggles);
            _offlineInputs.AddRange(_workInputs);
            _offlineButtons.AddRange(_workButtons);
            _workToggles.Clear();
            _workInputs.Clear();
            _workButtons.Clear();
            //
            bool hasSendBtn = false;
            foreach (var arg in _command._args)
            {
                switch (arg.argType)
                {
                    case CommandArgType.Choice:
                        var toggle = GetToggle();
                        arg.hasData = true;
                        toggle.GetComponent<CommandMsa>().SetArg(arg);
                        toggle.gameObject.SetActive(true);
                        break;
                    case CommandArgType.InputField:
                        var input = GetInputField();
                        arg.hasData = true;
                        input.GetComponent<CommandMsa>().SetArg(arg);
                        input.gameObject.SetActive(true);
                        break;
                    case CommandArgType.Button:
                        var button = GetButton();
                        arg.hasData = false;
                        button.GetComponent<CommandMsa>().SetArg(arg);
                        button.gameObject.SetActive(true);
                        break;
                    case CommandArgType.SendButton:
                        var sendButton = GetSendButton();
                        arg.hasData = false;
                        sendButton.GetComponent<CommandMsa>().SetArg(arg);
                        sendButton.gameObject.SetActive(true);
                        hasSendBtn = true;
                        break;
                    default:
                        break;
                }
            }
            foreach (var t in _offlineToggles) t.gameObject.SetActive(false);
            foreach (var t in _offlineInputs) t.gameObject.SetActive(false);
            foreach (var t in _offlineButtons) t.gameObject.SetActive(false);
            if (!hasSendBtn) _sendButton?.gameObject.SetActive(false);
        }

        public void SendCallBack(CommandArg arg)
        {
            //
            Dictionary<ushort, string> args = new Dictionary<ushort, string>();
            foreach (var commandArg in _command._args)
            {
                if (commandArg.hasData)
                    args.Add(commandArg.id, commandArg.data);
            }
            arg.CallBack(args);
        }

        private Toggle GetToggle()
        {
            if (_offlineToggles.Count > 0)
            {
                _workToggles.Add(_offlineToggles[0]);
                _offlineToggles.RemoveAt(0);
            }
            else
            {
                _workToggles.Add(Instantiate(toggle.gameObject, _content).GetComponent<Toggle>());
                _workToggles[_workToggles.Count - 1].gameObject.AddComponent<CommandMsa_Toggle>().menu=this;
            }
            return _workToggles[_workToggles.Count - 1];
        }
        private InputField GetInputField()
        {
            if (_offlineInputs.Count > 0)
            {
                _workInputs.Add(_offlineInputs[0]);
                _offlineInputs.RemoveAt(0);
            }
            else
            {
                _workInputs.Add(Instantiate(input.gameObject, _content).GetComponent<InputField>());
                _workInputs[_workInputs.Count - 1].gameObject.AddComponent<CommandMsa_Input>().menu=this;
            }
            return _workInputs[_workInputs.Count - 1];
        }
        private Button GetButton()
        {
            if (_offlineButtons.Count > 0)
            {
                _workButtons.Add(_offlineButtons[0]);
                _offlineButtons.RemoveAt(0);
            }
            else
            {
                _workButtons.Add(Instantiate(button.gameObject, _content).GetComponent<Button>());
                _workButtons[_workButtons.Count - 1].gameObject.AddComponent<CommandMsa_Button>().menu=this;
            }
            return _workButtons[_workButtons.Count - 1];
        }
        private Button GetSendButton()
        {
            if (_sendButton == null)
            {
                _sendButton = Instantiate(sendButton.gameObject, _content).GetComponent<Button>();
                _sendButton.gameObject.AddComponent<CommandMsa_SendButton>().menu=this;
            }
            return _sendButton;
        }

    }

}
