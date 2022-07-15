using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    [Range(0.001f, 10f)]
    public float zoom;
    public float alt = 0;
    public float distance = 6471000;

    //public MultiScaleObjectManager target;
    public Transform target;
    public GameObject cameraL;
    public GameObject cameraS;

    private void LateUpdate()
    {
        UpdateCameraPos();
        //MultiScaleWorld.instance.AlignCamera();

        if (cameraL.GetComponent<Camera>().orthographicSize != zoom)
        {
            cameraL.GetComponent<Camera>().orthographicSize = zoom;
        }
    }

    public void UpdateCameraPos()
    {
        transform.position = (target.position + new Vector3(0, alt, 0));
    }
}
