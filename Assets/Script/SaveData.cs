using System;
using System.Collections.Generic;

[Serializable]
public class SlotSaveData
{
    public int slotIndex;
    public string itemId;
}

[Serializable]
public class GameSaveData
{
    //棋盘上的物品
    public List<SlotSaveData> boardItems = new List<SlotSaveData>();

    //coin
    public int coins;

    //energy
    public int energy;

    //当前订单在数组中的位置
    public int orderIndex;

    //最后一次计算体力恢复的时间
    public string lastEnergyTimeUtc;
}