/****************************************************
    文件：BuffData.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/30 15:04:37
	功能：Buff数据，定义Buff的静态配置信息，作为ScriptableObject资产使用
*****************************************************/

using UnityEngine;

[CreateAssetMenu(fileName = "_BuffData", menuName = "BuffSystem/BuffData", order = 1)]
public class BuffData : ScriptableObject
{
    //基本信息
    public int id;                // Buff唯一ID    
    public string buffName;       // Buff名称
    public string description;    // Buff描述文本
    public Sprite icon;           // Buff图标
    public int priority;          // 优先级，用于处理多个Buff之间的覆盖或执行顺序
    public int maxStack;          // 优先级，用于处理多个Buff之间的覆盖或执行顺序
    public string[] tags;         // Buff标签，用于分类或条件判断
    //时间信息
    public bool isForever;        // 是否为永久Buff为，true时忽略duration
    public float duration;        // 持续时间（秒）
    public float tickTime;        // 触发间隔时间（秒），用于周期性触发OnTick
    //更新方式
    public BuffUpdateTimeEnum buffUpdateTime; // 当已存在相同Buff时，持续时间的更新方式（累加、替换、保持）
    public BuffRemoveStackUpdateEnum buffRemoveStackUpdate; // 移除一层Buff时，叠加层数的更新方式（清空、减少）
    //基础回调点
    public BaseBuffModule OnCreate; // Buff创建时触发的回调模块
    public BaseBuffModule OnRemove; // Buff移除时触发的回调模块
    public BaseBuffModule OnTick;   // Buff每经过一个tickTime间隔时触发的回调模块
    //伤害回调点
    public BaseBuffModule OnHit;    // 持有者造成伤害时触发的回调模块
    public BaseBuffModule OnBehurt; // 持有者受到伤害时触发的回调模块
    public BaseBuffModule OnKill;   // 持有者击杀目标时触发的回调模块
    public BaseBuffModule OnBekill; // 持有者被击杀时触发的回调模块
}