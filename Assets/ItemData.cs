using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "AnotherWorld/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite sprite;
    [TextArea] public string description;
}
