using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SortingObject : MonoBehaviour
{
    public int orderIdx
    {
        get
        {
            if (r == null)
            {
                r = this.GetComponent<Renderer>();
                if (r)
                {
                    curIndex = r.sortingOrder;
                }
            }

            return curIndex;
        }
    }
    private int curIndex = 0;
    private Renderer r = null;

    void OnDestroy()
    {
        SetSortIndex(0);
    }

    void SetSortIndex(int n_index)
    {
        if (r == null)
            r = this.GetComponent<Renderer>();
        if (r != null)
        {
            r.sortingOrder = curIndex;
        }
    }
}