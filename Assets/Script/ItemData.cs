using UnityEngine;

public enum ItemType
{
    Mergeable,
    Producer
}

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

    // 普通物品 / 生产器
    public ItemType itemType =
        ItemType.Mergeable;

    // 合成等级
    public int level = 1;

    // 玩家达到多少级后，
    // 该物品才允许进入订单
    [Min(1)]
    public int requiredPlayerLevel = 1;

    // 图片
    public Sprite icon;

    // 没图片时使用的颜色
    public Color backgroundColor =
        Color.white;

    // 合成后的下一级
    public ItemData nextItem;

    // ===== 生产器专用 =====

    // 这个生产器会生产什么
    public ItemData producedItem;

    // 每次生产消耗多少体力
    public int productionEnergyCost = 1;

    // 最大生产次数
    public int maxCharges = 5;

    // 次数耗尽后的冷却时间（秒）
    public int cooldownSeconds = 30;
}