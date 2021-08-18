using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
The purpose of this script is to be used on start interaction to change animator boolean value.
Note: Can not be set as static class because unity actions does not support it. This script should be attached to the gameobject.
     
*/

public class AnimatorExtensions : MonoBehaviour{

    public Animator _animator;
    public void SetBoolTrue(string bool_name)
    {
        _animator.SetBool(bool_name, true);
    }

    public void SetBoolFalse(string bool_name)
    {
        _animator.SetBool(bool_name, false);
    }
}
