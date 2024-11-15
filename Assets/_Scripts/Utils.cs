using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static void DetroyAndRemoveFromList(List<GameObject> list, GameObject target)
    {
        list.Remove(target);
        Destroy(target);
    }

    public static void DestroyAndRemoveAllFromList(List<GameObject> list)
    {
        foreach (var i in list) Destroy(i.gameObject);
        list.Clear();
    }
}
