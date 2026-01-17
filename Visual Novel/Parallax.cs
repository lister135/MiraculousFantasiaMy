using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// COMPLETELY REFACTOR
public class Parallax : MonoBehaviour
{
    private float length;
    private float startpos;
    
    public GameObject camera;

    [SerializeField] private float yScaleFactor = 1;
    public float parallaxValue;
    
    // Start is called before the first frame update
    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        // fix this, this is not good
        if (camera)
        {
            float distanceX = camera.transform.position.x * parallaxValue;
            transform.position = new Vector3(startpos + distanceX, yScaleFactor == 0 ? transform.position.y : transform.position.y * yScaleFactor, transform.position.z);
        }        
    }
}
