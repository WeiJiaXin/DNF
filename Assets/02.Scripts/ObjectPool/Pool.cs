using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    private GameObject prefab;
    private List<GameObject> offline;
    private List<GameObject> work;

    public Pool(GameObject p)
    {
        prefab = p;
        offline = new List<GameObject>();
        work = new List<GameObject>();
    }

    public virtual GameObject PopObj(Transform parent)
    {
        GameObject t = null;
        if (offline.Count > 0)
        {
            do
            {
                if (offline.Count <= 0)
                    return PopObj(parent);
                t = offline[0];
                offline.RemoveAt(0);
            } while (t == null);

            t.transform.SetParent(parent);
        }
        else
        {
            t = Object.Instantiate(prefab, parent);
        }

        t.transform.localPosition = Vector3.zero;
        t.SetActive(true);
        work.Add(t);
        return t;
    }

    public virtual void PushObj(GameObject t)
    {
        work.Remove(t);
        offline.Add(t);
        t.SetActive(false);
    }
}