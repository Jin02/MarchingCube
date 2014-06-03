using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class OcTree
{
    private enum Direction : int
    {
        FrontTopLeft = 0, FrontTopRight, BackTopLeft, BackTopRight,
        FrontBottomLeft, FrontBottomRight, BackBottomLeft, BackBottomRight
    };

    public OcTree parent
    {
        get;
        private set;
    }
    public int[] corners
    {
        get;
        private set;
    }
    public Bounds bounds
    {
        get;
        private set;
    }

    public OcTree[] childs
    {
        get;
        private set;
    }
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
        childs = new OcTree[8];
        corners = new int[8];

        for (int i = 0; i < 8; ++i)
        {
            childs[i] = null;
            corners[i] = 0;
        }

        depth = parent.depth + 1;
    }

    public OcTree(int w, int h, int l, Bounds bounds)
    {
        corners = new int[8];

        //루트 노드
        //int 값 하나로 그냥 모든 코너 값 세팅
        corners[(int)Direction.BackTopLeft] = 0;
        corners[(int)Direction.BackTopRight] = w;
        corners[(int)Direction.BackBottomLeft] = (w + 1) * l;
        corners[(int)Direction.BackBottomRight] = (w + 1) * (l + 1) - 1;

        corners[(int)Direction.FrontTopLeft] = (w + 1) * (l + 1) * (h);
        corners[(int)Direction.FrontTopRight] = corners[(int)Direction.FrontTopLeft] + w;
        corners[(int)Direction.FrontBottomLeft] = corners[(int)Direction.FrontTopLeft] + (w + 1) * l;
        corners[(int)Direction.FrontBottomRight] = corners[(int)Direction.FrontTopLeft] + (w + 1) * (l + 1) - 1;

        depth = 0;

        this.bounds = bounds;
        this.parent = null;
        childs = new OcTree[8];
    }

    public void Build()
    {
        if (SubDivide() == false)
            return;

        for (int i = 0; i < 8; ++i)
        {
            if (childs[i].isMinimum == false)
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

        //Debug.Log("depth : " + (depth+1) + " "
        //    + "/ " + direction            
        //    + "/ FTL " + child.corners[(int)Direction.FrontTopLeft]
        //    + "/ FTR " + child.corners[(int)Direction.FrontTopRight]
        //    + "/ BTL " + child.corners[(int)Direction.BackTopLeft]
        //    + "/ BTR " + child.corners[(int)Direction.BackTopRight]
        //    + "/ FBL " + child.corners[(int)Direction.FrontBottomLeft]
        //    + "/ FBR " + child.corners[(int)Direction.FrontBottomRight]
        //    + "/ BBL " + child.corners[(int)Direction.BackBottomLeft]
        //    + "/ BBR " + child.corners[(int)Direction.BackBottomRight]);
        //Debug.Log("depth : " + (depth + 1) + " " + "/ " + direction + " / " + child.bounds.center);

        return child;
    }

    private Bounds CalcBounds(Direction dir)
    {
        //Bound 계산 함수
        //코너 값에 따라 위치세팅. 사이즈는 그냥 현재 노드 값 나눠서 씀

        Bounds bounds = new Bounds();
        bounds.size = this.bounds.size / 2.0f;

        Vector3 offset = this.bounds.size / 4.0f;
        offset.x = Mathf.Abs(offset.x);
        offset.y = Mathf.Abs(offset.y);
        offset.z = Mathf.Abs(offset.z);

        Vector3 childCenter = this.bounds.center;
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

    public static void Calc3rdDimIdx(int idx, int w, int h, int l, out int x, out int y, out int z)
    {
        // Decode idx
        // 코너 값을 3차원 상 인덱스 값으로 다시 구해줌
        
        int tableCount = (w + 1) * (l + 1);

        z = (idx / tableCount);
        
        // 범위 넘으면 그냥 강제 세팅
        if (z >= l) z = l - 1;
        else if (z < 0) z = 0;

        y = (h - 1) - (idx % tableCount) / (h + 1);

        // 범위 넘으면 그냥 강제 세팅
        if (y >= h) y = h - 1;
        else if (y < 0) y = 0;

        x = (idx % tableCount) % (w + 1);

        // 범위 넘으면 그냥 강제 세팅
        if (x >= w) x = w - 1;
        else if (x < 0) x = 0;
    }

    private bool SubDivide()
    {
        //막상 복잡해 보이기는 하는데,
        //그냥 8개 코너 값을 잡고 반 씩 나누면서 공간 재 세팅하는 작업.
        //변수명을 읽어보시면 편합니다.
        
        if (isMinimum)
            return false;

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

        int center = 0;
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

    public bool find(Ray ray, ref List<OcTree> nodeList, ref List<float> distList, int w, int h, int l, float[, ,] densitys)
    {
        bool find = false;
        if (bounds.IntersectRay(ray))
        {
            if (isMinimum)
            {
                find = true;

                //Debug.Log(bounds);
                bool isOutRange = false;

                for (int i = 0; i < 8; ++i)
                {
                    int x, y, z;
                    //Decode corners[i] to xyz
                    Calc3rdDimIdx(corners[i], w, h, l, out x, out y, out z);

                    if (((x + 1) > w) && ((y + 1) > h) && ((z + 1) > l))
                    {
                        isOutRange = true;
                        break;
                    }
                }

                if (isOutRange == false)
                {
                    //찾음
                    nodeList.Add(this);
                    distList.Add(Vector3.Distance(Camera.main.transform.localPosition, bounds.center));
                }
            }
            else
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (CheckCornerInRange(densitys, w, h, l))
                    {
                        //자식들 검사.
                        find = childs[i].find(ray, ref nodeList, ref distList, w, h, l, densitys);
                    }
                }
            }
        }

        return find;
    }

    bool CheckCornerInRange(float[, ,] densitys, int w, int h, int l)
    {
        //현재 노드의 코너 값들이 범위 안에 있는지 체크
        for (int i = 0; i < 8; ++i)
        {
            int x, y, z;
            Calc3rdDimIdx(corners[i], w, h, l, out x, out y, out z);

            if (x < w && y < h && z < l)
                return true;
        }

        return false;
    }
}
