using System;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    public static Cursor instance { get; private set; }

    [Header("Cursor References")]
    public Image cursorRight;
    public Image cursorLeft;
    public GameObject cursorMain;

    [Header("Recenter Settings")]
    public float recenterSpeed = 5f;
    public Vector3 centerOffset = Vector3.zero;

    private bool isRecentering = false;
    private Vector3 initialRightPos;
    private Vector3 initialLeftPos;

    private Vector3 targetRightPos;
    private Vector3 targetLeftPos;

    private Vector3 rightVelocity = Vector3.zero;
    private Vector3 leftVelocity = Vector3.zero;

    public bool IsMoving => isRecentering;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        initialRightPos = cursorRight.rectTransform.localPosition;
        initialLeftPos = cursorLeft.rectTransform.localPosition;
    }

    void Update()
    {
        if (isRecentering)
        {
            cursorRight.rectTransform.localPosition = Vector3.SmoothDamp(
                cursorRight.rectTransform.localPosition,
                targetRightPos,
                ref rightVelocity,
                1f / recenterSpeed
            );

            cursorLeft.rectTransform.localPosition = Vector3.SmoothDamp(
                cursorLeft.rectTransform.localPosition,
                targetLeftPos,
                ref leftVelocity,
                1f / recenterSpeed
            );

            if (Vector3.SqrMagnitude(cursorRight.rectTransform.localPosition - targetRightPos) < 0.001f &&
                Vector3.SqrMagnitude(cursorLeft.rectTransform.localPosition - targetLeftPos) < 0.001f)
            {
                isRecentering = false;
                cursorRight.rectTransform.localPosition = targetRightPos;
                cursorLeft.rectTransform.localPosition = targetLeftPos;
            }
        }
    }

    public void RecenterCursor()
    {
        targetRightPos = -centerOffset;
        targetLeftPos = centerOffset;
        rightVelocity = Vector3.zero;
        leftVelocity = Vector3.zero;
        isRecentering = true;
    }

    public void ReplaceCursor()
    {
        targetRightPos = initialRightPos;
        targetLeftPos = initialLeftPos;
        rightVelocity = Vector3.zero;
        leftVelocity = Vector3.zero;
        isRecentering = true;
    }
}
