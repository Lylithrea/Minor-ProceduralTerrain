using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClickableNode : MonoBehaviour
{
    public bool isEnabled = false;
    private bool firstHit = false;

    private void OnDrawGizmos()
    {
        //add stuff here to do with enabled
        if(Selection.activeObject != null && Selection.activeObject.name == this.transform.name)
        {

        }
        else
        {
            firstHit = false;
        }
        if (isEnabled)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawSphere(this.transform.position, 0.05f);

    }

    private void OnDrawGizmosSelected()
    {
        if (!firstHit)
        {
            firstHit = true;
            isEnabled = !isEnabled;
        }
    }
}
