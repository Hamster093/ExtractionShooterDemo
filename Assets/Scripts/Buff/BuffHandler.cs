/****************************************************
    文件：BuffHandler.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/30 15:12:53
	功能：Buff处理器，负责Buff的添加、移除、计时更新及按优先级排序等逻辑  挂载在游戏对象上，管理该对象身上所有Buff的运行时状态
*****************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffHandler : MonoBehaviour
{
    /// 当前对象持有的Buff链表
    /// 使用LinkedList便于频繁的插入、删除操作
    public LinkedList<BuffInfo> buffList = new LinkedList<BuffInfo>();



    private void Update()
    {
        // 对buffTick处理
        BuffTickAndRemove();
    }

    /// <summary>
    /// 处理Buff的周期触发（OnTick）以及持续时间到期后的移除
    /// </summary>
    private void BuffTickAndRemove()
    {
        List<BuffInfo> deleteBuffListList = new List<BuffInfo>();
        foreach (var buffInfo in buffList)
        {
            if (buffInfo.buffData.OnTick != null)
            {
                if (buffInfo.tickTimer < 0)
                {
                    buffInfo.buffData.OnTick.Apply(buffInfo);
                    buffInfo.tickTimer = buffInfo.buffData.tickTime;
                }
                else
                {
                    buffInfo.tickTimer -= Time.deltaTime;
                }
            }
            if (buffInfo.durationTimer < 0)
            {
                deleteBuffListList.Add(buffInfo);
            }
            else
            {
                buffInfo.durationTimer -= Time.deltaTime;
            }
        }
        foreach (var buffInfo in deleteBuffListList)
        {
            RemoveBuff(buffInfo);
        }
    }

    /// <summary>
    /// 添加一个Buff
    /// 如果已存在相同ID的Buff，则根据配置进行叠加或刷新；否则新增并排序
    /// </summary>
    public void AddBuff(BuffInfo buffInfo)
    {
        BuffInfo findBuffInfo = FindBuff(buffInfo.buffData.id);
        if (findBuffInfo != null)
        {
            //buff存在
            if (findBuffInfo.curStack < findBuffInfo.buffData.maxStack)
            {
                findBuffInfo.curStack++;
                switch (findBuffInfo.buffData.buffUpdateTime)
                {
                    case BuffUpdateTimeEnum.Add:
                        findBuffInfo.durationTimer += findBuffInfo.buffData.duration;
                        break;
                    case BuffUpdateTimeEnum.Replace:
                        findBuffInfo.durationTimer = findBuffInfo.buffData.duration;
                        break;
                }
                findBuffInfo.buffData.OnCreate.Apply(findBuffInfo);
            }
        }
        else
        {
            buffInfo.durationTimer = buffInfo.buffData.duration;
            buffInfo.buffData.OnCreate.Apply(buffInfo);
            buffList.AddLast(buffInfo);
            //对这个buffList进行排序
            InsertionSortLinkedList(buffList);


        }
    }

    /// <summary>
    /// 移除一个Buff（或减少一层）
    /// 根据配置的移除层数更新方式，决定是直接清除还是层数减一
    /// </summary>
    public void RemoveBuff(BuffInfo buffInfo)
    {
        switch (buffInfo.buffData.buffRemoveStackUpdate)
        {
            case BuffRemoveStackUpdateEnum.Clear:
                buffInfo.buffData.OnRemove.Apply(buffInfo);
                buffList.Remove(buffInfo);
                break;
            case BuffRemoveStackUpdateEnum.Reduce:
                buffInfo.curStack--;
                buffInfo.buffData.OnRemove.Apply(buffInfo);
                if (buffInfo.curStack == 0)
                {                   
                    buffList.Remove(buffInfo);
                }
                else
                {
                    buffInfo.durationTimer = buffInfo.buffData.duration;
                }
                break;
        }
    }

    /// <summary>
    /// 根据BuffData的ID查找已存在的Buff
    /// </summary>
    private BuffInfo FindBuff(int buffDataID)
    {
        foreach (var buffInfo in buffList)
        {
            if (buffInfo.buffData.id == buffDataID)
            {
                return buffInfo;
            }
        }

        return default;
    }

    /// <summary>
    /// 使用插入排序对LinkedList进行排序
    /// 排序规则：按BuffData的priority降序排列（优先级高的在前）
    /// </summary>
    void InsertionSortLinkedList(LinkedList<BuffInfo> list)
    {
        if (list == null || list.First == null)
        {
            return; // 链表为空或只有一个元素时无需排序
        }

        LinkedListNode<BuffInfo> current = list.First.Next;

        while (current != null)
        {
            LinkedListNode<BuffInfo> next = current.Next;
            LinkedListNode<BuffInfo> prev = current.Previous;

            // 向前查找插入位置：找到第一个 priority <= current.priority 的节点
            while (prev != null && prev.Value.buffData.priority > current.Value.buffData.priority)
            {
                prev = prev.Previous;
            }

            if (prev == null)
            {
                // current 应该成为新的头节点
                list.Remove(current);
                list.AddFirst(current);
            }
            else
            {
                // 将 current 插入到 prev 之后
                list.Remove(current);
                list.AddAfter(prev, current);
            }

            current = next;
        }
    }

}