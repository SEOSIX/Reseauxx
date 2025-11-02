using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
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
        public IEnumerator DebugDislayConnexion(string pseudo)
        {
            dislayConnexion.text = $"{pseudo} join the game";
            yield return new WaitForSeconds(2f);
            dislayConnexion.text = "";
        }
    }
}