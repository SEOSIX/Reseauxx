using UnityEngine;

public class EscapeSettings : MonoBehaviour
{
    public GameObject target;
    private bool isPressed = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPressed = !isPressed;
            target.SetActive(isPressed);
        }
    }
}
