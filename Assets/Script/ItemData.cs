using UnityEngine;

[CreateAssetMenu(
    fileName = "Item_",
    menuName = "Merge Cafe/Item Data"
)]
public class ItemData : ScriptableObject
{
    // 唯一 ID
    public string itemId;

    // 显示名称
    public string itemName;

    // 等级
    public int level = 1;

    // 图片
    public Sprite icon;

    // 没有图片时使用这个颜色
    public Color backgroundColor = Color.white;

    // 合成后的下一级物品
    public ItemData nextItem;
}