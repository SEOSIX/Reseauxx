using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class PseudoManager : MonoBehaviour
    {
        public static PseudoManager instance { get; private set; }
        public InputField playerName;
        
        public TextMeshProUGUI dislayConnexion;

        private void Awake()
        {
            instance = this;
        }

        public virtual string DebugPseudoName()
        {
            return playerName.text;
        }


        public IEnumerator DebugDislayConnexion()
        {
            dislayConnexion.text = $"{playerName.text} join the game";
            yield return new WaitForSeconds(2f);
            dislayConnexion.text = "";
        }
    }
}