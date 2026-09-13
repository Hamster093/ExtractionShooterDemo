/****************************************************
    文件：InventoryService.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-05 20:00:51
	功能：全局管理服务 切换场景不销毁
*****************************************************/

using UnityEngine;

public class InventoryService : MonoBehaviour
{
    public static InventoryService Instance { get; private set; }
    
    /// <summary>
    /// 玩家背包数据
    /// </summary>
    public BackpackData PlayerBackpack { get; private set; }

    /// <summary>
    /// 玩家仓库数据（列表存储，供拖拽存放与数据库存档）
    /// </summary>
    public WarehouseData Warehouse { get; private set; }

    [Tooltip("玩家背包初始容量（格数），需与场景中背包面板格子数一致（当前为 30）")]
    [SerializeField] private int _initialCapacity = 30;

    [Tooltip("仓库初始容量（格数），需与场景中仓库面板格子数一致（当前为 60）")]
    [SerializeField] private int _warehouseCapacity = 60;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        //数据初始化
        PlayerBackpack = new BackpackData(_initialCapacity);
        Warehouse = new WarehouseData(_warehouseCapacity);
    }

}