using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class inventory : MonoBehaviour
{
    [SerializeField] private InputActionReference drop;
    public int invsize = 6;
    public item[] inv;
    public List<Image> slotpics;
    public List<TextMeshProUGUI> amounts;
    private int curfreeinx = 0;
    private int cci = 0;

    private void Awake()
    {
        inv = new item[invsize];
    }

    private void Update()
    {
        if (drop.action.IsPressed())
        {
            it_remove(inv[cci], 1);
        }
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
                showumages();
                showamounts();
            }
                    
        }
    }

    public void it_remove(item i)
    {
        int a = ininv(i);
        if(a != -1)
        {
            inv[a] = null;
            showumages();
            showamounts();
        }
    }

    public void it_remove(item i, int j)
    {
        if (ininv(i) != -1)
        {
            i.amount -= j;
            if(i.amount <= 0)
            {
                it_remove(i);
                showumages();
            }
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
