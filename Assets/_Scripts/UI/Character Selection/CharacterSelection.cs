using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    public virtual bool HandleCharacterSelectionInput(Ray ray, int playerIndex) => true;

    public virtual bool HandleCharacterDeselectionInput(int playerIndex) => true;
}
