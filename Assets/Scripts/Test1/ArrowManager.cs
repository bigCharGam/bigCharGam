using UnityEngine;
using System.Collections.Generic; // Queue

public class ArrowManager : MonoBehaviour
{
    public static ArrowManager instance;

    [SerializeField] private int maxArrows = 50;
    private readonly Queue<Arrow> arrows = new Queue<Arrow>();
    
    private void Awake()
    {
        instance = this;
    }
    public void NewArrow(Arrow arrow)
    {
        arrows.Enqueue(arrow);
        if (arrows.Count > maxArrows)
        {
            Arrow oldArrow = arrows.Dequeue();
            Destroy(oldArrow.gameObject);
        }
    }
}
