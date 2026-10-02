using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class OrderRequirement
{
    // 需要的物品
    public ItemData item;

    // 需要的数量
    public int amount;
}

public class OrderManager : MonoBehaviour
{
    [SerializeField]
    private MergeBoard board;

    // Day10 开始，这个数组代表：
    // “可以随机出现在订单里的物品”
    [SerializeField]
    private ItemData[] orderItems;

    [SerializeField]
    private TMP_Text orderText;

    [SerializeField]
    private TMP_Text coinText;

    [SerializeField]
    private Button submitButton;

    // 一个订单最多包含多少种物品
    [SerializeField]
    private int maxRequirementTypes = 2;

    // 当前订单的所有要求
    private List<OrderRequirement>
        currentRequirements =
            new List<OrderRequirement>();

    // 当前订单奖励
    private int currentReward = 0;

    // 当前金币
    private int coins = 0;

    private void Start()
    {
        // 新游戏没有订单时，
        // 自动生成第一份订单
        if (currentRequirements.Count == 0)
        {
            GenerateNextOrder();
        }

        UpdateUI();
    }

    private void Update()
    {
        // 棋盘内容会因为生产、合成发生变化，
        // 所以持续刷新提交按钮是否可用。
        if (submitButton != null)
        {
            submitButton.interactable =
                CanCompleteCurrentOrder();
        }
    }

    public void CompleteOrder()
    {
        if (board == null)
        {
            Debug.LogError(
                "OrderManager 没有设置 Board！"
            );

            return;
        }

        if (currentRequirements.Count == 0)
        {
            Debug.LogWarning(
                "当前没有有效订单！"
            );

            return;
        }

        // 第一阶段：只检查，不消耗
        if (!CanCompleteCurrentOrder())
        {
            LogMissingItems();
            return;
        }

        // 保存奖励，避免生成新订单后数值改变
        int completedReward =
            currentReward;

        // 第二阶段：所有物品都够，
        // 才开始统一消耗
        foreach (
            OrderRequirement requirement
            in currentRequirements
        )
        {
            for (
                int i = 0;
                i < requirement.amount;
                i++
            )
            {
                bool consumed =
                    board.TryConsumeItem(
                        requirement.item
                    );

                if (!consumed)
                {
                    Debug.LogError(
                        "订单物品消耗失败：" +
                        requirement.item.itemName
                    );

                    return;
                }
            }
        }

        coins += completedReward;

        Debug.Log(
            "订单完成，获得 " +
            completedReward +
            " Coins"
        );

        GenerateNextOrder();
        UpdateUI();
    }

    private bool CanCompleteCurrentOrder()
    {
        if (board == null ||
            currentRequirements.Count == 0)
        {
            return false;
        }

        foreach (
            OrderRequirement requirement
            in currentRequirements
        )
        {
            if (requirement == null ||
                requirement.item == null ||
                requirement.amount <= 0)
            {
                return false;
            }

            int currentAmount =
                board.CountItems(
                    requirement.item
                );

            if (currentAmount <
                requirement.amount)
            {
                return false;
            }
        }

        return true;
    }

    private void LogMissingItems()
    {
        foreach (
            OrderRequirement requirement
            in currentRequirements
        )
        {
            if (requirement == null ||
                requirement.item == null)
            {
                continue;
            }

            int currentAmount =
                board.CountItems(
                    requirement.item
                );

            if (currentAmount <
                requirement.amount)
            {
                int missing =
                    requirement.amount -
                    currentAmount;

                Debug.Log(
                    "缺少订单物品：" +
                    requirement.item.itemName +
                    " × " +
                    missing
                );
            }
        }
    }

    private void GenerateNextOrder()
    {
        currentRequirements.Clear();

        List<ItemData> candidates =
            new List<ItemData>();

        // 从 Inspector 的 orderItems 中
        // 建立合法候选池
        if (orderItems != null)
        {
            foreach (ItemData item in orderItems)
            {
                if (item == null)
                {
                    continue;
                }

                // Producer 不能成为订单物品
                if (item.itemType !=
                    ItemType.Mergeable)
                {
                    continue;
                }

                // 避免候选池重复
                if (!candidates.Contains(item))
                {
                    candidates.Add(item);
                }
            }
        }

        if (candidates.Count == 0)
        {
            currentReward = 0;

            Debug.LogError(
                "订单候选池为空！"
            );

            return;
        }

        int maximumTypes =
            Mathf.Clamp(
                maxRequirementTypes,
                1,
                candidates.Count
            );

        // Random.Range 的整数最大值不包含在内，
        // 所以这里写 maximumTypes + 1。
        int requirementTypeCount =
            UnityEngine.Random.Range(
                1,
                maximumTypes + 1
            );

        for (
            int i = 0;
            i < requirementTypeCount;
            i++
        )
        {
            int randomIndex =
                UnityEngine.Random.Range(
                    0,
                    candidates.Count
                );

            ItemData selectedItem =
                candidates[randomIndex];

            // 选过以后移除，
            // 防止同一个订单重复选到同一种物品
            candidates.RemoveAt(
                randomIndex
            );

            OrderRequirement requirement =
                new OrderRequirement();

            requirement.item =
                selectedItem;

            requirement.amount =
                GetRandomAmount(
                    selectedItem
                );

            currentRequirements.Add(
                requirement
            );
        }

        currentReward =
            CalculateCurrentReward();
    }

