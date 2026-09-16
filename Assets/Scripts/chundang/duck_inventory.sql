-- ============================================
-- 存档表（方案A：单表 JSON 字段）
-- 玩家完整存档：背包/仓库/装备栏 三份 JSON 列表 + 玩家状态
-- 在 MySQL（duck 库）中执行本脚本创建表
-- ============================================
CREATE TABLE IF NOT EXISTS duck_inventory (
  player_id      INT PRIMARY KEY,            -- 玩家ID，对应 duck.id
  backpack_json  TEXT,                       -- 背包物品 JSON（ItemSlotSaveListWrapper）
  warehouse_json TEXT,                       -- 仓库物品 JSON
  equipment_json TEXT,                       -- 装备栏物品 JSON（0=主武器/1=副武器/2=近战）
  health         INT DEFAULT -1,             -- 血量（-1=未保存）
  active_slot    INT DEFAULT 0,              -- 激活武器栏位
  mag_ammo_json  TEXT,                       -- 弹匣弹药 JSON（int[3]）
  scene_name     VARCHAR(64) DEFAULT '',     -- 存档时所在场景名
  updated_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
