using _KMH_Framework;
using FPS_Framework.Pool;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleHandler : MonoBehaviour
{
    protected ParticleSystem _particleSystem;

    [SerializeField]
    protected ImpactType impactType;

    protected void Awake()
    {
        _particleSystem = this.GetComponent<ParticleSystem>();
    }

    protected void OnEnable()
    {
        StartCoroutine(PostOnEnable());   
    }

    protected IEnumerator PostOnEnable()
    {
        yield return new WaitForSeconds(_particleSystem.duration);

        this.gameObject.ReturnPool(impactType);
    }
}
