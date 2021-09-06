using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareGizmo : MonoBehaviour
{

    void OnDrawGizmos()
    {

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(0,0,0), new Vector3(0,0,1));
        Gizmos.DrawLine(new Vector3(0,0,0), new Vector3(0,1,0));
        Gizmos.DrawLine(new Vector3(0,0,0), new Vector3(1,0,0));
        Gizmos.DrawLine(new Vector3(1,1,1), new Vector3(0,1,1));
        Gizmos.DrawLine(new Vector3(1,1,1), new Vector3(1,1,0));
        Gizmos.DrawLine(new Vector3(1,1,1), new Vector3(1,0,1));

        Gizmos.DrawLine(new Vector3(1,0,0), new Vector3(1,1,0));
        Gizmos.DrawLine(new Vector3(1,0,0), new Vector3(1,0,1));

        Gizmos.DrawLine(new Vector3(0,1,0), new Vector3(1,1,0));
        Gizmos.DrawLine(new Vector3(0,1,0), new Vector3(0,1,1));

        Gizmos.DrawLine(new Vector3(0,0,1), new Vector3(0,1,1));
        Gizmos.DrawLine(new Vector3(0,0,1), new Vector3(1,0,1));
    }

}
