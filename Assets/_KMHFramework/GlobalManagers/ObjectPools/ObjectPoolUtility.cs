using System;
using UnityEngine;

namespace FPS_Framework.Pool
{
    public static class ObjectPoolUtility
    {
        public static GameObject EnablePool(this ProjectileType type, Action<GameObject> beforeEnableAction = null)
        {
            GameObject pooledObj = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledObj;
        }

        public static GameObject EnablePool(this ImpactType type, Action<GameObject> beforeEnableAction = null)
        {
            GameObject pooledObj = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledObj;
        }

        public static GameObject EnablePool(this FxType type, Action<GameObject> beforeEnableAction = null)
        {
            GameObject pooledObj = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledObj;
        }

        public static GameObject EnablePool(this UnitType type, Action<GameObject> beforeEnableAction = null)
        {
            GameObject pooledObj = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledObj;
        }

        public static T EnablePool<T>(this ProjectileType type, Action<T> beforeEnableAction = null) where T : MonoBehaviour
        {
            T pooledComponent = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledComponent;
        }

        public static T EnablePool<T>(this ImpactType type, Action<T> beforeEnableAction = null) where T : MonoBehaviour
        {
            T pooledComponent = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledComponent;
        }

        public static T EnablePool<T>(this FxType type, Action<T> beforeEnableAction = null) where T : MonoBehaviour
        {
            T pooledComponent = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledComponent;
        }

        public static T EnablePool<T>(this UnitType type, Action<T> beforeEnableAction = null) where T : MonoBehaviour
        {
            T pooledComponent = ObjectPoolManager.Instance.GetPoolHandler(type).EnableObj(beforeEnableAction);
            return pooledComponent;
        }

        public static T GetComponent<T>(this ProjectileType type) where T : MonoBehaviour
        {
            T peek = ObjectPoolManager.Instance.GetPoolHandler(type).GetAnyObject<T>();
            return peek.GetComponent<T>();
        }

        public static GameObject PeekObj(this ProjectileType type)
        {
            GameObject peek = ObjectPoolManager.Instance.GetPoolHandler(type).GetAnyObject();
            return peek;
        }

        public static void ReturnPool(this GameObject obj, ProjectileType type)
        {
            ObjectPoolManager.Instance.GetPoolHandler(type).DisableObj(obj);
        }

        public static void ReturnPool(this GameObject obj, ImpactType type)
        {
            ObjectPoolManager.Instance.GetPoolHandler(type).DisableObj(obj);
        }

        public static void ReturnPool(this GameObject obj, FxType type)
        {
            ObjectPoolManager.Instance.GetPoolHandler(type).DisableObj(obj);
        }

        public static void ReturnPool(this GameObject obj, UnitType type)
        {
            ObjectPoolManager.Instance.GetPoolHandler(type).DisableObj(obj);
        }
    }
}