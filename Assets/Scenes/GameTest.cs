using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<DoubleClikeButton>().OnDoubleClick.AddListener(()=> {

            Log.Debug("双击");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
