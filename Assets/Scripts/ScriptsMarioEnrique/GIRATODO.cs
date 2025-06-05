using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GIRATODO : MonoBehaviour
{
    public Vector3 rotationSpeed;

    void FixedUpdate()
    {
        transform.Rotate(rotationSpeed);
    }
}
