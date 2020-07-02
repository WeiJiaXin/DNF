using System;
using UnityEngine;
using UnityEngine.UI;

namespace Lowy.DebugConsole
{
    public class DebugOutput : MonoBehaviour
    {
        #region UnityField

        [SerializeField] private int _lines = 40;
        [SerializeField] private Text _content;

        #endregion

        public void Output(string msa)
        {
            _content.text += "\n-> " + msa;
            var msas = _content.text.Split('\n');
            if (msas.Length > _lines)
                _content.text = _content.text.Remove(0, msas[0].Length + 1);
        }
    }
}