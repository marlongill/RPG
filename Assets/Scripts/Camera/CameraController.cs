using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;
    public float Smoothing;
    public Vector2 minPosition;
    public Vector2 maxPosition;

    void LateUpdate()
    {
        if (transform.position != Target.position)
        {
            Vector3 targetPosition = new Vector3(
                Mathf.Clamp(Target.position.x, minPosition.x, maxPosition.x),
                Mathf.Clamp(Target.position.y, minPosition.y, maxPosition.y),
                -20
            );
            transform.position = Vector3.Lerp(transform.position, targetPosition, Smoothing);
        }
    }
}
