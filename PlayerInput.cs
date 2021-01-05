using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInput : CustomInput
{
    Camera cam;
    Vector2 offset;
    [SerializeField] float mouseSensitivity;
    float screenHeightWorldUnits;
    float screenWidthWorldUnits;
    [SerializeField] float edgeMargin;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        //offset = crosshair.transform.position - cam.transform.position;
        screenHeightWorldUnits = 2 * cam.orthographicSize;
        screenWidthWorldUnits = screenHeightWorldUnits * cam.aspect;
        screenHeightWorldUnits -= edgeMargin;
        screenWidthWorldUnits -= edgeMargin;
    }

    // Update is called once per frame
    void Update()
    {
        // Controls.
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (!Chronos.Instance.playerActive)
        {
            return;
        }

        if (!Chronos.Instance.isRewinding)
        {
            Vector2 deltaMouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            offset += deltaMouse * mouseSensitivity;
            offset.x = Mathf.Clamp(offset.x, -screenWidthWorldUnits / 2, screenWidthWorldUnits / 2);
            offset.y = Mathf.Clamp(offset.y, -screenHeightWorldUnits / 2, screenHeightWorldUnits / 2);
            aimPosition = (Vector2)cam.transform.position + offset;
        } else
        {
            offset = aimPosition - (Vector2)cam.transform.position;
        }       

        shoot = Input.GetMouseButton(0);
    }
}
