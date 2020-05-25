using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballance2
{
    //调试承载
    public class Debug : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(Run());
        }
        void Update()
        {

        }

        IEnumerator Run()
        {
            yield return new WaitUntil(GameManager.IsGameBaseInitFinished);


        }


    }
}
