using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPickupTest : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private WeaponBase _weaponPrefab;
    [SerializeField] private Transform _handSocket;      // ★ 拖入手部武器父节点
    [SerializeField] private int _targetSlot = 0;
    [SerializeField] private KeyCode _pickupKey = KeyCode.Q;

    private void Update()
    {
        // ★ 使用 GetKeyDown 防止按住时连续生成武器
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log("按下Q捡起武器"+ _targetSlot);
            OnTestPickup();
        }
    }

    private void OnTestPickup()
    {
        if (_playerController == null || _weaponPrefab == null || _handSocket == null)
        {
            Debug.LogError("PlayerController / WeaponPrefab / HandSocket 未赋值！");
            return;
        }

        var weaponInstance = Instantiate(_weaponPrefab, _handSocket);

        _playerController.PickupWeapon(weaponInstance, _targetSlot);
        Debug.Log($"[按键测试] 按下 {_pickupKey} → 拾取 {_weaponPrefab.name} 到栏位 {_targetSlot}");
    }
}