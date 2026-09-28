

using System.Collections.Generic;
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
    public bool TryConsumeItem(
    ItemData targetData
)
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


}