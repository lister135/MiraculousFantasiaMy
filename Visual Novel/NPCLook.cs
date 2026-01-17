using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class NPCLook : MonoBehaviour
{
    private GameObject trackedObject;
    private bool isFacingRight = true;

    void Start()
    {
        trackedObject = GameObject.FindGameObjectWithTag("Player");
    }
    
    // Update is called once per frame
    void Update()
    {
        if (trackedObject)
        {
            if (trackedObject.transform.position.x < gameObject.transform.position.x && isFacingRight ||
                trackedObject.transform.position.x > gameObject.transform.position.x && !isFacingRight)
            {
                Flip();
            }
        }
    }
    
    private void Flip()
    {
        // flip the direction first
        isFacingRight = !isFacingRight;
        
        // check direction character is facing, then rotate accordingly
        transform.eulerAngles = new Vector3(0, isFacingRight ? 0 : 180, 0);
    }
}
