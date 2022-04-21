using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    //void Start() {
    //   Time.timeScale = PublicTimeScale;
    //}
    //void Update() {
    //    Time.timeScale = PublicTimeScale;
    //}

    public void OnTimeScaleChanged(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}
