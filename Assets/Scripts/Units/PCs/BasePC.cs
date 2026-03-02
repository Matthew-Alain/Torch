using System;
using Unity.VisualScripting;
using UnityEngine;

public class BasePC : BaseUnit
{
    public string GetClass()
    {
        int class_id = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar( //Get the character's level
            "SELECT dnd_class_1 FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", UnitID)
        ));

        string dnd_class_1 = Convert.ToString(DatabaseManager.Instance.ExecuteScalar( //Get the character's level
            "SELECT name FROM dndclasses WHERE id = (@PCID)",
            ("@PCID", class_id)
        ));
        return dnd_class_1;
    }

    public int GetSpeed()
    {
        int savedSpeed = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar( //Get the character's level
            "SELECT speed FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", UnitID)
        ));
        return savedSpeed;
    }
}
