using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class inventory : MonoBehaviour
{
    public int invsize = 6;
    private item[] inv;
    private int curfreeinx = 0;

    private void Awake()
    {
        inv = new item[invsize];
    }
    public void it_add(item i)
    {
        if (checkfreeindex())
        {
            findfreeindex();
            inv[curfreeinx] = i;
            i.index = curfreeinx;
        }
    }

    public void it_add(item i, int j)
    {
        i.amount += j;
    }

    public void it_remove(item i)
    {
            inv[i.index] = null;
    }

    public void it_remove(item i, int j)
    {
            i.amount += j;
    }

    public void it_combine(item i, item j)
    {
        if(i.name == j.name)
        {
            i.amount += j.amount;
            inv[j.index] = null;
        }
    }

    private void findfreeindex()
    {
        for(int i = 0; i < inv.Length; i++)
        {
            if (inv[i] == null)
            {
                curfreeinx = i;
            }
            else if(i == inv.Length - 1)
            {
                curfreeinx = -1;
            }
        }
    }

    private bool checkfreeindex()
    {
        if(curfreeinx == -1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}
