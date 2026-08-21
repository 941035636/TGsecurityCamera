using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeopleManagerPageUi : Window
{

    public PeopleManagentPanel _MainPeoplePanel;
    public override void Awake(params object[] paralist)
    {
        _MainPeoplePanel = GameObject.GetComponent<PeopleManagentPanel>();

    }

}
