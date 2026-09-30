
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
    public ItemData GetById(string itemId)
    {
        foreach (ItemData item in items)
        {
            if (item != null && item.itemId == itemId)
            {
                return item;
            }
        }

        Debug.LogWarning(
            "Catalog 中找不到 Item ID: " +
            itemId
        );

        // 没有找到对应等级
        return null;
    }
}