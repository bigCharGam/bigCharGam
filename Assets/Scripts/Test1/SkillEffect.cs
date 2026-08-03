using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    private Animator anim;
    void Awake()
    {
        anim = GetComponent<Animator>();
    }
    void Start()
    {
        //anim.Play("SkillEffect");
    }

    public void DestroyEffect()
    {
        Destroy(gameObject);
    }
}
