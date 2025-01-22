using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPangRotation : MonoBehaviour {
    public enum AxisBy
    {
        x = 0,
        y = 1,
        z = 2,
    }
    public AxisBy axisBy;
    private Transform trs;
    private Vector3 originalEuler;
    private Vector3 currEnuler;

    public float rotateSpeed = 0.2f;
    public float maxRotate = 60f;

    // Use this for initialization
    void Start () {
        trs = transform;

        originalEuler = trs.localEulerAngles;
        currEnuler = originalEuler;

    }
	
	// Update is called once per frame
	void Update () {
        float curr = 0;
        Vector3 currRotate = Vector3.zero;
        if (axisBy == AxisBy.x)
        {
            curr = currEnuler.x + rotateSpeed;
            if(curr >= originalEuler.x + maxRotate)
            {
                curr = originalEuler.x + maxRotate;
                rotateSpeed *= -1;
            }
            if (curr <= originalEuler.x - maxRotate)
            {
                curr = originalEuler.x - maxRotate;
                rotateSpeed *= -1;
            }
            currEnuler.x = curr;
            currRotate = new Vector3(curr, originalEuler.y, originalEuler.z);
        }
        if (axisBy == AxisBy.y)
        {
            curr = currEnuler.y + rotateSpeed;
            if (curr >= originalEuler.y + maxRotate)
            {
                curr = originalEuler.y + maxRotate;
                rotateSpeed *= -1;
            }
            if (curr <= originalEuler.y - maxRotate)
            {
                curr = originalEuler.y - maxRotate;
                rotateSpeed *= -1;
            }
            currEnuler.y = curr;
            currRotate = new Vector3(originalEuler.x, curr, originalEuler.z);
        }
        if (axisBy == AxisBy.z)
        {
            curr = currEnuler.z + rotateSpeed;
            if (curr >= originalEuler.z + maxRotate)
            {
                curr = originalEuler.z + maxRotate;
                rotateSpeed *= -1;
            }
            if (curr <= originalEuler.z - maxRotate)
            {
                curr = originalEuler.z - maxRotate;
                rotateSpeed *= -1;
            }
            currEnuler.z = curr;
            currRotate = new Vector3(originalEuler.x, originalEuler.y, curr);
        }

        trs.localEulerAngles = currRotate;
	}
}
