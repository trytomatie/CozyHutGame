using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Helper : MonoBehaviour
{

    /// <summary>
    /// Checks the ground beneath the character
    /// </summary>
    /// <param name="position"></param>
    /// <param name="cc"></param>
    /// <param name="layerMask"></param>
    /// <param name="distance"></param>
    /// <param name="scaleFactor"></param>
    /// <returns></returns>
    public static bool CheckBeneath(Vector3 position, CharacterController cc, LayerMask layerMask, float distance, float scaleFactor)
    {
        RaycastHit raycastHit;
        if (Physics.SphereCast(position + new Vector3(0,(cc.radius + cc.skinWidth) * scaleFactor,0),(cc.radius + cc.skinWidth) * scaleFactor, new Vector3(0, -1, 0), out raycastHit, distance, layerMask))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns distance between two gameobjects
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="distanceObject"></param>
    /// <returns></returns>
    public static float DistanceBetween(GameObject origin, GameObject distanceObject)
    {
        float distance = Vector3.Distance(distanceObject.transform.position, origin.transform.position);
        return distance;
    }

    /// <summary>
    /// returns Angle beteween 2 objects
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="angleObject"></param>
    /// <returns></returns>
    public static float AngleBetween(GameObject origin, GameObject angleObject)
    {

        Vector3 from = new Vector3(angleObject.transform.position.x, 0, angleObject.transform.position.z) - new Vector3(origin.transform.position.x, 0, origin.transform.position.z);
        float angle = Vector3.Angle(from, origin.transform.forward);
        return angle;
    }

}
