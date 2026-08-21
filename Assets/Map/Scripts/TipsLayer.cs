using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipsLayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Canvas canvas = this.GetComponent<Canvas>();
        // 检查是否已存在Canvas组件
        if (canvas == null)
        {
            // 如果不存在，则动态添加一个Canvas组件
            canvas = this.gameObject.AddComponent<Canvas>();
        }
        // 设置Canvas的渲染顺序属性
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
