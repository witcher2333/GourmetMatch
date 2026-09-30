using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderManager : MonoBehaviour
{
    [SerializeField]
    private MergeBoard board;

    // 所有订单按照这里的顺序循环
    [SerializeField]
    private ItemData[] orderItems;

    [SerializeField]
    private TMP_Text orderText;

    [SerializeField]
    private TMP_Text coinText;

    [SerializeField]
    private Button submitButton;

    // 当前订单在数组中的位置
    private int currentOrderIndex = 0;

    // 当前金币
    private int coins = 0;

    // 当前订单物品
    private ItemData CurrentOrderItem
    {
        get
        {
            if (orderItems == null ||
                orderItems.Length == 0)
            {
                return null;
            }

            return orderItems[currentOrderIndex];
        }
    }

    private void Start()
    {
        currentOrderIndex = 0;

        UpdateUI();
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

        ItemData currentItem =
            CurrentOrderItem;

        if (currentItem == null)
        {
            Debug.LogError(
                "当前订单 ItemData 为空！"
            );

            return;
        }

        // 检查棋盘有没有订单需要的物品
        int itemCount =
            board.CountItems(currentItem);

        if (itemCount < 1)
        {
            Debug.Log(
                "缺少订单物品：" +
                currentItem.itemName
            );

            return;
        }

        // 消耗一个订单物品
        bool success =
            board.TryConsumeItem(currentItem);

        if (!success)
        {
            Debug.LogWarning(
                "订单物品消耗失败！"
            );

            return;
        }

        // 获得奖励
        int reward =
            GetReward(currentItem);

        coins += reward;

        Debug.Log(
            "订单完成：" +
            currentItem.itemName +
            "，获得 " +
            reward +
            " Coins"
        );

        // 切换到下一订单
        GenerateNextOrder();

        // 更新 UI
        UpdateUI();
    }

    private int GetReward(ItemData item)
    {
        if (item == null)
        {
            return 0;
        }

        // 暂时按照等级计算奖励
        return item.level * 10;
    }

    private void GenerateNextOrder()
    {
        currentOrderIndex++;

        // 超过数组最后一个以后，从头开始
        if (currentOrderIndex >= orderItems.Length)
        {
            currentOrderIndex = 0;
        }
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text =
                "Coins: " + coins;
        }

        ItemData currentItem =
            CurrentOrderItem;

        if (currentItem == null)
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
            GetReward(currentItem);

        if (orderText != null)
        {
            orderText.text =
                "Order: 1 x " +
                currentItem.itemName +
                " (Lv." +
                currentItem.level +
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

    //让 OrderManager 暴露金币和订单状态
    public int GetCoins()
    {
        return coins;
    }
    public int GetCurrentOrderIndex()
    {
        return currentOrderIndex;
    }
    public void LoadState(
    int savedCoins,
    int savedOrderIndex
)
    {
        coins =
            Mathf.Max(0, savedCoins);

        if (orderItems == null ||
            orderItems.Length == 0)
        {
            currentOrderIndex = 0;
        }
        else
        {
            currentOrderIndex =
                Mathf.Clamp(
                    savedOrderIndex,
                    0,
                    orderItems.Length - 1
                );
        }

        UpdateUI();
    }

}