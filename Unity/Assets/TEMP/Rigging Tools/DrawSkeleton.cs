using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawSkeleton : MonoBehaviour
{
    public Color color = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (gameObject.activeInHierarchy && enabled)
        {
            Gizmos.color = color;
            var render = GetComponentInChildren<SkinnedMeshRenderer>();
            DrawChildBones(render.rootBone);
        }
    }

    private void DrawChildBones(Transform start)
    {
        for (int i = 0; i < start.childCount; i++)
        {
            var child = start.GetChild(i);
            Gizmos.DrawLine(start.position, child.position);
            DrawChildBones(child);
        }
    }

}
