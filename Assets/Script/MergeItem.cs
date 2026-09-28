
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class MergeItem : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    // 食材配置总表
    [SerializeField]
    private ItemCatalog catalog;

    // 显示等级
    [SerializeField]
    private TMP_Text levelLabel;

    // 显示食材名称
    [SerializeField]
    private TMP_Text nameLabel;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Image itemImage;

    private BoardSlot originalSlot;

    // 当前食材等级
    public int Level { get; private set; } = 1;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        itemImage = GetComponent<Image>();

        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    // 根据等级读取食材数据
    public void SetLevel(int newLevel)
    {
        if (catalog == null)
        {
            Debug.LogError("Item 没有设置 Catalog！");
            return;
        }

        ItemData data = catalog.GetByLevel(newLevel);

        if (data == null)
        {
            Debug.LogWarning(
                "找不到等级 " + newLevel + " 对应的食材！"
            );

            return;
        }

        // 更新当前等级
        Level = data.level;

        // 更新等级文字
        if (levelLabel != null)
        {
            levelLabel.text = Level.ToString();
        }

        // 更新食材名称
        if (nameLabel != null)
        {
            nameLabel.text = data.itemName;
        }

        // 有图片就显示图片，没有则使用背景颜色
        if (data.icon != null)
        {
            itemImage.sprite = data.icon;
            itemImage.color = Color.white;
        }
        else
        {
            itemImage.sprite = null;
            itemImage.color = data.backgroundColor;
        }
    }

    // 将物品放到指定格子中心
    public void SnapTo(BoardSlot slot)
    {
        rectTransform.SetParent(slot.transform, false);

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        rectTransform.sizeDelta = new Vector2(74, 74);

        rectTransform.anchoredPosition = Vector2.zero;

        rectTransform.localScale = Vector3.one;
    }

    // 开始拖动
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalSlot = GetComponentInParent<BoardSlot>();

        rectTransform.SetParent(rootCanvas.transform, true);

        canvasGroup.blocksRaycasts = false;

        OnDrag(eventData);
    }

    // 拖动中
    public void OnDrag(PointerEventData eventData)
    {
        RectTransform canvasRect =
            rootCanvas.transform as RectTransform;

        Vector2 localPoint;

        bool success =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

        if (success)
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    // 结束拖动
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        GameObject targetObject =
            eventData.pointerCurrentRaycast.gameObject;

        BoardSlot targetSlot = null;

        if (targetObject != null)
        {
            targetSlot =
                targetObject.GetComponentInParent<BoardSlot>();
        }

        // 未拖到有效格子
        if (targetSlot == null)
        {
            SnapTo(originalSlot);
            return;
        }

        // 拖回原格子
        if (targetSlot == originalSlot)
        {
            SnapTo(originalSlot);
            return;
        }

        MergeItem targetItem = targetSlot.GetItem();

        // 空格子：直接移动
        if (targetItem == null)
        {
            SnapTo(targetSlot);
            return;
        }

        // 等级相同，尝试合成
        if (targetItem.Level == Level)
        {
            // 检查是否还有下一级
            ItemData nextData = catalog.GetByLevel(Level + 1);

            // 如果已经达到最高等级，则不能合成
            if (nextData == null)
            {
                Debug.Log("已经达到当前最高等级！");

                SnapTo(originalSlot);
                return;
            }

            // 将目标物品升级
            targetItem.SetLevel(nextData.level);

            // 删除拖过来的物品
            Destroy(gameObject);

            return;
        }

        // 等级不同：返回原位
        SnapTo(originalSlot);
    }
}