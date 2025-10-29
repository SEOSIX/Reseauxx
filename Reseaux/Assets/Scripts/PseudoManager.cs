using System;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class PseudoManager : MonoBehaviour
    {
        public static PseudoManager instance { get; private set; }
        public InputField playerName;


        private void Awake()
        {
            instance = this;
        }

        public string DebugPseudoName()
        {
            return playerName.text;
        }
    }
}