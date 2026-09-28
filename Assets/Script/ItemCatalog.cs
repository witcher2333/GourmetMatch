
using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemCatalog",
    menuName = "Merge Cafe/Item Catalog"
)]
public class ItemCatalog : ScriptableObject
{
    [SerializeField]
    private ItemData[] items;

    // 根据等级查找食材
    public ItemData GetByLevel(int level)
    {
        foreach (ItemData item in items)
        {
            if (item != null && item.level == level)
            {
                return item;
            }
        }

        // 没有找到对应等级
        return null;
    }
}