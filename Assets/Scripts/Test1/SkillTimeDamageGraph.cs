using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillTimeDamageGraph", menuName = "ScriptableObject/TimeDamageGraph")]
public class SkillTimeDamageGraph : ScriptableObject
{
    // Define the time-damage graph for the skill
    public DamageGraph[] damageGraph;

    [System.Serializable]
    public class DamageGraph
    {
        public float time;
        public float damage;
    }
}
