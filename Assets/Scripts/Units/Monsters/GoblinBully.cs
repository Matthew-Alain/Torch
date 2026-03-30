using UnityEngine;

public class GoblinBully : BaseMonster
{
    void Start()
    {
        (int, int)[] actions = { (4, 2) };
        actionList.AddRange(actions);
        attackMod = 2;
        saveDC = 13;
    }
}
