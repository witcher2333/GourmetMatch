using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class MergeItem : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
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

    private MergeBoard board;
    private FoodProducer energySystem;

    // ===== Producer Runtime State =====
    //还剩多少次
    private int currentCharges;

    //冷却结束时间
    private DateTime cooldownEndUtc;

    //是否已经初始化过
    private bool producerStateInitialized = false;

    // 每秒刷新一次生产器冷却显示
    private float producerUiTimer = 0f;

    private Coroutine
    scaleAnimationCoroutine;

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

        board =
            FindFirstObjectByType<MergeBoard>();

        energySystem =
            FindFirstObjectByType<FoodProducer>();
    }

    private void Update()
    {
        if (Data == null ||
            Data.itemType != ItemType.Producer)
        {
            return;
        }

        producerUiTimer += Time.deltaTime;

        if (producerUiTimer < 1f)
        {
            return;
        }

        producerUiTimer = 0f;

        RefreshProducerCooldown();
        UpdateProducerDisplay();
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

        if (Data.itemType == ItemType.Producer &&
            !producerStateInitialized)
        {
            currentCharges =
                Mathf.Max(0, Data.maxCharges);

            cooldownEndUtc =
                DateTime.MinValue;

            producerStateInitialized = true;
        }

        if (levelLabel != null)
        {
            if (Data.itemType ==
                ItemType.Producer)
            {
                UpdateProducerDisplay();
            }
            else
            {
                levelLabel.text =
                    "Lv." + Data.level;
            }
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
        if (board != null)
        {
            board.ClearSelection();
        }

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

        if (Data != null &&
    Data.itemType == ItemType.Producer)
        {
            SnapTo(originalSlot);
            return;
        }

        if (targetItem.Data != null &&
            targetItem.Data.itemType ==
                ItemType.Producer)
        {
            SnapTo(originalSlot);
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

            ItemData mergedData =
    Data.nextItem;

            // 升级目标物品
            targetItem.SetData(
                mergedData
            );

            targetItem.PlayMergeAnimation();

            if (GameFeedbackManager.Instance !=
                null)
            {
                GameFeedbackManager.Instance
                    .PlayMergeSound();

                GameFeedbackManager.Instance
                    .ShowMessage(
                        "Merged: " +
                        mergedData.itemName
                    );
            }

            // 删除当前物品
            Destroy(gameObject);

            return;
        }

        // 不同物品不能合成
        SnapTo(originalSlot);
    }

    //点击生产器
    public void OnPointerClick(PointerEventData eventData)
    {
        // 没有数据
        if (Data == null)
        {
            return;
        }

        // 普通食材点击后选中
        if (Data.itemType != ItemType.Producer)
        {
            if (board != null)
            {
                board.SelectItem(this);
            }

            return;
        }

        RefreshProducerCooldown();

        // 没配置生产物
        if (Data.producedItem == null)
        {
            Debug.LogError(
                Data.itemName +
                " 没有配置 Produced Item！"
            );

            return;
        }

        // 没找到棋盘
        if (board == null)
        {
            Debug.LogError(
                "Producer 找不到 MergeBoard！"
            );

            return;
        }

        // 没找到体力系统
        if (energySystem == null)
        {
            Debug.LogError(
                "Producer 找不到 FoodProducer！"
            );

            return;
        }

        // 没有剩余生产次数
        if (currentCharges <= 0)
        {
            UpdateProducerDisplay();

            TimeSpan remaining =
                cooldownEndUtc -
                DateTime.UtcNow;

            int seconds =
                Mathf.Max(
                    0,
                    Mathf.CeilToInt(
                        (float)remaining.TotalSeconds
                    )
                );

            Debug.Log(
                Data.itemName +
                " 正在冷却，剩余 " +
                seconds +
                " 秒"
            );

            if (GameFeedbackManager.Instance !=
    null)
            {
                GameFeedbackManager.Instance
                    .ShowMessage(
                        Data.itemName +
                        " cooling: " +
                        seconds +
                        "s",
                        true
                    );

                GameFeedbackManager.Instance
                    .PlayErrorSound();
            }

            return;
        }

        // 先检查棋盘空间
        if (!board.HasEmptySlot())
        {
            Debug.Log(
                "棋盘已满，无法生产！"
            );

            if (GameFeedbackManager.Instance !=null)
            {
                GameFeedbackManager.Instance
                    .ShowMessage(
                        "Board is full!",
                        true
                    );

                GameFeedbackManager.Instance
                    .PlayErrorSound();
            }

            return;
        }

        // 再尝试扣体力
        bool paid =
            energySystem.TrySpendEnergy(
                Data.productionEnergyCost
            );

        if (!paid)
        {
            return;
        }

        // 最后真正生成物品
        bool success =
            board.TrySpawnItem(
                Data.producedItem
            );

        if (success)
        {
            currentCharges--;

            if (GameFeedbackManager.Instance != null)
            {
                GameFeedbackManager.Instance
                    .ShowMessage(
                        "Produced: " +
                        Data.producedItem.itemName
                    );
            }

            Debug.Log(
                Data.itemName +
                " 生产了 " +
                Data.producedItem.itemName +
                "，剩余次数：" +
                currentCharges
            );

            // 次数耗尽，开始冷却
            if (currentCharges <= 0)
            {
                currentCharges = 0;

                cooldownEndUtc =
                    DateTime.UtcNow.AddSeconds(
                        Mathf.Max(0, Data.cooldownSeconds)
                    );

                Debug.Log(
                    Data.itemName +
                    " 次数耗尽，开始冷却！"
                );
            }

            RefreshProducerCooldown();
            UpdateProducerDisplay();
        }
    }

    private void RefreshProducerCooldown()
    {
        if (Data == null ||
            Data.itemType != ItemType.Producer)
        {
            return;
        }

        // 还有次数，不需要冷却
        if (currentCharges > 0)
        {
            return;
        }

        // 没有设置冷却结束时间
        if (cooldownEndUtc == DateTime.MinValue)
        {
            return;
        }

        // 冷却完成
        if (DateTime.UtcNow >= cooldownEndUtc)
        {
            currentCharges =
                Mathf.Max(0, Data.maxCharges);

            cooldownEndUtc =
                DateTime.MinValue;

            Debug.Log(
                Data.itemName +
                " 冷却完成，恢复至 " +
                currentCharges +
                " 次"
            );
        }
    }

    private void UpdateProducerDisplay()
    {
        if (Data == null ||
            Data.itemType != ItemType.Producer ||
            levelLabel == null)
        {
            return;
        }

        int maxCharges =
            Mathf.Max(0, Data.maxCharges);

        if (currentCharges > 0)
        {
            levelLabel.text =
                currentCharges +
                " / " +
                maxCharges;

            return;
        }

        if (cooldownEndUtc == DateTime.MinValue)
        {
            levelLabel.text =
                "0 / " + maxCharges;

            return;
        }

        TimeSpan remaining =
            cooldownEndUtc - DateTime.UtcNow;

        if (remaining.TotalSeconds <= 0)
        {
            levelLabel.text =
                maxCharges +
                " / " +
                maxCharges;

            return;
        }

        int totalSeconds =
            Mathf.CeilToInt(
                (float)remaining.TotalSeconds
            );

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        levelLabel.text =
            "Cooling\n" +
            minutes.ToString("00") +
            ":" +
            seconds.ToString("00");
    }

    // 显示或取消选中状态
    public void SetSelected(bool selected)
    {
        if (itemImage == null ||
            Data == null)
        {
            return;
        }

        if (selected)
        {
            // 选中时使用黄色
            itemImage.color =
                new Color(
                    1f,
                    0.8f,
                    0.25f,
                    1f
                );
        }
        else
        {
            // 取消选中后恢复原本颜色
            if (Data.icon != null)
            {
                itemImage.color =
                    Color.white;
            }
            else
            {
                itemImage.color =
                    Data.backgroundColor;
            }
        }
    }

    public void PlaySpawnAnimation()
    {
        StartScaleAnimation(
            0.65f,
            1.12f,
            0.22f
        );
    }

    public void PlayMergeAnimation()
    {
        StartScaleAnimation(
            1f,
            1.25f,
            0.25f
        );
    }

    private void StartScaleAnimation(
        float startScale,
        float peakScale,
        float duration
    )
    {
        if (rectTransform == null ||
            !isActiveAndEnabled)
        {
            return;
        }

        if (scaleAnimationCoroutine != null)
        {
            StopCoroutine(
                scaleAnimationCoroutine
            );
        }

        scaleAnimationCoroutine =
            StartCoroutine(
                AnimateScale(
                    startScale,
                    peakScale,
                    duration
                )
            );
    }

    private IEnumerator AnimateScale(
        float startScale,
        float peakScale,
        float duration
    )
    {
        float firstDuration =
            Mathf.Max(
                0.01f,
                duration * 0.55f
            );

        float secondDuration =
            Mathf.Max(
                0.01f,
                duration - firstDuration
            );

        float elapsed = 0f;

        while (elapsed < firstDuration)
        {
            elapsed += Time.deltaTime;

            float scale =
                Mathf.Lerp(
                    startScale,
                    peakScale,
                    elapsed /
                    firstDuration
                );

            rectTransform.localScale =
                Vector3.one * scale;

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < secondDuration)
        {
            elapsed += Time.deltaTime;

            float scale =
                Mathf.Lerp(
                    peakScale,
                    1f,
                    elapsed /
                    secondDuration
                );

            rectTransform.localScale =
                Vector3.one * scale;

            yield return null;
        }

        rectTransform.localScale =
            Vector3.one;

        scaleAnimationCoroutine = null;
    }

    public int GetProducerCharges()
    {
        if (Data == null ||
            Data.itemType != ItemType.Producer)
        {
            return 0;
        }

        RefreshProducerCooldown();

        return currentCharges;
    }

    public string GetProducerCooldownEndUtc()
    {
        if (Data == null ||
            Data.itemType != ItemType.Producer)
        {
            return "";
        }

        RefreshProducerCooldown();

        if (cooldownEndUtc == DateTime.MinValue)
        {
            return "";
        }

        return cooldownEndUtc.ToString("O");
    }

    public void LoadProducerState(
        int savedCharges,
        string savedCooldownEndUtc
    )
    {
        if (Data == null ||
            Data.itemType != ItemType.Producer)
        {
            return;
        }

        producerStateInitialized = true;

        int maxCharges =
            Mathf.Max(0, Data.maxCharges);

        // 兼容 Day8 的旧存档：旧格式没有生产器状态。
        if (savedCharges == 0 &&
            string.IsNullOrEmpty(savedCooldownEndUtc))
        {
            currentCharges = maxCharges;
            cooldownEndUtc = DateTime.MinValue;
            UpdateProducerDisplay();
            return;
        }

        currentCharges =
            Mathf.Clamp(
                savedCharges,
                0,
                maxCharges
            );

        cooldownEndUtc = DateTime.MinValue;

        if (currentCharges <= 0)
        {
            DateTime parsedTime;

            bool parsed =
                DateTime.TryParse(
                    savedCooldownEndUtc,
                    null,
                    System.Globalization
                        .DateTimeStyles
                        .RoundtripKind,
                    out parsedTime
                );

            if (parsed)
            {
                cooldownEndUtc =
                    parsedTime.ToUniversalTime();
            }
            else
            {
                // 无法解析的冷却时间不能让生产器永久卡在 0 次。
                currentCharges = maxCharges;
            }
        }

        RefreshProducerCooldown();
        UpdateProducerDisplay();
    }
}
