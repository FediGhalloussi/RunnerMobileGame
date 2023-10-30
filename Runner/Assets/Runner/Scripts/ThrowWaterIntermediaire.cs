using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HyperCasual.Runner
{
    public class ThrowWaterIntermediaire : MonoBehaviour
    {
        void ThrowWater()
        {
            PlayerController.Instance.OnThrowAnimationEnd();
        }
    }
}