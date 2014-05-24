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
    public int depth
    {
        get;
        private set;
    }

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

        depth = parent.depth + 1;
    }

    public OcTree(int w, int h, int l, Bounds bounds)
    {
        //루트 노드
        corners[(int)Direction.FrontTopLeft] = 0;
        corners[(int)Direction.FrontTopRight] = w;
        corners[(int)Direction.FrontBottomLeft] = (w + 1) * l;
        corners[(int)Direction.FrontBottomRight] = (w + 1) * (l + 1) - 1;

        corners[(int)Direction.BackTopLeft] = (w + 1) * (l + 1) * (h);
        corners[(int)Direction.BackTopRight] = corners[(int)Direction.BackTopLeft] + w;
        corners[(int)Direction.BackBottomLeft] = corners[(int)Direction.BackTopLeft] + (w + 1) * l;
        corners[(int)Direction.BackBottomRight] = corners[(int)Direction.BackTopLeft] + (w + 1) * (l + 1) - 1;

        depth = 0;

        Debug.Log("depth : " + (depth) + " "
            + "/ FTL " + corners[(int)Direction.FrontTopLeft]
            + "/ FTR " + corners[(int)Direction.FrontTopRight]
            + "/ BTL " + corners[(int)Direction.BackTopLeft]
            + "/ BTR " + corners[(int)Direction.BackTopRight]
            + "/ FBL " + corners[(int)Direction.FrontBottomLeft]
            + "/ FBR " + corners[(int)Direction.FrontBottomRight]
            + "/ BBL " + corners[(int)Direction.BackBottomLeft]
            + "/ BBR " + corners[(int)Direction.BackBottomRight]);

        this.bounds = bounds;
    }
    
    public void Build()
    {
        if (SubDivide() == false)
            return;

        for (int i = 0; i < 8; ++i)
        {
            if(childs[i].isMinimum == false)
                childs[i].Build();
        }
    }

    private OcTree AddChild(Direction direction, int[] corners)
    {
        OcTree child = new OcTree(this);
        child.SetCorners(corners);
        child.bounds = CalcBounds(direction);

        return child;
    }

    private OcTree AddChild(Direction direction, int frontTopLeft, int frontTopRight, int backTopLeft, int backTopRight, int frontBottomLeft, int frontBottomRight, int backBottomLeft, int backBottomRight)
    {
        OcTree child = new OcTree(this);

        child.corners[(int)Direction.FrontTopLeft] = frontTopLeft;
        child.corners[(int)Direction.FrontTopRight] = frontTopRight;
        child.corners[(int)Direction.BackTopLeft] = backTopLeft;
        child.corners[(int)Direction.BackTopRight] = backTopRight;

        child.corners[(int)Direction.FrontBottomLeft] = frontBottomLeft;
        child.corners[(int)Direction.FrontBottomRight] = frontBottomRight;
        child.corners[(int)Direction.BackBottomLeft] = backBottomLeft;
        child.corners[(int)Direction.BackBottomRight] = backBottomRight;

        child.bounds = CalcBounds(direction);

        Debug.Log("depth : " + (depth+1) + " "
            + "/ " + direction            
            + "/ FTL " + child.corners[(int)Direction.FrontTopLeft]
            + "/ FTR " + child.corners[(int)Direction.FrontTopRight]
            + "/ BTL " + child.corners[(int)Direction.BackTopLeft]
            + "/ BTR " + child.corners[(int)Direction.BackTopRight]
            + "/ FBL " + child.corners[(int)Direction.FrontBottomLeft]
            + "/ FBR " + child.corners[(int)Direction.FrontBottomRight]
            + "/ BBL " + child.corners[(int)Direction.BackBottomLeft]
            + "/ BBR " + child.corners[(int)Direction.BackBottomRight]);


        return child;
    }

    private Bounds CalcBounds(Direction dir)
    {
        Bounds bounds = new Bounds();
        bounds.size = bounds.size / 2.0f;

        Vector3 offset = bounds.center - bounds.size / 4.0f;
        offset.x = Mathf.Abs(offset.x);
        offset.y = Mathf.Abs(offset.y);
        offset.z = Mathf.Abs(offset.z);

        Vector3 childCenter = bounds.center;
        Vector3 d = Vector3.zero;

        //FrontTopLeft = 0, FrontTopRight, BackTopLeft, BackTopRight,
        //FrontBottomLeft, FrontBottomRight, BackBottomLeft, BackBottomRight

        //Top이면, 1, Bot이면 -1
        if ((int)dir / 4 == 0) d.y = 1.0f;
        else d.y = -1.0f;

        //left면 -1, right면 1
        if ((int)dir % 2 == 0) d.x = -1.0f;
        else d.x = 1.0f;

        //front면 1, back이면 -1
        if (((int)dir / 2) % 2 == 0) d.z = 1.0f;
        else d.z = -1.0f;

        childCenter.x += d.x * offset.x;
        childCenter.y += d.y * offset.y;
        childCenter.z += d.z * offset.z;

        bounds.center = childCenter;

        return bounds;
    }

    private void SetCorners(int[] corners)
    {
        for (int i = 0; i < 8; ++i)
            this.corners[i] = corners[i];
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

        int frontMidLeft = (corners[(int)Direction.FrontTopLeft] + corners[(int)Direction.FrontBottomLeft]) / 2;
        int frontMidRight = (corners[(int)Direction.FrontTopRight] + corners[(int)Direction.FrontBottomRight]) / 2;
        int backMidLeft = (corners[(int)Direction.BackTopLeft] + corners[(int)Direction.BackBottomLeft]) / 2;
        int backMidRight = (corners[(int)Direction.BackTopRight] + corners[(int)Direction.BackBottomRight]) / 2;

        int topSurfaceMid = (corners[(int)Direction.FrontTopLeft] + corners[(int)Direction.FrontTopRight] + corners[(int)Direction.BackTopLeft] + corners[(int)Direction.BackTopRight]) / 4;
        int botSurfaceMid = (corners[(int)Direction.FrontBottomLeft] + corners[(int)Direction.FrontBottomRight] + corners[(int)Direction.BackBottomLeft] + corners[(int)Direction.BackBottomRight]) / 4;
        int leftSurfaceMid = (corners[(int)Direction.FrontTopLeft] + corners[(int)Direction.BackTopLeft] + corners[(int)Direction.FrontBottomLeft] + corners[(int)Direction.BackBottomLeft]) / 4;
        int rightSurfaceMid = (corners[(int)Direction.FrontTopRight] + corners[(int)Direction.BackTopRight] + corners[(int)Direction.FrontBottomRight] + corners[(int)Direction.BackBottomRight]) / 4;
        int frontSurfaceMid = (corners[(int)Direction.FrontTopLeft] + corners[(int)Direction.FrontTopRight] + corners[(int)Direction.FrontBottomLeft] + corners[(int)Direction.FrontBottomRight]) / 4;
        int backSurfaceMid = (corners[(int)Direction.BackTopLeft] + corners[(int)Direction.BackTopRight] + corners[(int)Direction.BackBottomLeft] + corners[(int)Direction.BackBottomRight]) / 4;

        int center = 0 ;
        for (int i = 0; i < 8; ++i)
            center += corners[i];
        center /= 8;

        childs[(int)Direction.FrontTopLeft] = AddChild(Direction.FrontTopLeft, corners[(int)Direction.FrontTopLeft], frontTopMid, midTopLeft, topSurfaceMid, frontMidLeft, frontSurfaceMid, leftSurfaceMid, center);
        childs[(int)Direction.FrontTopRight] = AddChild(Direction.FrontTopRight, frontTopMid, corners[(int)Direction.FrontTopRight], topSurfaceMid, midTopRight, frontSurfaceMid, frontMidRight, center, rightSurfaceMid);
        childs[(int)Direction.BackTopLeft] = AddChild(Direction.BackTopLeft, midTopLeft, topSurfaceMid, corners[(int)Direction.BackTopLeft], backTopMid, leftSurfaceMid, center, backMidLeft, backSurfaceMid);
        childs[(int)Direction.BackTopRight] = AddChild(Direction.BackTopRight, topSurfaceMid, midTopRight, backTopMid, corners[(int)Direction.BackTopRight], center, rightSurfaceMid, backSurfaceMid, backMidRight);
        childs[(int)Direction.FrontBottomLeft] = AddChild(Direction.FrontBottomLeft, frontMidLeft, frontSurfaceMid, leftSurfaceMid, center, corners[(int)Direction.FrontBottomLeft], frontBotMid, midBotLeft, botSurfaceMid);
        childs[(int)Direction.FrontBottomRight] = AddChild(Direction.FrontBottomRight, frontSurfaceMid, frontMidRight, center, rightSurfaceMid, frontBotMid, corners[(int)Direction.FrontBottomRight], botSurfaceMid, midBotRight);
        childs[(int)Direction.BackBottomLeft] = AddChild(Direction.BackBottomLeft, leftSurfaceMid, center, backMidLeft, backSurfaceMid, midBotLeft, botSurfaceMid, corners[(int)Direction.BackBottomLeft], backBotMid);
        childs[(int)Direction.BackBottomRight] = AddChild(Direction.BackBottomRight, center, rightSurfaceMid, backSurfaceMid, backMidRight, botSurfaceMid, midBotRight, backBotMid, corners[(int)Direction.BackBottomRight]);

        return true;
    }
}
