using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class inventory : MonoBehaviour
{ 
    public int invsize = 6;
    public item[] inv;
    public List<Image> slotpics;
    public List<TextMeshProUGUI> amounts;
    public int curfreeinx = 0;

    private void Awake()
    {
        inv = new item[invsize];
    }
    public void it_add(item i)
    {
        findfreeindex();
        if (checkfreeindex())
        {
            int a = ininv(i);
            if (a != -1)
            {
                inv[a].amount += i.amount;
                showamounts();
            }
            else
            {
                inv[curfreeinx] = i;
                i.index = curfreeinx;
                showumages();
            }
                    
        }
    }

    public void test2()
    {
        it_remove(inv[0], 1);
    }
    public void test4()
    {
        it_remove(inv[0]);
    }


    public void it_remove(item i)
    {
            if(i != null)
            {
                inv[i.index] = null;
                showumages();
                showamounts();
            }
    }
    public void it_remove(item i, int j)
    {
        if (i != null)
        {
            i.amount -= j;
            showamounts();
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

    private int ininv(item it)
    {
        for(int i =0; i<inv.Length; i++)
        {
            if(inv[i] != null && it.data == inv[i].data)
            {
                return i;
            }
        }
        return -1;
    }

    private void showumages()
    {
        for(int i = 0; i < inv.Length; i++)
        {
            if (inv[i] != null && slotpics[i] != null)
            {
                slotpics[i].sprite = inv[i].data.pic;
            }
            if (inv[i] == null && slotpics[i] != null)
            {
                slotpics[i].sprite = null;
            }
        }
    }

    private void showamounts()
    {
        for(int i = 0;i < inv.Length; i++)
        {
            if(inv[i] != null && amounts[i] != null)
            {
                if (inv[i].amount == 1)
                {
                    amounts[i].text = " ";
                }
                else if(inv[i].amount == 0)
                {
                    inv[i] = null;
                }
                else
                {
                    amounts[i].text = inv[i].amount.ToString();
                }
            }
            if (inv[i] == null && amounts[i] != null)
            {
                amounts[i].text = " ";
            }
        }
    } 

}
