/****************************************************
    文件：SpawnGOBM.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026/9/30 15:53:12
	功能：Buff模块——生成游戏对象
*****************************************************/

using UnityEngine;
[CreateAssetMenu(fileName = "_SpawnG0BM",menuName = "BuffSysteam/SpawnGOBM",order = 1)]
public class SpawnGOBM : BaseBuffModule
{
    public GameObject prefab;

    public Vector3 localPosition;

    /// <summary>
    /// 应用生成逻辑
    /// 将预制体实例化并挂载到目标对象下，设置其本地坐标
    /// </summary>
    public override void Apply(BuffInfo buffInfo, DamageInfo damageInfo = null)
    {
        var gameObject = Instantiate(prefab, buffInfo.target.transform);
        gameObject.transform.localPosition = localPosition;
    }
}
