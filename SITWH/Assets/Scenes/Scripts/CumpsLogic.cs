using UnityEngine;
using System.Collections;

public class CrumpsLogic : MonoBehaviour
{
    public Animator animator;
    public LayerMask grabbableLayer;

    public float health = 100f;
    public float healthChangeAmount = 10f;

    bool isReacting;
    bool isDead;
    Coroutine currentReaction;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        health = 100f;
    }

    public void OnGoodObjectDestroyed(Vector3 position)
    {
        if (isDead) return;

        health += 5f;
        StartReaction(GoodReaction());
    }

    public void OnBadObjectDestroyed(Vector3 position)
    {
        if (isDead) return;

        health -= healthChangeAmount;

        if (health <= 0)
        {
            health = 0;
            isDead = true;
            StartReaction(muer());
            return;
        }

        StartReaction(BadReaction());
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (IsInLayerMask(other.gameObject, grabbableLayer))
        {
            if (!isReacting)
                StartReaction(DodgeReaction());
        }
    }

    void StartReaction(IEnumerator reaction)
    {
        if (currentReaction != null)
            StopCoroutine(currentReaction);

        currentReaction = StartCoroutine(reaction);
    }

    IEnumerator GoodReaction()
    {
        isReacting = true;

        animator.ResetTrigger("NoTrigger");
        animator.ResetTrigger("DodgeTrigger");

        animator.SetTrigger("ClapTrigger");

        yield return new WaitForSeconds(1.2f);

        isReacting = false;
    }

    IEnumerator BadReaction()
    {
        isReacting = true;

        animator.ResetTrigger("ClapTrigger");
        animator.ResetTrigger("DodgeTrigger");

        animator.SetTrigger("NoTrigger");

        yield return new WaitForSeconds(1.2f);

        isReacting = false;
    }

    IEnumerator DodgeReaction()
    {
        isReacting = true;

        animator.SetTrigger("DodgeTrigger");

        yield return new WaitForSeconds(0.7f);

        isReacting = false;
    }

    IEnumerator muer()
    {
        isReacting = true;

        animator.SetTrigger("Death");

        yield return new WaitForSeconds(0.2f);

        isReacting = false;
    }

    bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }
}