using UnityEngine;
using System.Collections;
using System;

public class OcTree
{
    private enum Direction : int
    {
        FrontTopLeft = 0, FrontTopRight, BackTopLeft, BackTopRight,
        FrontBottomLeft, FrontBottomRight, BackBottomLeft, BackBottomRight
    };

    private OcTree   parent   = null;
    private int[]    corners   = new int[8]; // Enum.GetValues(typeof(Direction)).Length

    private int      centerIndex;
    private Bounds   bounds;

    private OcTree[] childs   = new OcTree[8]; // Enum.GetValues(typeof(Direction)).Length

    public bool isMinimum
    {
        get
        {
            return (corners[(int)Direction.FrontTopRight] - corners[(int)Direction.FrontTopLeft] <= 1);
        }
    }

    public OcTree(OcTree parent)
    {
        this.parent = parent;
        centerIndex = 0;

        for (int i = 0; i < 8; ++i)
        {
            childs[i] = null;
            corners[i] = 0;
        }

        bounds = new Bounds(Vector3.zero, Vector3.zero);
    }

    public OcTree(int w, int h, int l)
    {
        //루트 노드
        corners[(int)Direction.FrontTopLeft] = 0;
        corners[(int)Direction.FrontTopRight] = w;
        corners[(int)Direction.BackTopLeft] = (w + 1) * l;
        corners[(int)Direction.BackTopRight] = (w + 1) * (l + 1) - 1;

        corners[(int)Direction.FrontBottomLeft] = (w + 1) * (l + 1) * (h - 1);
        corners[(int)Direction.FrontBottomRight] = corners[(int)Direction.FrontBottomLeft] + w;
        corners[(int)Direction.BackBottomLeft] = corners[(int)Direction.FrontBottomLeft] + (w + 1) * l;
        corners[(int)Direction.BackBottomRight] = corners[(int)Direction.FrontBottomLeft] + (w + 1) * (l + 1) - 1;

        //bound 설정
    }

    void Build()
    {
        //center
        //aabb

        if (SubDivide() == false)
            return;

        for (int i = 0; i < 8; ++i)
            childs[i].Build();
    }

    private OcTree AddChild(int[] corners)
    {
        OcTree child = new OcTree(this);
        child.SetCorners(corners);

        return child;
    }

    private void SetCorners(int[] corners)
    {
        for (int i = 0; i < 8; ++i)
            this.corners[i] = corners[i];

        //bounds
    }

    private bool SubDivide()
    {
        if (isMinimum)
            return true;

        int frontTopMid = (corners[(int)Direction.FrontTopLeft] + corners[(int)Direction.FrontTopRight]) / 2;
        int backTopMid = (corners[(int)Direction.BackTopLeft] + corners[(int)Direction.BackTopRight]) / 2;
        int midTopLeft = (corners[(int)Direction.FrontTopLeft] + corners[(int)Direction.BackTopLeft]) / 2;
        int midTopRight = (corners[(int)Direction.FrontTopRight] + corners[(int)Direction.BackTopRight]) / 2;

        int frontBotMid = (corners[(int)Direction.FrontBottomLeft] + corners[(int)Direction.FrontBottomRight]) / 2;
        int backBotMid = (corners[(int)Direction.BackBottomLeft] + corners[(int)Direction.BackBottomRight]) / 2;
        int midBotLeft = (corners[(int)Direction.FrontBottomLeft] + corners[(int)Direction.BackBottomLeft]) / 2;
        int midBotRight = (corners[(int)Direction.FrontBottomRight] + corners[(int)Direction.BackBottomRight]) / 2;

        int topMid = (corners[(int)Direction.FrontTopLeft] + corners[(int)Direction.FrontTopRight] + corners[(int)Direction.BackTopLeft] + corners[(int)Direction.BackTopRight]) / 4;
        int botMid = (corners[(int)Direction.FrontBottomLeft] + corners[(int)Direction.FrontBottomRight] + corners[(int)Direction.BackBottomLeft] + corners[(int)Direction.BackBottomRight]) / 4;
        int leftMid = (corners[(int)Direction.FrontTopLeft] + corners[(int)Direction.BackTopLeft] + corners[(int)Direction.FrontBottomLeft] + corners[(int)Direction.BackBottomLeft]) / 4;
        int rightMid = (corners[(int)Direction.FrontTopRight] + corners[(int)Direction.BackTopRight] + corners[(int)Direction.FrontBottomRight] + corners[(int)Direction.BackBottomRight]) / 4;
        int frontMid = (corners[(int)Direction.FrontTopLeft] + corners[(int)Direction.FrontTopRight] + corners[(int)Direction.FrontBottomLeft] + corners[(int)Direction.FrontBottomRight]) / 4;
        int backMid = (corners[(int)Direction.BackTopLeft] + corners[(int)Direction.BackTopRight] + corners[(int)Direction.BackBottomLeft] + corners[(int)Direction.BackBottomRight]) / 4;

        int center = 0 ;
        for (int i = 0; i < 8; ++i)
            center += corners[i];
        center /= 8;



        return true;
    }
}
