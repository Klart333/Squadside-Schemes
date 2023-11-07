using Sirenix.OdinInspector;
using UnityEngine;

public class ParticleManager : Singleton<ParticleManager>
{
    [Title("Particle System")]
    [SerializeField]
    private PooledMonoBehaviour splashParticle;

    [SerializeField]
    private PooledMonoBehaviour poisonParticle;

    [SerializeField]
    private PooledMonoBehaviour lightParticle;

    [SerializeField]
    private PooledMonoBehaviour circleParticle;

    [SerializeField]
    private PooledMonoBehaviour leavesParticle;

    public PooledMonoBehaviour SplashParticle => splashParticle;
    public PooledMonoBehaviour PoisonParticle => poisonParticle;
    public PooledMonoBehaviour LightParticle => lightParticle;
    public PooledMonoBehaviour CircleParticle => circleParticle;
    public PooledMonoBehaviour LeavesParticle => leavesParticle;
}
