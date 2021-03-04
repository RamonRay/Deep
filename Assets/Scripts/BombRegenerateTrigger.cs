using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombRegenerateTrigger : MonoBehaviour {

    public int bombIndex;
    public bool instantiateOnDestroy=true;
    private void OnDestroy()
    {
        if (instantiateOnDestroy)
        {
            BombInstantiation.instance.CreateBombWait(bombIndex);
        }
    }
}
