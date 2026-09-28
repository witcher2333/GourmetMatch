
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderManager : MonoBehaviour
{
    // 棋盘系统
    [SerializeField]
    private MergeBoard board;

    // 食材配置总表
    [SerializeField]
    private ItemCatalog catalog;

    // 订单文字
    [SerializeField]
    private TMP_Text orderText;

    // 金币文字
    [SerializeField]
    private TMP_Text coinText;

    // 提交订单按钮
    [SerializeField]
    private Button submitButton;

    // 当前订单需要的食材等级
    private int currentOrderLevel = 2;

    // 当前金币数量
    private int coins = 0;

    private void Start()
    {
        UpdateUI();
    }

    // 玩家点击提交订单时调用
    public void CompleteOrder()
    {
        if (board == null || catalog == null)
        {
            Debug.LogError("订单系统缺少 Board 或 Catalog！");
            return;
        }

        // 第一步：检查棋盘是否有需要的食材
        int itemCount =
            board.CountItemsByLevel(currentOrderLevel);

        if (itemCount < 1)
        {
            Debug.Log("订单无法完成：缺少对应食材！");
            return;
        }

        // 第二步：尝试消耗食材
        bool success =
            board.TryConsumeItem(currentOrderLevel);

        if (!success)
        {
            Debug.LogWarning("食材消耗失败！");
            return;
        }

        // 第三步：发放奖励
        int reward = GetReward(currentOrderLevel);

        coins += reward;

        Debug.Log(
            "订单完成！获得 " + reward + " 金币"
        );

        // 第四步：生成下一笔订单
        GenerateNextOrder();

        // 第五步：更新界面
        UpdateUI();
    }

    // 根据订单难度计算金币奖励
    private int GetReward(int level)
    {
        switch (level)
        {
            case 2:
                return 10;

            case 3:
                return 25;

            case 4:
                return 50;

            default:
                return 5;
        }
    }

    // 生成下一笔订单
    private void GenerateNextOrder()
    {
        if (currentOrderLevel == 2)
        {
            currentOrderLevel = 3;
        }
        else
        {
            currentOrderLevel = 2;
        }
    }

    // 更新订单与金币显示
    private void UpdateUI()
    {
        // 更新金币
        if (coinText != null)
        {
            coinText.text = "Coins: " + coins;
        }

        // 如果还没有设置食材总表
        if (catalog == null)
        {
            Debug.LogError("没有设置 Item Catalog！");
            return;
        }

        // 查找当前订单的食材数据
        ItemData data =
            catalog.GetByLevel(currentOrderLevel);

        // 找不到食材配置时，禁用提交按钮
        if (data == null)
        {
            if (orderText != null)
            {
                orderText.text = "Invalid Order";
            }

            if (submitButton != null)
            {
                submitButton.interactable = false;
            }

            return;
        }

        // 获取本次订单的金币奖励
        int reward = GetReward(currentOrderLevel);

        // 更新订单文字
        if (orderText != null)
        {
            orderText.text =
                "Order: 1 x " + data.itemName +
                " (Lv." + currentOrderLevel + ")" +
                "\nReward: " + reward + " Coins";
        }

        // 订单有效，允许点击提交
        if (submitButton != null)
        {
            submitButton.interactable = true;
        }
    }
}
