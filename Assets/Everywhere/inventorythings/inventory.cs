using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class inventory : MonoBehaviour
{
    public Image slot1; 
    public Image slot2; 
    public Image slot3; 
    public Image slot4; 
    public Image slot5; 
    public Image slot6; 
    public int invsize = 6;
    public item[] inv;
    private Image[] slotpics;
    public int curfreeinx = 0;

    private void Awake()
    {
        inv = new item[invsize];
        slotpics = new Image[invsize];
        slotpics[0] = slot1;
        slotpics[1] = slot2;
        slotpics[2] = slot3;
        slotpics[3] = slot4;
        slotpics[4] = slot5;
        slotpics[5] = slot6;
    }
    public void it_add(item i)
    {
        findfreeindex();
        if (checkfreeindex())
        {
            inv[curfreeinx] = i;
            i.index = curfreeinx;
            showumages();
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
                return;
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

    private void showumages()
    {
        for(int i = 0; i < inv.Length; i++)
        {
            if (inv[i] != null && slotpics[i] != null)
            {
                slotpics[i].sprite = inv[i].pic;
            }
        }
    }

}
