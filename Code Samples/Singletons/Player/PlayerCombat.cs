using Items;
using Input;
using UnityEngine;
using UnityEngine.Events;

namespace Singletons.Player
{
    public class PlayerCombat : Singleton<PlayerCombat>
    {
        [SerializeField] private Transform _weaponHoldPoint;

        private Weapon _attachedWeapon;

        public bool HasWeaponAttached => _attachedWeapon != null;

        public UnityEvent<Weapon> OnWeaponAttach { get; set; } = new();

        private void OnEnable()
        {
            OnWeaponAttach.AddListener(AttachWeapon);
        }

        private void OnDisable()
        {
            OnWeaponAttach.RemoveAllListeners();
        }

        private void Update()
        {
            if (_attachedWeapon != null)
            {
                _attachedWeapon.transform.SetPositionAndRotation(_weaponHoldPoint.position, _weaponHoldPoint.rotation);
            }

            if(UserInput.Instance.ThrowWeaponPressed && _attachedWeapon != null)
            {
                ThrowWeapon();
            }
        }

        public void AttachWeapon(Weapon weapon)
        {
            if (_attachedWeapon != null)
            {
                ThrowWeapon();
            }

            weapon.GetComponent<Rigidbody>().isKinematic = true;
            weapon.transform.SetParent(transform);
            weapon.GetComponent<ItemInstance>().enabled = false;
            _attachedWeapon = weapon;
        }

        public void ThrowWeapon()
        {
            _attachedWeapon.transform.SetParent(null);
            var rb = _attachedWeapon.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
            _attachedWeapon.GetComponent<ItemInstance>().enabled = true;
            _attachedWeapon = null;
        }
    }
}