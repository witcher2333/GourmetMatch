

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MergeBoard : MonoBehaviour
{
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
        SpawnItem(0, 1);
        SpawnItem(1, 1);

        SpawnItem(4, 2);
        SpawnItem(5, 2);
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
    private void SpawnItem(int slotIndex, int level)
    {
        BoardSlot slot = slots[slotIndex];

        MergeItem item = Instantiate(
            itemPrefab,
            slot.transform
        );

        item.SetLevel(level);

        item.SnapTo(slot);
    }

    // 新增：尝试在随机空格中创建物品
    public bool TrySpawnItem(int level)
    {
        // 如果棋盘还没有初始化，就不能生产
        if (slots.Count == 0)
        {
            return false;
        }

        // 第一步：找出所有空格
        List<BoardSlot> emptySlots = new List<BoardSlot>();

        foreach (BoardSlot slot in slots)
        {
            if (slot.GetItem() == null)
            {
                emptySlots.Add(slot);
            }
        }

        // 第二步：判断棋盘是否已满
        if (emptySlots.Count == 0)
        {
            Debug.Log("棋盘已满，无法生产！");
            return false;
        }

        // 第三步：随机选择一个空格
        int randomIndex = Random.Range(0, emptySlots.Count);

        BoardSlot selectedSlot = emptySlots[randomIndex];

        // 第四步：创建新物品
        MergeItem newItem = Instantiate(
            itemPrefab,
            selectedSlot.transform
        );

        // 设置物品等级
        newItem.SetLevel(level);

        // 让物品位于格子中心
        newItem.SnapTo(selectedSlot);

        Debug.Log("成功生产一个 " + level + " 级物品！");

        // 告诉其他系统，生产成功
        return true;
    }


    // 获取棋盘上指定等级的食材数量
    public int CountItemsByLevel(int level)
    {
        int count = 0;

        foreach (BoardSlot slot in slots)
        {
            MergeItem item = slot.GetItem();

            if (item != null && item.Level == level)
            {
                count++;
            }
        }

        return count;
    }


    // 尝试消耗一个指定等级的食材
    public bool TryConsumeItem(int level)
    {
        foreach (BoardSlot slot in slots)
        {
            MergeItem item = slot.GetItem();

            if (item == null)
            {
                continue;
            }

            if (item.Level == level)
            {
                // 立即禁用物品，避免同一帧内被重复领取
                item.gameObject.SetActive(false);

                // 删除这个物品
                Destroy(item.gameObject);

                Debug.Log("消耗了一个 Lv." + level + " 食材");

                return true;
            }
        }

        // 遍历结束，仍然没有找到目标食材
        return false;
    }


}