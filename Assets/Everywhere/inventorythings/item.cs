using UnityEngine;

[CreateAssetMenu(fileName = "item", menuName = "Game/item")]
public class item : ScriptableObject
{
    public string name;
    [SerializeField] public Sprite pic;
    public int index;
    public int amount;
}
