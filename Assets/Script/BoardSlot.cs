
using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    // 获取当前格子中的物品
    public MergeItem GetItem()
    {
        return GetComponentInChildren<MergeItem>();
    }
}