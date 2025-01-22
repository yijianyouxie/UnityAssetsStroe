using Games.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPangColor : MonoBehaviour {

    public Color from;
    public Color to;

    public float step = 0.1f;
    private float currValue = 0f;

    public DynamicLight dpl;
	
	// Update is called once per frame
	void Update () {
		if(null != dpl)
        {
            currValue += step;
            if(currValue >= 1f || currValue <= 0f)
            {
                step *= -1;
            }

            dpl.lightColor = Color.Lerp(from, to, currValue);
        }
	}
}
