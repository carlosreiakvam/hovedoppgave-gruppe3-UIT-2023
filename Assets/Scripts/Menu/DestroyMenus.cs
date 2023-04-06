using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyMenus : MonoBehaviour
{
    /*        
        The menus gameobject from the last programatically create scripts.
        It is therefore neccessary to destroy the gameobject manualy.
        This happens here in order to not get an empty screen in between scene changes.
*/

    void Start()
    {
        GameObject menus = GameObject.Find("Menus");
        if (menus != null)
        {
            Debug.Log("destroying menus");
            Destroy(menus);
        }
    }

}
