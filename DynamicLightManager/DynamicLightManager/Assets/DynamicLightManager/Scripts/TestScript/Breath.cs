using Games.Manager;
using UnityEngine;

public class Breath : MonoBehaviour {

    DynamicLight dpl;
    public float maxLightIntensity = 2f;
    public float minLightIntensity = 0.2f;
    public float step = 0.02f;
	// Use this for initialization
	void Start () {
        dpl = GetComponent<DynamicLight>();

    }
	
	// Update is called once per frame
	void Update () {
		if(null != dpl)
        {
            dpl.lightIntensity += step;
            if(dpl.lightIntensity > maxLightIntensity || dpl.lightIntensity < minLightIntensity)
            {
                step *= -1;
            }
        }
	}
}
