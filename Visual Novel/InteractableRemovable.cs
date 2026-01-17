using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableRemovable : AInteractable
{
    public override void Interact()
    {
        Destroy(gameObject);
    }
}
