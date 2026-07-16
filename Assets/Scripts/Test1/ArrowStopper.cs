using UnityEngine;

public class ArrowStopper : MonoBehaviour
{
    private Arrow arrow;
    public LayerMask groundLayer;

    private void Awake()
    {
        arrow = GetComponentInParent<Arrow>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Layer Collision Matrix 쓸수있긴한데 공격판정 레이어를 떼는게 불안해서
        // 1 << 6 = 000...0001000000
        if (((1 << other.gameObject.layer) & groundLayer.value) != 0)
        {
            arrow.ArrowStop();
        }
    }
}
