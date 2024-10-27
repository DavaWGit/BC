/**
 * Autor: David Zahálka
 *
 * Skript pohybu kamery
 * 
 * Tento skript řídí pohyb herní kamery. Udržuje kameru zaměřenou na cílový objekt (hráče)
 * a zároveň zajišťuje, že kamera zůstává uvnitř hranic mapy.
 * 
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public Tilemap tilemap;
    public Vector3 bottomLeft, topRight;
    private float halfHeight, halfWidth;
    // Start is called before the first frame update
    void Start()
    {
        // omezení kamery na plochu mapy
        target = PlayerMovement.instance.transform;

        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Camera.main.aspect;

        bottomLeft = tilemap.localBounds.min + new Vector3(halfWidth, halfHeight, 0f);
        topRight = tilemap.localBounds.max + new Vector3(-halfWidth, -halfHeight, 0f);

        PlayerMovement.instance.SetBounds(tilemap.localBounds.min, tilemap.localBounds.max);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(target != null)
        {
            // kamera následuje hráče
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, bottomLeft.x, topRight.x), Mathf.Clamp(transform.position.y, bottomLeft.y, topRight.y), transform.position.z);
        }
    }
}
