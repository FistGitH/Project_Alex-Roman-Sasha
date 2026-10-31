using UnityEngine;

[CreateAssetMenu(fileName = "itemdata", menuName = "Game/itemdata")]
public class itemdata : ScriptableObject
{
    public string name;
    [SerializeField] public Sprite pic;
}
