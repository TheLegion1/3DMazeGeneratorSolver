using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCamera : MonoBehaviour
{
    Vector3 last;
    Vector3 current;
    public float rotationSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
        last = transform.position;
        current = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        current = transform.position;
        Vector3 dir = (last - current).normalized;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(-dir),
                Time.deltaTime * rotationSpeed
            );
        }

        last = current;
    }
}
