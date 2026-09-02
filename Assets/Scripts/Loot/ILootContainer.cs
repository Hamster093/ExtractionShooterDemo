/****************************************************
    文件：ILootContainer.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-03 16:13:37
	功能：可打开的战利品容器接口
*****************************************************/

using System.Collections.Generic;
/// <summary>
/// 宝箱、尸体、掉落物都实现此接口
/// </summary>
public interface ILootContainer
{
    //容器名称
    string ContainerName { get; }

    //获取战利品物品列表
    IReadOnlyList<ItemData> GetLootItems();

    //取走物品后的回调（如尸体消失等）
    void OnLootTaken(ItemData item);
}