    private int GetRandomAmount(
        ItemData item
    )
    {
        if (item == null)
        {
            return 1;
        }

        // Lv.1 和 Lv.2 可以要求 1～2 个
        if (item.level <= 2)
        {
            return UnityEngine.Random.Range(
                1,
                3
            );
        }

        // Lv.3 以上先只要求 1 个
        return 1;
    }

    private int CalculateCurrentReward()
    {
        int reward = 0;

        foreach (
            OrderRequirement requirement
            in currentRequirements
        )
        {
            if (requirement == null ||
                requirement.item == null)
            {
                continue;
            }

            int safeLevel =
                Mathf.Max(
                    1,
                    requirement.item.level
                );

            int safeAmount =
                Mathf.Max(
                    1,
                    requirement.amount
                );

            reward +=
                safeLevel *
                safeAmount *
                10;
        }

        // 多种物品订单增加组合奖励
        if (currentRequirements.Count > 1)
        {
            reward += 10;
        }

        return reward;
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text =
                "Coins: " + coins;
        }

        if (currentRequirements.Count == 0)
        {
            if (orderText != null)
            {
                orderText.text =
                    "No Order";
            }

            if (submitButton != null)
            {
                submitButton.interactable =
                    false;
            }

            return;
        }

        if (orderText != null)
        {
            StringBuilder builder =
                new StringBuilder();

            builder.AppendLine(
                "Order:"
            );

            foreach (
                OrderRequirement requirement
                in currentRequirements
            )
            {
                if (requirement == null ||
                    requirement.item == null)
                {
                    continue;
                }

                builder.AppendLine(
                    requirement.amount +
                    " x " +
                    requirement.item.itemName
                );
            }

            builder.Append(
                "Reward: " +
                currentReward +
                " Coins"
            );

            orderText.text =
                builder.ToString();
        }

        if (submitButton != null)
        {
            submitButton.interactable =
                CanCompleteCurrentOrder();
        }
    }

    // 增加金币
    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        coins += amount;

        UpdateUI();

        Debug.Log(
            "获得 " +
            amount +
            " Coins，当前金币：" +
            coins
        );
    }

    // 是否有足够金币
    public bool CanAfford(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        return coins >= amount;
    }

    // 尝试消费金币
    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (coins < amount)
        {
            Debug.Log(
                "金币不足，需要 " +
                amount +
                " Coins，当前只有 " +
                coins
            );

            return false;
        }

        coins -= amount;

        UpdateUI();

        Debug.Log(
            "消费 " +
            amount +
            " Coins，剩余：" +
            coins
        );

        return true;
    }

    public int GetCoins()
    {
        return coins;
    }

    public int GetCurrentOrderReward()
    {
        return currentReward;
    }

    public List<OrderRequirementSaveData>
        GetOrderSaveData()
    {
        List<OrderRequirementSaveData>
            savedRequirements =
                new List<OrderRequirementSaveData>();

        foreach (
            OrderRequirement requirement
            in currentRequirements
        )
        {
            if (requirement == null ||
                requirement.item == null)
            {
                continue;
            }

            OrderRequirementSaveData saveData =
                new OrderRequirementSaveData();

            saveData.itemId =
                requirement.item.itemId;

            saveData.amount =
                requirement.amount;

            savedRequirements.Add(
                saveData
            );
        }

        return savedRequirements;
    }

    public void LoadState(
        int savedCoins,
        List<OrderRequirementSaveData>
            savedRequirements,
        int savedReward,
        ItemCatalog catalog
    )
    {
        coins =
            Mathf.Max(
                0,
                savedCoins
            );

        currentRequirements.Clear();

        if (savedRequirements != null &&
            catalog != null)
        {
            foreach (
                OrderRequirementSaveData saved
                in savedRequirements
            )
            {
                if (saved == null ||
                    string.IsNullOrEmpty(
                        saved.itemId
                    ) ||
                    saved.amount <= 0)
                {
                    continue;
                }

                ItemData item =
                    catalog.GetById(
                        saved.itemId
                    );

                if (item == null ||
                    item.itemType !=
                        ItemType.Mergeable)
                {
                    continue;
                }

                // 如果异常存档里同一种物品出现两次，
                // 加载时把数量合并
                OrderRequirement existing =
                    currentRequirements.Find(
                        requirement =>
                            requirement.item ==
                            item
                    );

                if (existing != null)
                {
                    existing.amount +=
                        saved.amount;
                }
                else
                {
                    OrderRequirement requirement =
                        new OrderRequirement();

                    requirement.item =
                        item;

                    requirement.amount =
                        saved.amount;

                    currentRequirements.Add(
                        requirement
                    );
                }
            }
        }

        // 兼容旧存档：
        // 旧存档没有 orderRequirements，
        // 就生成一个新订单。
        if (currentRequirements.Count == 0)
        {
            GenerateNextOrder();
        }
        else
        {
            // 奖励缺失或非法时重新计算
            if (savedReward > 0)
            {
                currentReward =
                    savedReward;
            }
            else
            {
                currentReward =
                    CalculateCurrentReward();
            }
        }

        UpdateUI();
    }
}