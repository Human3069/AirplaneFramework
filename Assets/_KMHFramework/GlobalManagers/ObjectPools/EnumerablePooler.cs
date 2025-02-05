using System.Collections.Generic;
using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace FPS_Framework.Pool.Internal
{
    public abstract class EnumerablePooler
    {
        public abstract void OnAwake(Transform targetTransform);

        public abstract Type GetEnumType();
    }

    [Serializable]
    public class EnumerablePooler<TEnum> : EnumerablePooler where TEnum : struct, IConvertible
    {
        [SerializeField]
        [SerializedDictionary("Enum", "Class")]
        protected SerializedDictionary<TEnum, PoolHandler> PoolHandlerDic = new SerializedDictionary<TEnum, PoolHandler>();

        public override void OnAwake(Transform targetTransform)
        {
            foreach (KeyValuePair<TEnum, PoolHandler> pair in PoolHandlerDic)
            {
                GameObject poolHandlerParent = new GameObject("PoolHandler_" + pair.Value.Prefab.name);
                poolHandlerParent.transform.SetParent(targetTransform);

                pair.Value.Initialize(poolHandlerParent.transform);
            }
        }

        public PoolHandler GetPoolHandler(TEnum enumValue)
        {
            return PoolHandlerDic[enumValue];
        }

        public override Type GetEnumType()
        {
            return typeof(TEnum);
        }
    }
}