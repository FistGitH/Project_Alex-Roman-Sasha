using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class inventory : MonoBehaviour
{
    private List<item> inv;
    private int curfreeinx = 0;

    public void it_add(item i)
    {
        findfreeindex();
        inv[curfreeinx] = i;
        i.index = curfreeinx;
    }
    public void it_remove(item i)
    {
        inv.Remove(i);
    }
    private void findfreeindex()
    {
        for(int i = 0; i < inv.Count; i++)
        {
            if (inv[i] == null)
            {
                curfreeinx = i;
            }
            else if(i == inv.Count - 1)
            {
                curfreeinx = i + 1;
            }
        }
    }

}
