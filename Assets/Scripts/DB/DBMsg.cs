/****************************************************
    文件：DBMsg.cs
    作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-11 01:10:18
    功能：MySQL 数据库连接与玩家账号数据操作
*****************************************************/

using MySqlConnector;
using System;
using UnityEngine;

/// <summary>
/// 玩家数据结构，只保存当前用到的字段
/// </summary>
public class PlayerData
{
    public int id;       // 玩家唯一 ID，对应 duck.id
    public string name;  // 玩家昵称，对应 duck.name
}

public class DBMsg
{
    private static DBMsg instance = null;

    /// <summary>
    /// 单例入口
    /// </summary>
    public static DBMsg Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DBMsg();
            }
            return instance;
        }
    }

    // MySQL 连接对象
    private MySqlConnection conn;

    /// <summary>
    /// 初始化数据库连接
    /// </summary>
    public void Init()
    {
        try
        {
            string connStr = "server=localhost;User Id=root;Database=duck;password=123456;Charset=utf8";
            conn = new MySqlConnection(connStr);
            conn.Open();
            Debug.Log("DBMsg Init Done");
        }
        catch (Exception e)
        {
            Debug.LogError("DBMsg Init Error: " + e.Message);
        }
    }

    /// <summary>
    /// 查询玩家数据。
    /// 如果账号不存在，则创建新账号并返回。
    /// 如果账号存在但密码错误，返回 null。
    /// 如果查询异常，返回 null。
    /// </summary>
    public PlayerData QueryPlayerData(string acct, string pass)
    {
        PlayerData playerData = null;

        try
        {
            string sql = "select * from duck where acct = @acct";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@acct", acct);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // 账号已存在，检查密码
                        string dbPass = reader["pass"].ToString();

                        if (dbPass == pass)
                        {
                            // 密码正确，返回玩家数据
                            playerData = new PlayerData
                            {
                                id = Convert.ToInt32(reader["id"]),
                                name = reader["name"].ToString()
                                // TOADD：其他字段在这里读取
                            };
                        }
                        else
                        {
                            // 账号存在，但密码错误
                            Debug.LogWarning("密码错误，acct = " + acct);
                            return null;
                        }
                    }
                }
            }

            // 没有查到账号，创建新账号
            if (playerData == null)
            {
                PlayerData newPlayer = new PlayerData
                {
                    id = -1,
                    // 默认昵称
                    name = "DUCK"
                };

                int newId = InsertNewAcctData(acct, pass, newPlayer);

                if (newId > 0)
                {
                    newPlayer.id = newId;
                    playerData = newPlayer;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("QueryPlayerData Error: " + e.Message);
            return null;
        }

        return playerData;
    }

    /// <summary>
    /// 插入新账号，返回新账号 id；失败返回 -1。
    /// </summary>
    private int InsertNewAcctData(string acct, string pass, PlayerData pd)
    {
        int id = -1;

        try
        {
            string sql = "insert into duck set acct=@acct, pass=@pass, name=@name";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@acct", acct);
                cmd.Parameters.AddWithValue("@pass", pass);
                cmd.Parameters.AddWithValue("@name", pd.name);

                cmd.ExecuteNonQuery();

                // LastInsertedId 是 long，这里转成 int
                id = Convert.ToInt32(cmd.LastInsertedId);
            }
        }
        catch (Exception e)
        {
            // 原写法 Console.WriteLine(..., LogType.Error) 不会正确输出 Unity 错误日志
            Debug.LogError("Insert PlayerData Error: " + e.Message);
        }

        return id;
    }

    /// <summary>
    /// 查询某个昵称是否已经存在
    /// </summary>
    public bool QueryNameData(string name)
    {
        bool exist = false;

        try
        {
            string sql = "select * from duck where name = @name";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@name", name);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        exist = true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Query Name State Error: " + e.Message);
        }

        return exist;
    }

    /// <summary>
    /// 更新玩家数据，目前只更新 name。
    /// </summary>
    public bool UpdatePlayerData(int id, PlayerData playerData)
    {
        try
        {
            string sql = "update duck set name=@name where id=@id";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", playerData.name);

                // TOADD：其他字段继续加 set 和参数
                cmd.ExecuteNonQuery();
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Update PlayerData Error: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 登录：校验账号与密码。
    /// 账号存在且密码正确，返回 PlayerData；账号不存在或密码错误，返回 null。
    /// （与 QueryPlayerData 的区别：登录不会自动创建新账号）
    /// </summary>
    public PlayerData Login(string acct, string pass)
    {
        PlayerData playerData = null;

        try
        {
            string sql = "select * from duck where acct = @acct";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@acct", acct);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string dbPass = reader["pass"].ToString();

                        if (dbPass == pass)
                        {
                            playerData = new PlayerData
                            {
                                id = Convert.ToInt32(reader["id"]),
                                name = reader["name"].ToString()
                                // TOADD：其他字段在这里读取
                            };
                        }
                        else
                        {
                            Debug.LogWarning("密码错误，acct = " + acct);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("账号不存在，acct = " + acct);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Login Error: " + e.Message);
            return null;
        }

        return playerData;
    }

    /// <summary>
    /// 账号是否存在
    /// </summary>
    public bool AcctExists(string acct)
    {
        bool exists = false;

        try
        {
            string sql = "select id from duck where acct = @acct";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@acct", acct);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        exists = true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("AcctExists Error: " + e.Message);
        }

        return exists;
    }

    /// <summary>
    /// 注册：插入新账号，默认昵称=账号。
    /// 返回新账号 id；账号已存在返回 -2；插入失败返回 -1。
    /// </summary>
    public int Register(string acct, string pass)
    {
        if (AcctExists(acct))
        {
            Debug.LogWarning("注册失败：账号已存在，acct = " + acct);
            return -2;
        }

        try
        {
            string sql = "insert into duck set acct=@acct, pass=@pass, name=@name";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@acct", acct);
                cmd.Parameters.AddWithValue("@pass", pass);
                cmd.Parameters.AddWithValue("@name", acct); // 默认昵称先用账号名

                cmd.ExecuteNonQuery();

                // LastInsertedId 是 long，这里转成 int
                return Convert.ToInt32(cmd.LastInsertedId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Register Error: " + e.Message);
            return -1;
        }
    }

    /// <summary>
    /// 保存玩家完整存档到数据库（方案A：单表 JSON 字段，表结构见 chundang/duck_inventory.sql）。
    /// 背包/仓库/装备栏用 JsonUtility 序列化为 JSON 文本列；血量/激活栏位/弹匣/场景名存独立列。
    /// 重复保存按 player_id 覆盖（upsert）。
    /// </summary>
    /// <param name="playerId">玩家ID（duck.id）</param>
    /// <param name="backpackSlots">背包物品列表（ItemSlotSaveData：slotIndex/itemID/amount，只含非空格子）</param>
    /// <param name="warehouseSlots">仓库物品列表</param>
    /// <param name="equipmentSlots">装备栏物品列表（0=主武器/1=副武器/2=近战）</param>
    /// <param name="health">血量（-1=未保存）</param>
    /// <param name="activeSlot">激活武器栏位</param>
    /// <param name="magAmmo">每栏位弹匣弹药（int[3]，null 表示无武器）</param>
    /// <param name="sceneName">存档时所在场景名</param>
    public void SaveInventory(int playerId,
        System.Collections.Generic.List<ItemSlotSaveData> backpackSlots,
        System.Collections.Generic.List<ItemSlotSaveData> warehouseSlots,
        System.Collections.Generic.List<ItemSlotSaveData> equipmentSlots,
        int health, int activeSlot, int[] magAmmo, string sceneName)
    {
        try
        {
            string sql = @"INSERT INTO duck_inventory
                (player_id, backpack_json, warehouse_json, equipment_json, health, active_slot, mag_ammo_json, scene_name)
                VALUES (@player_id, @backpack, @warehouse, @equipment, @health, @active_slot, @mag_ammo, @scene_name)
                ON DUPLICATE KEY UPDATE
                backpack_json = @backpack, warehouse_json = @warehouse, equipment_json = @equipment,
                health = @health, active_slot = @active_slot, mag_ammo_json = @mag_ammo, scene_name = @scene_name";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@player_id", playerId);
                cmd.Parameters.AddWithValue("@backpack", ToJsonList(backpackSlots));
                cmd.Parameters.AddWithValue("@warehouse", ToJsonList(warehouseSlots));
                cmd.Parameters.AddWithValue("@equipment", ToJsonList(equipmentSlots));
                cmd.Parameters.AddWithValue("@health", health);
                cmd.Parameters.AddWithValue("@active_slot", activeSlot);
                cmd.Parameters.AddWithValue("@mag_ammo", magAmmo != null ? JsonUtility.ToJson(magAmmo) : "");
                cmd.Parameters.AddWithValue("@scene_name", sceneName ?? "");

                cmd.ExecuteNonQuery();
            }

            Debug.Log($"[DBMsg] 存档成功：playerId={playerId}, backpack={backpackSlots?.Count ?? 0} 条, " +
                      $"warehouse={warehouseSlots?.Count ?? 0} 条, equipment={equipmentSlots?.Count ?? 0} 条, HP={health}");
        }
        catch (Exception e)
        {
            Debug.LogError("SaveInventory Error: " + e.Message);
        }
    }

    /// <summary>
    /// 按玩家ID加载完整存档（方案A）。无存档返回 null。
    /// 调用方：SaveGameService.LoadGame → 背包/仓库 LoadFromSaveList + PlayerStateData.Import
    /// </summary>
    public InventorySaveData LoadInventory(int playerId)
    {
        InventorySaveData data = null;

        try
        {
            string sql = "select * from duck_inventory where player_id = @player_id";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@player_id", playerId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        data = new InventorySaveData
                        {
                            backpack = FromJsonList(reader["backpack_json"]),
                            warehouse = FromJsonList(reader["warehouse_json"]),
                            equipment = FromJsonList(reader["equipment_json"]),
                            health = reader["health"] is DBNull ? -1 : Convert.ToInt32(reader["health"]),
                            activeSlot = reader["active_slot"] is DBNull ? 0 : Convert.ToInt32(reader["active_slot"]),
                            magAmmo = ParseMagAmmo(reader["mag_ammo"]),
                            sceneName = reader["scene_name"] is DBNull ? "" : reader["scene_name"].ToString()
                        };
                    }
                }
            }

            if (data == null)
                Debug.Log($"[DBMsg] 玩家 {playerId} 无存档记录");
        }
        catch (Exception e)
        {
            Debug.LogError("LoadInventory Error: " + e.Message);
            return null;
        }

        return data;
    }

    // ---- 存档序列化辅助 ----

    /// <summary>List&lt;ItemSlotSaveData&gt; → JSON（JsonUtility 需包装类）</summary>
    private static string ToJsonList(System.Collections.Generic.List<ItemSlotSaveData> list)
    {
        if (list == null || list.Count == 0) return "";
        return JsonUtility.ToJson(new ItemSlotSaveListWrapper { items = list });
    }

    /// <summary>JSON → List&lt;ItemSlotSaveData&gt;（空/非法返回空列表）</summary>
    private static System.Collections.Generic.List<ItemSlotSaveData> FromJsonList(object jsonValue)
    {
        if (jsonValue is DBNull || jsonValue == null) return new System.Collections.Generic.List<ItemSlotSaveData>();
        string json = jsonValue.ToString();
        if (string.IsNullOrEmpty(json)) return new System.Collections.Generic.List<ItemSlotSaveData>();

        var wrapper = JsonUtility.FromJson<ItemSlotSaveListWrapper>(json);
        return wrapper != null && wrapper.items != null ? wrapper.items : new System.Collections.Generic.List<ItemSlotSaveData>();
    }

    /// <summary>弹匣 JSON → int[3]（空/非法返回 null）</summary>
    private static int[] ParseMagAmmo(object jsonValue)
    {
        if (jsonValue is DBNull || jsonValue == null) return null;
        string json = jsonValue.ToString();
        if (string.IsNullOrEmpty(json)) return null;

        try
        {
            return JsonUtility.FromJson<int[]>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"ParseMagAmmo Error: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 关闭数据库连接，可在游戏退出或对象销毁时调用
    /// </summary>
    public void Close()
    {
        if (conn != null)
        {
            conn.Close();
            conn.Dispose();
            conn = null;
        }
    }
}