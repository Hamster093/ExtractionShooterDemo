/****************************************************
    文件：FogController.cs
	作者：DADI
    邮箱: 1581507659@qq.com
    日期：2026-09-20 16:41:06
	功能：将玩家的位置、朝向以及视野参数实时传递给 Shader
*****************************************************/

using UnityEngine;

public class FogController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Renderer fogRenderer;
    [SerializeField] private VisionCone visionCone;

    private MaterialPropertyBlock _mpb;
    private Renderer _renderer;
    private float _lastViewDistance = -1f;
    private float _lastFov = -1f;

    private static readonly int ID_PlayerPos = Shader.PropertyToID("_PlayerPos");
    private static readonly int ID_PlayerForward = Shader.PropertyToID("_PlayerForward");
    private static readonly int ID_ViewDistance = Shader.PropertyToID("_ViewDistance");
    private static readonly int ID_Fov = Shader.PropertyToID("_Fov");

    private void Start()
    {
        _renderer = fogRenderer != null ? fogRenderer : GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_mpb); // 保留材质原有值作为默认
        PushStaticParams();
        _renderer.SetPropertyBlock(_mpb);
    }

    private void Update()
    {
        if (player == null || _renderer == null) return;

        Vector3 aimDir = player.forward; // 或你自己的 aimDir 计算
        _mpb.SetVector(ID_PlayerPos, player.position);
        _mpb.SetVector(ID_PlayerForward, aimDir.normalized);
        PushStaticParams();
        _renderer.SetPropertyBlock(_mpb);
    }

    private void PushStaticParams()
    {
        if (visionCone == null) return;
        if (!Mathf.Approximately(_lastViewDistance, visionCone.viewDistance))
        {
            _lastViewDistance = visionCone.viewDistance;
            _mpb.SetFloat(ID_ViewDistance, _lastViewDistance);
        }
        if (!Mathf.Approximately(_lastFov, visionCone.fov))
        {
            _lastFov = visionCone.fov;
            _mpb.SetFloat(ID_Fov, _lastFov);
        }
    }
}