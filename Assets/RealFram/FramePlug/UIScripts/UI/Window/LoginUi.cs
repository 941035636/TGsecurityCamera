using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginUi : Window
{
    LoginPanel _loginpanel;
    public override void Awake(params object[] paralist)
    {
        _loginpanel = GameObject.GetComponent<LoginPanel>();

    }
}
