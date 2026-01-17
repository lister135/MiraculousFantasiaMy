using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientParticles : MonoBehaviour
{
    public bool disableOnStart = true;
    [SerializeField] private float positionOffset = 7.7f;
    
    [HideInInspector] public ParticleSystem ambientParticles;
    
    // Start is called before the first frame update
    void Start()
    {
        ambientParticles = GetComponent<ParticleSystem>();
    }
        
    public void AttachToCamera()
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            gameObject.transform.parent = camera.transform;
            gameObject.transform.position = camera.transform.position + Vector3.forward * positionOffset;
        }
        else
        {
            Debug.Log("Camera not found");
        }
    }
    
    public void PlayParticles()
    {
        ambientParticles.gameObject.SetActive(true);
        ambientParticles.Play();
    }
    
    public void StopParticles()
    {
        ambientParticles.Stop();
    }
}
