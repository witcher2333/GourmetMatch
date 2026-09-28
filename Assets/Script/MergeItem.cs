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
    [SerializeField]
    private TMP_Text levelLabel;

    [SerializeField]
    private TMP_Text nameLabel;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;
    private Image itemImage;

    private BoardSlot originalSlot;

    // 当前物品的数据
    public ItemData Data { get; private set; }

    public int Level
    {
        get
        {
            if (Data == null)
                return 0;

            return Data.level;
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        itemImage = GetComponent<Image>();

        rootCanvas =
            GetComponentInParent<Canvas>().rootCanvas;
    }

    // 设置当前物品
    public void SetData(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("ItemData 为空！");
            return;
        }

        Data = data;

        if (levelLabel != null)
        {
            levelLabel.text =
                "Lv." + Data.level;
        }

        if (nameLabel != null)
        {
            nameLabel.text =
                Data.itemName;
        }

        if (Data.icon != null)
        {
            itemImage.sprite = Data.icon;
            itemImage.color = Color.white;
        }
        else
        {
            itemImage.sprite = null;
            itemImage.color =
                Data.backgroundColor;
        }
    }

    public void SnapTo(BoardSlot slot)
    {
        rectTransform.SetParent(
            slot.transform,
            false
        );

        rectTransform.anchorMin =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchorMax =
            new Vector2(0.5f, 0.5f);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.sizeDelta =
            new Vector2(74, 74);

        rectTransform.anchoredPosition =
            Vector2.zero;

        rectTransform.localScale =
            Vector3.one;
    }

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        originalSlot =
            GetComponentInParent<BoardSlot>();

        rectTransform.SetParent(
            rootCanvas.transform,
            true
        );

        canvasGroup.blocksRaycasts = false;

        OnDrag(eventData);
    }

    public void OnDrag(
        PointerEventData eventData
    )
    {
        RectTransform canvasRect =
            rootCanvas.transform as RectTransform;

        Vector2 localPoint;

        bool success =
            RectTransformUtility
                .ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint
                );

        if (success)
        {
            rectTransform.anchoredPosition =
                localPoint;
        }
    }

    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        canvasGroup.blocksRaycasts = true;

        GameObject targetObject =
            eventData.pointerCurrentRaycast.gameObject;

        BoardSlot targetSlot = null;

        if (targetObject != null)
        {
            targetSlot =
                targetObject
                    .GetComponentInParent<BoardSlot>();
        }

        // 没拖到棋盘
        if (targetSlot == null)
        {
            SnapTo(originalSlot);
            return;
        }

        // 回到原格子
        if (targetSlot == originalSlot)
        {
            SnapTo(originalSlot);
            return;
        }

        MergeItem targetItem =
            targetSlot.GetItem();

        // 空格：直接移动
        if (targetItem == null)
        {
            SnapTo(targetSlot);
            return;
        }

        // 只有完全相同的 ItemData 才允许合成
        if (targetItem.Data == Data)
        {
            // 已达到最高等级
            if (Data.nextItem == null)
            {
                Debug.Log(
                    Data.itemName +
                    " 已经是最高等级"
                );

                SnapTo(originalSlot);
                return;
            }

            // 升级目标物品
            targetItem.SetData(
                Data.nextItem
            );

            // 删除当前物品
            Destroy(gameObject);

            return;
        }

        // 不同物品不能合成
        SnapTo(originalSlot);
    }
}