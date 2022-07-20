using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [Range(0.001f, 10f)]
    public float zoom;
    public float alt = 0;

    public Transform target;
    public GameObject cameraObj;

    private void LateUpdate()
    {
        if(target == null) { return; }
        UpdateCameraPos();

        if (cameraObj.GetComponent<Camera>().orthographicSize != zoom)
        {
            cameraObj.GetComponent<Camera>().orthographicSize = zoom;
        }
    }

    public void UpdateCameraPos()
    {
        transform.position = (target.position + new Vector3(0, alt, 0));
    }
}
