using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField]
    private OrderManager orderManager;

    [SerializeField]
    private FoodProducer energySystem;

    [SerializeField]
    private MergeBoard board;

    [SerializeField]
    private GameObject shopPanel;

    [SerializeField]
    private ItemData flourData;

    [SerializeField]
    private ItemData milkData;

    [SerializeField]
    private int energyPrice = 10;

    [SerializeField]
    private int itemPrice = 15;

    private void Start()
    {
        CloseShop();
    }

    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(
                true
            );
        }
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(
                false
            );
        }
    }

    public void BuyEnergy()
    {
        if (orderManager == null ||
            energySystem == null)
        {
            Debug.LogError(
                "ShopManager 缺少金币或体力系统引用！"
            );

            return;
        }

        if (energySystem.IsEnergyFull())
        {
            Debug.Log(
                "体力已经满了，不能购买！"
            );

            return;
        }

        bool paid =
            orderManager.TrySpendCoins(
                energyPrice
            );

        if (!paid)
        {
            return;
        }

        bool added =
            energySystem.TryAddEnergy(
                1
            );

        // 极少数情况下增加失败，
        // 把金币退回
        if (!added)
        {
            orderManager.AddCoins(
                energyPrice
            );

            Debug.LogWarning(
                "增加体力失败，金币已退回。"
            );

            return;
        }

        Debug.Log(
            "购买 1 点体力成功！"
        );

        if (GameFeedbackManager.Instance !=
    null)
        {
            GameFeedbackManager.Instance
                .ShowMessage(
                    "Energy +1"
                );

            GameFeedbackManager.Instance
                .PlayCoinSound();
        }
    }

    public void BuyFlour()
    {
        BuyItem(
            flourData
        );
    }

    public void BuyMilk()
    {
        BuyItem(
            milkData
        );
    }

    private void BuyItem(
        ItemData item
    )
    {
        if (orderManager == null ||
            board == null)
        {
            Debug.LogError(
                "ShopManager 缺少金币或棋盘引用！"
            );

            return;
        }

        if (item == null)
        {
            Debug.LogError(
                "商店商品 ItemData 为空！"
            );

            return;
        }

        if (item.itemType !=
            ItemType.Mergeable)
        {
            Debug.LogError(
                "商店不能出售 Producer！"
            );

            return;
        }

        // 必须先检查空格，再扣金币
        if (!board.HasEmptySlot())
        {
            Debug.Log(
                "棋盘已满，无法购买物品！"
            );

            return;
        }

        bool paid =
            orderManager.TrySpendCoins(
                itemPrice
            );

        if (!paid)
        {
            return;
        }

        bool spawned =
            board.TrySpawnItem(
                item
            );

        // 如果生成失败，把金币退回
        if (!spawned)
        {
            orderManager.AddCoins(
                itemPrice
            );

            Debug.LogWarning(
                "物品生成失败，金币已退回。"
            );

            return;
        }

        Debug.Log(
            "购买 " +
            item.itemName +
            " 成功！"
        );

        if (GameFeedbackManager.Instance !=
    null)
        {
            GameFeedbackManager.Instance
                .ShowMessage(
                    "Purchased: " +
                    item.itemName
                );

            GameFeedbackManager.Instance
                .PlayCoinSound();
        }

    }
}