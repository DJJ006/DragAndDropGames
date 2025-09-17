using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectScript : MonoBehaviour
{
    public GameObject[] vehicles;
    [HideInInspector]
    public Vector2[] startCoordinates;
    public Canvas can;
    public AudioSource effects;
    public AudioClip[] audioCli;
    [HideInInspector]
    public bool rightPlace = false;
    public GameObject lastDragged = null;
    public bool drag = false;
    
    void Awake()
    {
        Debug.Log(vehicles.Length);
        Debug.Log(startCoordinates.Length);
        startCoordinates = new Vector2[vehicles.Length];
        for(int i=0; i<vehicles.Length; i++)
        {
            startCoordinates[i] = vehicles[i].GetComponent<RectTransform>().localPosition;
        }
        
    }

    
    void Update()
    {
        
    }
}
