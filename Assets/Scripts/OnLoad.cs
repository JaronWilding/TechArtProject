using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
[Serializable]
public class OnLoad : MonoBehaviour
{
    
    public static void Build(int amount, GameObject pivot, GameObject box, float far)
    {
        GameObject last = new GameObject("Root");
        GameObject root = last;
        root.AddComponent<MultiInverse>();
        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(pivot);
            obj.name = string.Format("Pivot_{0}", i);
            if(i == 0)
            {
               // root.GetComponent<MultiInverse>().BaseJoint = obj.transform;
            }
            
            if (last.name == "Root")
            {
                obj.transform.SetParent(last.transform);
            }
            else
            {
                obj.transform.SetParent(last.transform);
                obj.transform.localPosition = new Vector3(0f, 0f, far);
            }
            if(i != amount - 1)
            {
                last = obj;
                //root.GetComponent<MultiInverse>().lastJoint = last.transform;
            }
            else
            {
                obj.GetComponent<GizmoDraw>().radius = 0.2f;
                obj.GetComponent<GizmoDraw>().colour = Color.red;
                obj.name = "Locator";
                obj.transform.SetParent(null);
                root.GetComponent<MultiInverse>().Target = obj.transform;
            }
            
        }

        
    }
}
