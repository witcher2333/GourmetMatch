using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderManager : MonoBehaviour
{
    [SerializeField]
    private MergeBoard board;

    // 第一种订单物品
    [SerializeField]
    private ItemData firstOrderItem;

    // 第二种订单物品
    [SerializeField]
    private ItemData secondOrderItem;

    [SerializeField]
    private TMP_Text orderText;

    [SerializeField]
    private TMP_Text coinText;

    [SerializeField]
    private Button submitButton;

    private ItemData currentOrderItem;

    private int coins = 0;

    private void Start()
    {
        currentOrderItem = firstOrderItem;

        UpdateUI();
    }

    public void CompleteOrder()
    {
        if (board == null)
        {
            Debug.LogError("OrderManager 没有设置 Board！");
            return;
        }

        if (currentOrderItem == null)
        {
            Debug.LogError("当前订单没有设置 ItemData！");
            return;
        }

        // 检查棋盘上有没有订单要求的物品
        int itemCount =
            board.CountItems(currentOrderItem);

        if (itemCount < 1)
        {
            Debug.Log(
                "订单无法完成：缺少 " +
                currentOrderItem.itemName
            );

            return;
        }

        // 消耗物品
        bool success =
            board.TryConsumeItem(
                currentOrderItem
            );

        if (!success)
        {
            Debug.LogWarning("食材消耗失败！");
            return;
        }

        // 获得金币
        int reward =
            GetReward(currentOrderItem);

        coins += reward;

        Debug.Log(
            "订单完成！获得 " +
            reward +
            " 金币"
        );

        // 下一笔订单
        GenerateNextOrder();

        UpdateUI();
    }

    private int GetReward(ItemData item)
    {
        if (item == null)
        {
            return 0;
        }

        // 暂时按照等级决定奖励
        return item.level * 10;
    }

    private void GenerateNextOrder()
    {
        if (currentOrderItem == firstOrderItem)
        {
            currentOrderItem =
                secondOrderItem;
        }
        else
        {
            currentOrderItem =
                firstOrderItem;
        }
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text =
                "Coins: " + coins;
        }

        if (currentOrderItem == null)
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

        int reward =
            GetReward(currentOrderItem);

        if (orderText != null)
        {
            orderText.text =
                "Order: 1 x " +
                currentOrderItem.itemName +
                " (Lv." +
                currentOrderItem.level +
                ")" +
                "\nReward: " +
                reward +
                " Coins";
        }

        if (submitButton != null)
        {
            submitButton.interactable =
                true;
        }
    }
}