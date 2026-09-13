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
    /// 保存玩家背包+仓库物品到数据库（预留扩展点）
    /// 数据来源：GameService.Backpack.ExportToSaveList() / GameService.Warehouse.ExportToSaveList()
    /// </summary>
    /// <param name="playerId">玩家ID（duck.id）</param>
    /// <param name="backpackSlots">背包物品列表（ItemSlotSaveData：slotIndex/itemID/amount）</param>
    /// <param name="warehouseSlots">仓库物品列表</param>
    public void SaveInventory(int playerId, System.Collections.Generic.List<ItemSlotSaveData> backpackSlots, System.Collections.Generic.List<ItemSlotSaveData> warehouseSlots)
    {
        // TODO 2026-09-14：建物品存档表（如 duck_inventory：player_id + 背包/仓库 JSON 或逐行存）
        // 建议：一张表存 player_id、inventory_type(0=背包 1=仓库)、slot_index、item_id、amount
        // 或两列 JSON 文本列（backpack_data / warehouse_data），用 JsonUtility.ToJson(List<ItemSlotSaveData>)
        // 调用时机：玩家退出/切换场景/定时保存时调用
        Debug.Log($"[DBMsg] SaveInventory 尚未实现（预留扩展点）：playerId={playerId}, " +
                  $"backpack={backpackSlots?.Count ?? 0} 条, warehouse={warehouseSlots?.Count ?? 0} 条");
    }

    /// <summary>
    /// 按玩家ID加载背包+仓库物品（预留扩展点）
    /// 返回两个列表；调用方用 GameService.Backpack.LoadFromSaveList(...) / Warehouse.LoadFromSaveList(...) 恢复
    /// </summary>
    public (System.Collections.Generic.List<ItemSlotSaveData> backpack, System.Collections.Generic.List<ItemSlotSaveData> warehouse) LoadInventory(int playerId)
    {
        // TODO 2026-09-14：实现读档 SQL，与 SaveInventory 对应
        Debug.Log($"[DBMsg] LoadInventory 尚未实现（预留扩展点）：playerId={playerId}");
        return (null, null);
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