using UnityEngine;

public class UnderseaGrappler : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = { (7, 9) };
        actionList.AddRange(actions);
        attackMod = 2;
        proficiency = 2;
    }
}
