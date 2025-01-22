using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSelf : MonoBehaviour {

    public enum Axis
    {
        X,
        Y,
        Z,
    }
    private Transform trans = null;
    public float speed = 0.5f;
    public float angle = 20f;

    public Axis axis = Axis.X;
    // Use this for initialization
    void Start()
    {
        trans = transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(axis == Axis.X)
        {
            trans.Rotate(new Vector3(angle * Time.deltaTime * speed, 0, 0));

        }else if(axis == Axis.Y)
        {
            trans.Rotate(new Vector3(0, angle * Time.deltaTime * speed, 0));
        }
        else if(axis == Axis.Z)
        {
            //trans.Rotate(trans.transform.forward, angle * Time.deltaTime * speed);
            trans.Rotate(new Vector3(0, 0, angle * Time.deltaTime * speed));
        }
    }
}
