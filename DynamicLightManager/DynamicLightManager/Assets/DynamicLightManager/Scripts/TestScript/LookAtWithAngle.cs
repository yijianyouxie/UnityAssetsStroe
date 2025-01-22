using UnityEngine;

[ExecuteInEditMode]
public class LookAtWithAngle : MonoBehaviour {

    private Transform trans;
    //private Quaternion lastQtn;
    // Use this for initialization

    public Transform mainUICamera;
	void Start () {
        trans = transform;
        trans.localEulerAngles = new Vector3(-90f, 0, 0);
        //lastQtn = trans.localRotation;
    }
	
	// Update is called once per frame
	void LateUpdate () {
        //Debug.LogError("========" + transform.forward + " :" + transform.up + " :" + transform.right);
        Transform camTrans = null;
        if (null != Camera.main)
        {
            camTrans = Camera.main.transform;
        }
        else
        {
            return;
        }
        var camera = camTrans;
        Vector3 direction = camera.position - trans.position;
        Vector3 normal = Vector3.Cross(direction, trans.up);
        Vector3 normal2 = Vector3.Cross(trans.up, normal);
        //normal2 = Vector3.Normalize(normal2);
        //Vector3 transForwardNormal = Vector3.Normalize(trans.forward);
        //float cosin = Vector3.Dot(normal2, transForwardNormal);

        Quaternion lookRotation = Quaternion.LookRotation(normal2, trans.up);
        //Debug.LogError("========" + cosin + " :" + Mathf.Acos(cosin) * 180 / Mathf.PI + " :" + Vector3.Angle(transForwardNormal, normal2));
        //transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
        //transform.localEulerAngles = new Vector3(0f, Mathf.Acos(cosin) * 180 / Mathf.PI, 0f);
        //Quaternion desQtn = Quaternion.Euler(0f, Mathf.Acos(cosin) * 180 / Mathf.PI, 0f);
        //trans.localRotation = desQtn;// Quaternion.Lerp(lastQtn, desQtn, 0.5f);
        trans.rotation = lookRotation;// Quaternion.Lerp(lastQtn, desQtn, 0.5f);
        //lastQtn = trans.localRotation;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawLine(transform.position, Camera.main.transform.position);

    //    Vector3 direction = Camera.main.transform.position - transform.position;
    //    Vector3 normal = Vector3.Cross(direction, transform.up);
    //    Gizmos.DrawLine(transform.position, transform.position + normal);

    //    Gizmos.color = Color.cyan;
    //    Vector3 normal2 = Vector3.Cross(transform.up, normal);
    //    Gizmos.DrawLine(transform.position, transform.position + normal2);
    //}
}
