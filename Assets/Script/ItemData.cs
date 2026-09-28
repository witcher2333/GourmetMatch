
using UnityEngine;

[CreateAssetMenu(
    fileName = "Item_",
    menuName = "Merge Cafe/Item Data"
)]
public class ItemData : ScriptableObject
{
    // 食材等级
    public int level = 1;

    // 食材名称
    public string itemName;

    // 食材图片
    public Sprite icon;

    // 没有图片时使用的背景颜色
    public Color backgroundColor = Color.white;
}