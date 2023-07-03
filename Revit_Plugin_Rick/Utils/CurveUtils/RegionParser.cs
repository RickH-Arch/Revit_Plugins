﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using Revit_Plugin_Rick.Extension;
using Revit_Plugin_Rick.Utils;
using Revit_Plugin_Rick.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;
using System.Collections;

namespace Revit_Plugin_Rick.Utils.CurveUtils
{
    /// <summary>
    /// A class automatically group connected curves, just pass it curves!
    /// One instance of this class can only deal with one revit element
    /// This class can also export regions based on every curve group
    /// </summary>
    public class RegionParser
    {
        List<CurveGroup> curveGroups = new List<CurveGroup>();
        List<MetaCurve> floatCurves = new List<MetaCurve>();
        public const double CONNECT_THRESHOLD = 1.0e-6;
        public bool AddCurve(Curve curve)
        {
            MetaCurve newMc = new MetaCurve(curve);
            
            //any float metaCurve can connect this new metaCurve?
            List<MetaCurve> connectedCurves = new List<MetaCurve>();
            for (int i = 0; i < floatCurves.Count; i++)
            {
                MetaCurve connectedCurve = null;
                floatCurves[i].ConnectCurve(newMc, out connectedCurve);

                if ( connectedCurve != null)
                {
                    connectedCurves.Add(floatCurves[i]);
                    connectedCurves.Add(connectedCurve);
                    floatCurves.RemoveAt(i);
                    break;
                }

            }
            if (connectedCurves.Count > 0)
            {
                //new group appear,seek any float curve can connect this curve
                var newGroup = new CurveGroup(connectedCurves);
                for (int i = 0; i < floatCurves.Count; i++)
                {
                    MetaCurve connectedCurve;
                    if (newGroup.GroupConnectCurve(floatCurves[i], out connectedCurve) == ConnectResult.Success)
                    {
                        floatCurves.RemoveAt(i);
                        break;
                    }
                }
                AddToGroup(newGroup);
                
                
                return true;
            }
            

            //any group can connect this new metaCurve?
            foreach (var g in curveGroups)
            {
                MetaCurve connectedCurve;
                if(g.GroupConnectCurve(newMc,out connectedCurve) == ConnectResult.Success)
                {
                    curveGroups.RemoveAt(curveGroups.IndexOf(g));
                    AddToGroup(g);
                    return true;
                }
            }

            floatCurves.Add(newMc);
            return true;

        }


        /// <summary>
        /// this funcyion make sure curveGroups has no two connected groups that is seperate in the list
        /// </summary>
        /// <param name="cg"></param>
        private void AddToGroup(CurveGroup cg)
        {
            for(int i = 0; i < curveGroups.Count; i++)
            {
                if(cg.GroupConnectGroup(curveGroups[i]) == ConnectResult.Success)
                {
                    curveGroups.RemoveAt(i);
                    i--;
                }
            }
            curveGroups.Add(cg);
        }
       




        /// <summary>
        /// record a group of connected curves,
        /// this group ensure a curve at most connect two curves
        /// </summary>
        class CurveGroup
        {
            public List<MetaCurve> Curves { get; private set; }
            public CurveGroup(List<MetaCurve> curves)
            {
                foreach(var c in curves)
                {
                    Curves.Add(c);
                }
            }
            public CurveGroup() { }

            public ConnectResult GroupConnectCurve(MetaCurve mc_in,out MetaCurve connectedCurve)
            {
                connectedCurve = null;
                foreach(var mc in Curves)
                {
                    mc.ConnectCurve(mc_in,out connectedCurve);
                    if(connectedCurve != null)
                    {
                        if(connectedCurve.HeadCurve != null)
                        {
                            Curves.Insert(Curves.Count, connectedCurve);
                        }else if(connectedCurve.TailCurve != null)
                        {
                            Curves.Insert(0, connectedCurve);
                        }
                        return ConnectResult.Success;
                    }
                }
                return ConnectResult.Fail;
                
            }

            public ConnectResult GroupConnectGroup(CurveGroup targetGroup)
            {
                XYZ head = this.GetGroupEndPoint(0);
                XYZ tail = this.GetGroupEndPoint(1);
                XYZ targetHead = targetGroup.GetGroupEndPoint(0);
                XYZ targetTail = targetGroup.GetGroupEndPoint(1);

                if (head.DistanceTo(targetHead) < CONNECT_THRESHOLD)
                {
                    CurveGroup newGroup = targetGroup.GetReversedGroup();
                    this.MergeGroup(newGroup,0);
                    return ConnectResult.Success;
                }
                else if (head.DistanceTo(targetTail) < CONNECT_THRESHOLD)
                {
                    this.MergeGroup(targetGroup, 0);
                    return ConnectResult.Success;
                }
                else if (tail.DistanceTo(targetTail) < CONNECT_THRESHOLD)
                {
                    CurveGroup newGroup = targetGroup.GetReversedGroup();
                    this.MergeGroup(newGroup, 1);
                    return ConnectResult.Success;
                }
                else if (tail.DistanceTo(targetHead) < CONNECT_THRESHOLD)
                {
                    this.MergeGroup(targetGroup, 1);
                    return ConnectResult.Success;
                }
                return ConnectResult.Fail;
            }

