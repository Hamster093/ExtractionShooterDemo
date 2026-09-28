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

    /// <summary>
    /// 玩家快捷栏数据（6 格）：存放的是对背包物品实例的【引用】，不是物品拥有权。
    /// 随本服务 DontDestroyOnLoad 一起跨场景保留，但不进数据库存档（读档后自动失效并解绑）。
    /// </summary>
    public HotbarData Hotbar { get; private set; }

    /// <summary>
    /// 玩家背包容量（格数）：必须与场景中背包面板（PlayerBackpackPanel/Scroll View/Viewport/Content）的格子数一致（当前 30 格）。
    /// </summary>
    public const int BackpackCapacity = 30;

    [Tooltip("仓库初始容量（格数），需与场景中仓库面板格子数一致（当前为 60）")]
    [SerializeField] private int _warehouseCapacity = 60;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        //数据初始化
        PlayerBackpack = new BackpackData(BackpackCapacity);
        Warehouse = new WarehouseData(_warehouseCapacity);
        Hotbar = new HotbarData(HotbarService.SlotCount);
    }

}