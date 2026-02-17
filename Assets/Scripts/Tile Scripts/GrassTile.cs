using UnityEngine;

public class GrassTile : Tile
{
    [SerializeField] private Color baseColour, offsetColour;

    public override void Init(int encounterID, int x, int y)
    {
        tileEncounter = encounterID;
        tileX = x;
        tileY = y;
        var isOffset = (x + y) % 2 == 1;
        rend.color = isOffset ? offsetColour : baseColour;
    }

}
