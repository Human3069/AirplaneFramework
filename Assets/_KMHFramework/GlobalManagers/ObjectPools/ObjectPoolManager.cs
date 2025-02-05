using FPS_Framework.Pool.Internal;
using System.Reflection;
using UnityEngine;

namespace FPS_Framework.Pool
{
    public enum ProjectileType
    {
        _50cal_Friendly,
        _50kg_Bomb_Friendly,

        _50cal_Enemy,
    }

    public enum ImpactType
    {
        _40mm_Explosion_Air,
        _105mm_Explosion,

        _50cal_Impact_Ground,
        _50cal_Impact_Water,
    }

    public enum FxType
    {
        //
    }

    public enum UnitType
    {
        //
    }

    public class ObjectPoolManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[ObjectPoolManager]</b></color> {0}";

        protected static ObjectPoolManager _instance;
        public static ObjectPoolManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [SerializeField]
        protected EnumerablePooler<ProjectileType> projectilePooler;
        [Space(10)]
        [SerializeField]
        protected EnumerablePooler<ImpactType> impactPooler;
        [Space(10)]
        [SerializeField]
        protected EnumerablePooler<FxType> fxPooler;
        [Space(10)]
        [SerializeField]
        protected EnumerablePooler<UnitType> unitPooler;
        
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("");
                Destroy(this.gameObject);
                return;
            }

            FieldInfo[] fieldInfos = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo info in fieldInfos)
            {
                if (info.FieldType.Name.Contains(nameof(EnumerablePooler)) == true)
                {
                    EnumerablePooler enumerablePooler = info.GetValue(this) as EnumerablePooler;

                    GameObject enumerablePoolerObj = new GameObject("EnumerablePooler_" + enumerablePooler.GetEnumType().Name);
                    enumerablePoolerObj.transform.SetParent(this.transform);

                    enumerablePooler.OnAwake(enumerablePoolerObj.transform);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public PoolHandler GetPoolHandler(ProjectileType type)
        {
            return projectilePooler.GetPoolHandler(type);
        }

        public PoolHandler GetPoolHandler(ImpactType type)
        {
            return impactPooler.GetPoolHandler(type);
        }

        public PoolHandler GetPoolHandler(FxType type)
        {
            return fxPooler.GetPoolHandler(type);
        }

        public PoolHandler GetPoolHandler(UnitType type)
        {
            return unitPooler.GetPoolHandler(type);
        }
    }
}