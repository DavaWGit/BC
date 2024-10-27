using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = cursorPos;
        if(UIController.instance.isMenuOpen || !LevelManager.instance.gameActive)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
    }
}
