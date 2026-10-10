using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "item", menuName = "Game/item")]
public class item : ScriptableObject
{
    public string name;
    [SerializeField] public Image pic;
    public int index;
    public int amount;
}
