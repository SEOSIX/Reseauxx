using UnityEngine;

namespace DefaultNamespace
{
    public class Mass : MonoBehaviour
    {
        [SerializeField] private PlayerMovement pm;
        public void AnimFinished()
        {
            pm.canWalk = true;
        }
    }
}