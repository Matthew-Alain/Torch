using UnityEngine;

public class GoblinMage : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = { (8, 2) };
        actionList.AddRange(actions);
    }
}
