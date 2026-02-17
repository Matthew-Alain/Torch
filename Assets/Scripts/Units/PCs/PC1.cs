using UnityEngine;

public class PC1 : BasePC
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

// I think this script doesn't need to exist. The tutorial I followed suggested
// creating individual scripts for each PC, but I think the expectation was that
// each PC would have a different set of pre-determined abilities, which is not
// the case for this. Instead, all PCs will have the "BasePC" script.
// I'm keeping this here for now in case I want to refer to it later