using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class OnClickEffect : MonoBehaviour
{
    [SerializeField] private VisualEffect vfxGraphAsset = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void TriggerVFX()
    {
        Vector3 mousePosition = Input.mousePosition;

        Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        spawnPosition.z = 0f;
        VisualEffect vfxGraphInstance = Instantiate(vfxGraphAsset);

        vfxGraphInstance.transform.position = spawnPosition;

        vfxGraphInstance.Play();

    }

}