            /// <summary>
            /// merge other group,if dim = 0, merge to head, if dim = 1, merge to tail
            /// two group's direction must be same, or merge process will fail
            /// </summary>
            /// <param name="in_group"></param>
            /// <param name="dim"></param>
            private void MergeGroup(CurveGroup in_group,int dim)
            {
                if(dim == 0)
                {
                    if (in_group.Curves[in_group.Curves.Count - 1].TailCurve != null) return;
                    in_group.Curves[in_group.Curves.Count - 1].TailCurve = this.Curves[0];
                    for(int i = 0; i < in_group.Curves.Count; i++)
                    {
                        Curves.Insert(i, in_group.Curves[i]);
                    }
                }
                else if(dim == 1)
                {
                    if (in_group.Curves[0].HeadCurve != null) return;
                    in_group.Curves[0].HeadCurve = this.Curves[Curves.Count - 1];
                    for(int i = 0; i < in_group.Curves.Count; i++)
                    {
                        this.Curves.Add(in_group.Curves[i]);
                    }
                }
            }

            public CurveGroup GetReversedGroup()
            {
                CurveGroup reversedGroup = new CurveGroup();
                //head(i==0)-tail ---> tail-head
                for(int i = 0; i < Curves.Count; i++)
                {
                    Curve c = Curves[i].Curve;
                    MetaCurve newC = new MetaCurve(c.Reverse());
                    
                    reversedGroup.Curves.Add(newC);
                    if (i != 0)
                    {
                        reversedGroup.Curves[0].TailCurve = reversedGroup.Curves[1];
                    }

                }
                return reversedGroup;
            }

            public XYZ GetGroupEndPoint(int ind)
            {
                if(ind == 0)
                {
                    return Curves[0].Curve.GetEndPoint(0);
                }
                else if(ind == 1)
                {
                    return Curves[Curves.Count - 1].Curve.GetEndPoint(1);
                }
                return null;
            }

        }

        /// <summary>
        /// record front curve and back curve
        /// </summary>
        public class MetaCurve
        {
            // back curve | self curve | front curve
            // -------->    --------->   --------->
            //             head      tail
            //      GetEndPoint(0)  GetEndPoint(1)

            public Curve Curve { get; set; }

            private MetaCurve headCurve;
            public MetaCurve HeadCurve
            {
                get
                {
                    return headCurve;
                }
                set
                {
                    var c = value;
                    headCurve = value;
                    if (value.TailCurve == null)
                    {
                        value.TailCurve = this;
                    }
                }
            }
            private MetaCurve tailCurve;
            public MetaCurve TailCurve
            {
                get
                {
                    return tailCurve;
                }
                set
                {
                    var c = value;
                    tailCurve = value;
                    if (value.HeadCurve == null)
                    {
                        value.HeadCurve = this;
                    }
                }
            }



            public MetaCurve(Curve curve)
            {
                Curve = curve;
            }

            public void ConnectCurve(MetaCurve c,out MetaCurve connectedCurve)
            {
                var curve = c.Curve;
                XYZ targetHead = curve.GetEndPoint(0);
                XYZ targetTail = curve.GetEndPoint(1);

                XYZ head = Curve.GetEndPoint(0);
                XYZ tail = Curve.GetEndPoint(1);

                connectedCurve = null;
                //target tail must be on head
                //if not, reverse it
                if (head.DistanceTo(targetHead) < CONNECT_THRESHOLD)
                {
                    if (HeadCurve != null)
                    {
                        return;
                    }
                    connectedCurve = new MetaCurve(curve.Reverse());
                    HeadCurve = connectedCurve;
                    return;
                }
                else if (head.DistanceTo(targetTail) < CONNECT_THRESHOLD)
                {
                    if (HeadCurve != null)
                    {
                        return;
                    }
                    HeadCurve = c;
                    connectedCurve = c;
                    return;
                }
                else if (tail.DistanceTo(targetHead) < CONNECT_THRESHOLD)
                {
                    if (TailCurve != null)
                    {
                        return;
                    }
                    TailCurve = c;
                    connectedCurve = c;
                    return;
                }
                else if (tail.DistanceTo(targetTail) < CONNECT_THRESHOLD)
                {
                    if (TailCurve != null)
                    { 
                        return;
                    }
                    connectedCurve = new MetaCurve(curve.Reverse());
                    TailCurve = connectedCurve;
                    return ;
                }
                
            }

            
        }
    }

        

    public enum ConnectResult
    {
        Success,
        Fail,
        Conflict,
    }

    

}