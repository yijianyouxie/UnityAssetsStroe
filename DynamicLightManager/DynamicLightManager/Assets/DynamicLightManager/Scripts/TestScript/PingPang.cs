using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PingPang : MonoBehaviour {

    public enum AxisBy
    {
        x = 0,
        y = 1,
        z = 2,
    }

    public AxisBy axisBy;
    public float moveDis = 10f;
    public float moveSpeed = 1f;

    private Transform tr;
    private Vector3 originalPos;
	// Use this for initialization
	void Start () {
        tr = transform;
        originalPos = tr.position;

    }
	
	// Update is called once per frame
	void Update () {
		if(axisBy == AxisBy.x)
        {
            tr.position = tr.position + new Vector3(1,0,0)* moveSpeed;
            if(tr.position.x >= originalPos.x + moveDis)
            {
                tr.position = originalPos;
            }
        }else if(axisBy == AxisBy.y)
        {

        }else if(axisBy == AxisBy.z)
        {
            tr.position = tr.position + new Vector3(0, 0, 1) * moveSpeed;
            if (tr.position.z >= originalPos.z + moveDis)
            {
                tr.position = originalPos;
            }
        }
	}
}
