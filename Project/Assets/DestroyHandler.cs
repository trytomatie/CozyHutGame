using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyHandler : MonoBehaviour
{
    public void DestroyGameObject(GameObject gameObject)
    {
        Destroy(gameObject);
    }
}
