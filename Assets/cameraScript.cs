using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraScript : MonoBehaviour
{
    public GameObject target;
    public float floatDistance;
    public float movementSpeed = 5;
    public float rotateSpeed = 15;
    public bool rotateAroundObject = false;
    public bool follow = true;
    public Vector3 rotateTarget;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rotateAroundObject)
        {
            transform.LookAt(rotateTarget);
            transform.Translate(Vector3.right * Time.deltaTime * rotateSpeed);
        }
        else {
            if (follow)
            {
                if (target != null) {
                    transform.position = new Vector3(target.transform.position.x, target.transform.position.y + floatDistance, target.transform.position.z);
                    transform.LookAt(target.transform.position);
                }
                
            }
            else {
                //static world cam 
                Vector3 targetPos = new Vector3(rotateTarget.x, rotateTarget.y + floatDistance, rotateTarget.z);
                transform.position = targetPos;
                transform.LookAt(rotateTarget);
            }
            
        }
        
    }

}
