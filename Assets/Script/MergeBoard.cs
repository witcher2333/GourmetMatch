

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MergeBoard : MonoBehaviour
{

    [SerializeField]
    private ItemData startingLevel1Item;

    [SerializeField]
    private ItemData startingLevel2Item;

    [SerializeField]
    private RectTransform boardArea;

    [SerializeField]
    private MergeItem itemPrefab;

    [SerializeField]
    private ItemData flourProducerData;

    [SerializeField]
    private ItemData milkProducerData;

    [SerializeField]
    private ItemData coffeeProducerData;

    [SerializeField]
    private OrderManager orderManager;

    [SerializeField]
    private Button sellButton;

    // 当前选中的普通物品
    private MergeItem selectedItem;

    // 保存所有棋盘格子
    private List<BoardSlot> slots = new List<BoardSlot>();

    private void Start()
    {
        if (boardArea == null || itemPrefab == null)
        {
            Debug.LogError("请先设置 Board Area 和 Item Prefab！");
            return;
        }

        // 创建 16 个格子
        for (int i = 0; i < 16; i++)
        {
            CreateSlot(i);
        }

        // 创建最初的四个物品
        SpawnItem(0, startingLevel1Item);
        SpawnItem(1, startingLevel1Item);

        SpawnItem(4, startingLevel2Item);
        SpawnItem(5, startingLevel2Item);
        SpawnItem(12,flourProducerData);
        SpawnItem(15,milkProducerData );
        SpawnItem(14,coffeeProducerData);

        UpdateSellButton();
    }

    private void CreateSlot(int index)
    {
        GameObject slotObject = new GameObject(
            "Slot_" + index,
            typeof(RectTransform),
            typeof(Image),
            typeof(BoardSlot)
        );

        slotObject.transform.SetParent(boardArea, false);

        Image image = slotObject.GetComponent<Image>();

        image.color = new Color(0.96f, 0.88f, 0.72f);
        image.raycastTarget = true;

        BoardSlot slot = slotObject.GetComponent<BoardSlot>();

        slots.Add(slot);
    }

    // 在指定位置创建物品
    private void SpawnItem(
    int slotIndex,
    ItemData data
)
    {
        if (data == null)
        {
            Debug.LogError(
                "SpawnItem 收到了空的 ItemData！"
            );

            return;
        }

        if (slotIndex < 0 ||
            slotIndex >= slots.Count)
        {
            Debug.LogError(
                "SpawnItem 收到了非法 Slot Index：" +
                slotIndex
            );

            return;
        }

        BoardSlot slot =
            slots[slotIndex];

        MergeItem item =
            Instantiate(
                itemPrefab,
                slot.transform
            );

        item.SetData(data);

        item.SnapTo(slot);
    }

    // 新增：尝试在随机空格中创建物品
    public bool TrySpawnItem(ItemData data)
    {
        if (slots.Count == 0)
        {
            return false;
        }

        List<BoardSlot> emptySlots =
            new List<BoardSlot>();

        foreach (BoardSlot slot in slots)
        {
            if (slot.GetItem() == null)
            {
                emptySlots.Add(slot);
            }
        }

        if (emptySlots.Count == 0)
        {
            Debug.Log(
                "棋盘已满，无法生产！"
            );

            return false;
        }

        int randomIndex =
            Random.Range(
                0,
                emptySlots.Count
            );

        BoardSlot selectedSlot =
            emptySlots[randomIndex];

        MergeItem newItem =
            Instantiate(
                itemPrefab,
                selectedSlot.transform
            );

        newItem.SetData(data);

        newItem.SnapTo(selectedSlot);

        Debug.Log(
            "生产：" + data.itemName
        );

        return true;
    }


    // 获取棋盘上指定等级的食材数量
    public int CountItems(ItemData targetData)
    {
        int count = 0;

        foreach (BoardSlot slot in slots)
        {
            MergeItem item =
                slot.GetItem();

            if (
                item != null &&
                item.Data == targetData
            )
            {
                count++;
            }
        }

        return count;
    }


    // 尝试消耗一个指定等级的食材
    public bool TryConsumeItem(ItemData targetData)
    {
        foreach (BoardSlot slot in slots)
        {
            MergeItem item =
                slot.GetItem();

            if (item == null)
            {
                continue;
            }

            if (item.Data == targetData)
            {
                if (item == selectedItem)
                {
                    ClearSelection();
                }

                item.gameObject.SetActive(
                    false
                );

                Destroy(item.gameObject);

                Debug.Log(
                    "消耗：" +
                    targetData.itemName
                );

                return true;
            }
        }

        return false;
    }

    //Json save data
    public List<SlotSaveData> GetBoardSaveData()
    {
        List<SlotSaveData> saveItems =
            new List<SlotSaveData>();

        for (int i = 0; i < slots.Count; i++)
        {
            MergeItem item =
                slots[i].GetItem();

            if (item == null ||
                item.Data == null)
            {
                continue;
            }

            SlotSaveData saveData =
                new SlotSaveData();

            saveData.slotIndex = i;
            saveData.itemId =
                item.Data.itemId;

            if (item.Data.itemType ==
                ItemType.Producer)
            {
                saveData.producerCharges =
                    item.GetProducerCharges();

                saveData.producerCooldownEndUtc =
                    item.GetProducerCooldownEndUtc();
            }

            saveItems.Add(saveData);
        }

        return saveItems;
    }

    //clean the table
    public void ClearBoard()
    {
        ClearSelection();

        foreach (BoardSlot slot in slots)
        {
            MergeItem item =
                slot.GetItem();

            if (item != null)
            {
                item.gameObject.SetActive(false);
                Destroy(item.gameObject);
            }
        }
    }

    //recovery the table from the json
    public void LoadBoard(
    List<SlotSaveData> savedItems,
    ItemCatalog catalog
)
    {
        if (catalog == null)
        {
            Debug.LogError(
                "LoadBoard 缺少 ItemCatalog！"
            );

            return;
        }

        ClearBoard();

        foreach (SlotSaveData savedItem
                 in savedItems)
        {
            if (savedItem.slotIndex < 0 ||
                savedItem.slotIndex >= slots.Count)
            {
                Debug.LogWarning(
                    "非法 Slot Index: " +
                    savedItem.slotIndex
                );

                continue;
            }

            ItemData data =
                catalog.GetById(
                    savedItem.itemId
                );

            if (data == null)
            {
                continue;
            }

            BoardSlot slot =
                slots[savedItem.slotIndex];

            MergeItem item =
                Instantiate(
                    itemPrefab,
                    slot.transform
                );

            item.SetData(data);

            if (data.itemType ==
                ItemType.Producer)
            {
                item.LoadProducerState(
                    savedItem.producerCharges,
                    savedItem.producerCooldownEndUtc
                );
            }

            item.SnapTo(slot);
        }
    }

    //判断还有没有空格
    public bool HasEmptySlot()
    {
        foreach (BoardSlot slot in slots)
        {
            if (slot.GetItem() == null)
            {
                return true;
            }
        }

        return false;
    }

    // 选择一个普通物品
    public void SelectItem(MergeItem item)
    {
        if (item == null ||
            item.Data == null)
        {
            ClearSelection();
            return;
        }

        // Producer 不允许进入出售选择
        if (item.Data.itemType ==
            ItemType.Producer)
        {
            ClearSelection();
            return;
        }

        // 再次点击同一物品时取消选择
        if (selectedItem == item)
        {
            ClearSelection();
            return;
        }

        // 先取消之前的选择
        if (selectedItem != null)
        {
            selectedItem.SetSelected(
                false
            );
        }

        selectedItem = item;

        selectedItem.SetSelected(
            true
        );

        UpdateSellButton();

        Debug.Log(
            "选中：" +
            selectedItem.Data.itemName
        );
    }

    // 取消当前选择
    public void ClearSelection()
    {
        if (selectedItem != null)
        {
            selectedItem.SetSelected(
                false
            );
        }

        selectedItem = null;

        UpdateSellButton();
    }

    // 出售当前选择的物品
    public void SellSelectedItem()
    {
        if (selectedItem == null ||
            selectedItem.Data == null)
        {
            Debug.Log(
                "当前没有选择可以出售的物品。"
            );

            UpdateSellButton();
            return;
        }

        if (selectedItem.Data.itemType ==
            ItemType.Producer)
        {
            Debug.Log(
                "Producer 不能出售！"
            );

            ClearSelection();
            return;
        }

        if (orderManager == null)
        {
            Debug.LogError(
                "MergeBoard 没有设置 OrderManager！"
            );

            return;
        }

        MergeItem itemToSell =
            selectedItem;

        string itemName =
            itemToSell.Data.itemName;

        int sellPrice =
            GetSellPrice(
                itemToSell.Data
            );

        ClearSelection();

        itemToSell.gameObject.SetActive(
            false
        );

        Destroy(
            itemToSell.gameObject
        );

        orderManager.AddCoins(
            sellPrice
        );

        Debug.Log(
            "出售 " +
            itemName +
            "，获得 " +
            sellPrice +
            " Coins"
        );
    }

    // 根据物品等级计算售价
    private int GetSellPrice(
        ItemData item
    )
    {
        if (item == null)
        {
            return 0;
        }

        int level =
            Mathf.Max(
                1,
                item.level
            );

        switch (level)
        {
            case 1:
                return 2;

            case 2:
                return 5;

            case 3:
                return 12;

            case 4:
                return 25;

            default:
                return 25 +
                    (level - 4) * 15;
        }
    }

    // 更新出售按钮状态
    private void UpdateSellButton()
    {
        if (sellButton == null)
        {
            return;
        }

        sellButton.interactable =
            selectedItem != null &&
            selectedItem.Data != null &&
            selectedItem.Data.itemType ==
                ItemType.Mergeable;
    }

}
