using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsToggle : MonoBehaviour
{
    private List<MeshRenderer> Renderer { get; set; } = new List<MeshRenderer>();

    public void OnToggle(bool b) {
        if(b) {
            Renderer.ForEach(r => r.enabled = true);
        }
        else {
            Renderer.Clear();
            Renderer.AddRange(FindObjectsOfType<MeshRenderer>());
            Renderer.ForEach(r => r.enabled = false);
        }
    }
}
