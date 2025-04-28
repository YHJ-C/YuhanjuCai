using Coffee.UIExtensions;
using UnityEngine;

public class MoneyShooter : MonoBehaviour
{
    public ParticleSystem moneyParticle;
    private UIParticleAttractor attr;

    public void Play(UIParticleAttractor attr,int m)
    {
        this.attr = attr;
        attr.AddParticleSystem(moneyParticle);
        moneyParticle.emission.SetBurst(0, new ParticleSystem.Burst(0f, m));
        moneyParticle.Play();

        Destroy(gameObject, moneyParticle.main.duration);
    }

    private void OnDestroy()
    {
        if (attr != null)
        {
            attr.RemoveParticleSystem(moneyParticle);
        }
    }


}
