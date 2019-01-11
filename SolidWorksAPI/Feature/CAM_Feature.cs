using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolidWorks.Interop.sldcostingapi;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.IO;
using CAMWORKSLib;

namespace SolidWorksAPI
{
    /// <summary>
    /// CAM特征处理类
    /// </summary>
    public class CAM_Feature
    {
        /* 说明:先执行GetFeatuer方法 获取所有特征  拿结果在去调用 ComputeFeature 方法来计算出 所有特征金额 放入到 TotalFeatureMoney 当中 在去调用 GetTotalMoney 获得最后金额  
         * 如果有额外的特征出现 可直接追加 GetFeature_XXX 方法 在 ComputeFeature 中添加调用即可 */

        /// <summary>
        /// 全部特征总结算结果
        /// </summary>
        public List<FeatureAmount> TotalFeatureMoney = new List<FeatureAmount>();
        /// <summary>
        /// 获取零件设置 次数 （为了获取装夹时间 ）
        /// </summary>
        private int SetCount = 0;
        /// <summary>
        /// 毛坯尺寸
        /// </summary>
        public double[] StockSize = new double[3];

        public CAM_Feature()
        {
          
        }

        #region ===== 函数 =====
        /// <summary>
        /// 计算铣削总特征
        /// </summary>
        /// <param name="swCAM"></param>
        public void ComputeFeature_Mill(List<SwCAM_Mill> swCAMs)
        {
            foreach (SwCAM_Mill item in swCAMs)
            {
                switch (item.VolumeType)
                {
                    case (int)CWVolumeType_e.CW_HOLE_VOLUME://孔或者孔组
                        GetFeature_Hole(item);
                        break;
                    case (int)CWVolumeType_e.CW_HOLECTRSUNK_VOLUME: //埋头孔
                        GetFeature_HoleCtrsunk(item);
                        break;
                    case (int)CWVolumeType_e.CW_HOLECTRBORE_VOLUME: //沉镗孔
                        GetFeature_HoleCtrbore(item);
                        break;
                    case (int)CWVolumeType_e.CW_MULTISTEP_VOLUME:   //MS孔  (多阶)
                        GetFeature_MSHole(item);
                        break;
                    case (int)CWVolumeType_e.CW_POCKET_VOLUME://不规则凹腔 、圆形凹腔、腰形凹腔、矩形凹腔
                        if (item.FeatureName.IndexOf("矩形") >= 0)
                            GetFeature_RectangleCavity(item);
                        else if (item.FeatureName.IndexOf("腰型") >= 0)
                            GetFeature_KidneyPock(item);
                        else if (item.FeatureName.IndexOf("圆形凹腔") >= 0)
                            GetFeature_CirclePock(item);
                        else
                            GetFeature_AnomalyCavity(item);//不规则凹腔
                        break;
                    case (int)CWVolumeType_e.CW_SLOT_VOLUME://不规则槽、矩形槽、腰形槽
                        if (item.FeatureName.IndexOf("矩形") >= 0)
                            GetFeature_RectangleGroove(item);
                        else if (item.FeatureName.IndexOf("腰型") >= 0)
                            GetFeature_KidneySlot(item);
                        else
                            GetFeature_AnomalyGroove(item);//不规则槽
                        break;
                    case (int)CWVolumeType_e.CW_OPENPOCKET_VOLUME: //开放式凹腔 、周界-非封闭凹腔
                        if (item.FeatureName.IndexOf("开放式凹腔") >= 0)
                            GetFeature_OpenCavity(item);
                        break;
                    case (int)CWVolumeType_e.CW_BOSS_VOLUME://圆形凸台 
                        break;
                    case (int)CWVolumeType_e.CW_SLAB_VOLUME: //面特征
                        break;
                    case (int)CWVolumeType_e.CW_MULTIFACE_VOLUME:
                        break;
                    case (int)CWVolumeType_e.CW_WORKPIECE_VOLUME:
                        break;
                    case (int)CWVolumeType_e.CW_3AXIS_VOLUME:
                        break;
                    default:
                        break;

                }
            }
            GetSetFixture();//装夹时间
            GetFaceMill();//面部粗铣
            GetFaceMill();//面部粗铣
            GetProfileMill();//外轮廓粗铣
        }
        /// <summary>
        /// 获取特征总金额 [要先执行 ComputeFeature 方法]
        /// </summary>
        /// <returns></returns>
        public decimal GetTotalMoney()
        {
            decimal tolMoney = 0;
            foreach (FeatureAmount item in TotalFeatureMoney)
            {
                tolMoney += item.Money;
            }
            return tolMoney;
        }
        /// <summary>
        /// 获取特征总时长 [要先执行 ComputeFeature 方法]
        /// </summary>
        /// <returns></returns>
        public double GetTotalTime()
        {
            double tolTime = 0;
            foreach (FeatureAmount item in TotalFeatureMoney)
            {
                tolTime += item.TotalTime;
            }
            return tolTime;
        }
        /// <summary>
        /// 获取机床类型（元/小时）
        /// </summary>
        /// <returns></returns>
        public double GetMachineMoney()
        {
            return 40;
        }
        /// <summary>
        /// 长宽高转换(根据数值不同重新分配长宽高)
        /// </summary>
        /// <param name="oldBound"></param>
        /// <param name="Depth"></param>
        /// <returns></returns>
        public double[] ConvertLWH(double[] oldBound, double Depth)
        {
            List<double> temp = oldBound.ToList();
            if (Depth != -1000)
                temp.Add(Depth);
            temp.Sort();
            temp.Reverse();//倒叙排列
            return temp.ToArray();
        }
        /// <summary>
        /// 裁剪次数 （深度/刀具直径*0.25）
        /// </summary>
        /// <returns></returns>
        public int NumberOfWalkCut(double Depth, double Dia)
        {
            double cutTools = Dia * 0.25;//算出刀具每次的切割深度
            double sumWalk = Depth / cutTools;//换算出 总共需要走多少次
            double Cei = Math.Ceiling(sumWalk);//获取最大整数  例： 3.1 = 4
            return Convert.ToInt32(Cei);
        }
        /// <summary>
        /// 获取材料
        /// </summary>
        /// <returns></returns>
        public Materials GetMaterials()
        {
            return Materials.Aluminum;
        }
        /// <summary>
        /// 获取岛屿的预估尺寸（单个岛屿）
        /// </summary>
        /// <returns></returns>
        public double[] GetIsLandArea(double length,double width,int isLandCount)
        {
            switch (isLandCount)
            {
                case 1:
                    return new double[] { length * 0.2 , width * 0.2  };// 1个岛屿 占有 20% 特征面积
                case 2:
                    return new double[] { length * 0.3 / isLandCount, width * 0.3 / isLandCount };// 2个岛屿 占有 30% 特征面积
                case 3:
                    return new double[] { length * 0.4 / isLandCount, width * 0.4 / isLandCount };// 3个岛屿 占有 40% 特征面积
                case 4:
                    return new double[] { length * 0.5 / isLandCount, width * 0.5 / isLandCount };// 4个岛屿 占有 50% 特征面积
                case 5:
                    return new double[] { length * 0.6 / isLandCount, width * 0.6 / isLandCount };// 5个岛屿 占有 60% 特征面积
                default:
                    return new double[] { length * 0.7 / isLandCount, width * 0.7 / isLandCount };// 大于5个岛屿最多只占有 70% 特征面积
            }
        }
        #endregion

        #region ===== 获取特征相关方法 =====
        /// <summary>
        /// 获取铣削所有特征
        /// </summary>
        /// <returns></returns>
        public List<SwCAM_Mill> GetFeatuer_Mill()
        {
            try
            {
                List<SwCAM_Mill> swList = new List<SwCAM_Mill>();//获取全部特征

                CWApp cwApp = new CWApp();
                CWPartDoc cwPd = (CWPartDoc)cwApp.IGetActiveDoc();
                CWDoc cwDoc = (CWDoc)cwApp.IGetActiveDoc();

                cwApp.ActiveDocEMF();//提取特征 

                CWPartDoc cwPartDoc = (CWPartDoc)cwDoc;
                CWMachine cwMach = (CWMachine)cwPartDoc.IGetMachine();

                CWDispatchCollection cwDispCol = (CWDispatchCollection)cwMach.IGetEnumSetups();

                ICWDoc3 a1 = (ICWDoc3)cwDoc;
                double d1, d2, d3, d4, d5, d6;
                a1.GetBoxRecordParams(out d1, out d2, out d3, out d4, out d5, out d6);
                StockSize = ConvertLWH(new double[] { d4, d5, d6 }, -1000);//获取毛坯尺寸

                if (cwDispCol.Count == 0)
                {
                    Console.Write("该图纸无法提取特征！");
                    return null;
                }

                for (int i = 0; i < cwDispCol.Count; i++)// 铣削零件设置组  如果count=0 则代表提取特征失败
                {
                    CWBaseSetup cwBaseSetup = (CWBaseSetup)cwDispCol.Item(i);//获取 铣削零件设置 

                    SetCount = cwDispCol.Count;//获取零件设置 次数。。
                    if (cwBaseSetup == null)
                        continue;

                    ICWDispatchCollection FeatList = (ICWDispatchCollection)cwBaseSetup.IGetEnumFeatures();//方法获取程序上的所有特性

                    for (int j = 0; j < FeatList.Count; j++)
                    {
                        ICWFeature pThisFeat = FeatList.Item(j);

                        SwCAM_Mill obj = new SwCAM_Mill();
                        obj.FeatureName = pThisFeat.FeatureName;
                        obj.FeatureType = pThisFeat.FeatType;
                        obj.Axis = pThisFeat.Axis;
                        obj.Suppressed = pThisFeat.Suppressed;

                        double[] atts = pThisFeat.GetFeatureAttributes();//获得该特征的参数属性

                        if (pThisFeat.FeatType == (int)CWFeatType_e.CW_FEAT_TYPE_MILL)
                        {
                            double[] stPos = { 0, 0, 0 };
                            ICWMillFeature pMillFeat = (CWMillFeature)pThisFeat;
                            pMillFeat.GetStartPosition(out stPos[0], out stPos[1], out stPos[2]);
                            obj.StartPosition = stPos;
                            obj.Shape = pMillFeat.Shape; //CWShpeType_e 类型
                            obj.Depth = pMillFeat.Depth;
                            string strAttribute = ""; int nIdAttribute = 0;
                            pMillFeat.GetAttribute(out strAttribute, out nIdAttribute);
                            obj.Tactics = strAttribute;
                            obj.TacticsID = nIdAttribute;
                            obj.TdbIdForAttribute = pMillFeat.GetTdbIdForAttribute(strAttribute);
                            obj.TopFilletRadius = pMillFeat.IGetTopFilletRadius();
                            obj.Chamfer = atts[16];//倒角
                            obj.Maxdiameter = atts[1];//直径
                            obj.BottomRadius = atts[20];//底部半径
                            obj.Mindiameter = atts[9];//精加工半径 
                            obj.Bound = new double[] { atts[7], atts[8], atts[9] };//长、宽、高

                            //double[] stBound = { 0, 0, 0 };
                            //pMillFeat.GetBoundParams(out stBound[0], out stBound[1], out stBound[2]);
                            //obj.Bound = stBound;

                            if (pMillFeat.VolumeType == 4)
                            {

                                //ICWTaperTool ssd = (ICWTaperTool)cwMach.IGetToolcrib();
                                //ICWDispatchCollection ddd = (ICWDispatchCollection)cwMach.IGetToolcrib();
                                //for (int u = 0; u < ddd.Count; u++)
                                //{
                                //    object fsfs = ddd.Item(u);

                                //}

                                //ICWOperation op = (ICWOperation)pThisFeat.GetOperationList();
                                //ICWTool col = (ICWTool)op.IGetTool();
                                //ICWTaperTool tap = (ICWTaperTool)op.IGetTool();


                            }

                            double Distance = 0;
                            //pMillFeat.GetDistanceUptoStock(out Distance);
                            obj.DistanceUptoStock = Distance;

                            obj.VolumeType = pMillFeat.VolumeType;

                            ///======= 获取特征独有属性
                            SetVolumeType(pMillFeat.VolumeType, atts, pMillFeat, obj);

                            List<Island> islands = new List<Island>();//获取岛屿
                            ICWDispatchCollection islandCollection = pMillFeat.IGetIslands();
                            object jjjj = pMillFeat.IGetIslands();
                            if (pThisFeat.FeatureName == "矩形凹腔1")
                            {
                                int landCount = pMillFeat.IGetIslandCount();
                                object objjjj = pMillFeat.IGetIslands();

                                foreach (CWIslandInfo item in islandCollection)
                                {
                                    Island island = new Island();
                                    island.Depth = item.GetDepth();
                                    islands.Add(island);
                                }

                                for (int m = 0; m < islandCollection.Count; m++)
                                {
                                    object cc = islandCollection.Item(m);
                                }

                            }


                            foreach (CWIslandInfo item in islandCollection)
                            {
                                Island island = new Island();
                                island.Depth = item.GetDepth();
                                islands.Add(island);
                            }

                            obj.Islands = islands;
                            obj.IslandCount = pMillFeat.IGetIslandCount();
                            obj.ThroughOrblind = pMillFeat.SubType;
                            obj.CornerRadius = pMillFeat.IGetCornerRadius();
                            obj.TDBFeatID = pMillFeat.GetTDBFeatID();
                            obj.Taper = pMillFeat.Taper;

                            if (obj.Taper)
                            {

                                //TaperInfo ti = new TaperInfo();
                                //ti.TaperAngle = pMillFeat.IGetTaperAngle();
                                ////ti.TaperHeight = obj.BottomRadius == 0 ? obj.Depth : (obj.Depth - obj.BottomRadius - obj.Mindiameter);//底部半径==0？深度：（深度-底部半径-精加工半径）
                                //ti.BottomArcFaceRadius = obj.BottomRadius;
                                ////ti.BottomArcFaceHeight = obj.BottomRadius == 0 ? 0 : (obj.Depth - ti.TaperHeight - obj.Mindiameter);//（深度-锥度高-精加工半径）= 底部高度

                            }



                            if (pMillFeat.IsPatternFeature()) //获取组信息， 子列表
                            {
                                //获取子特征
                                GetSubFeatuer((ICWPatternFeature)pMillFeat, obj);
                            }
                        }
                        #region 车削
                        else if (pThisFeat.FeatType == (int)CWFeatType_e.CW_FEAT_TYPE_TURN)
                        {
                            Console.Write("车削特征:" + pThisFeat.FeatureName + "\n");
                            ICWTurnFeature pturnFeat = (CWTurnFeature)pThisFeat;

                            Console.Write(pturnFeat.GetAttributeName() + "\n");

                            double minDia = 0;
                            double maxDia = pturnFeat.GetMinMaxDiameter(out minDia);

                            Console.Write("minDia:" + minDia + ",maxDia:" + maxDia + "\n");
                            Console.Write("FeatureType:" + pturnFeat.GetTurnFeatureType() + "\n");
                            Console.Write("是否主轴：" + pturnFeat.GetSpindleType().ToString() + "\n");
                            Console.Write("类型" + pturnFeat.GetTurnFeatureType() + "\n");

                            double sx = 0, sy = 0;
                            double ex = 0, ey = 0;
                            pturnFeat.GetStartPosition(out sx, out sy);

                            Console.Write("sx:" + minDia + ",sy:" + maxDia + "\n");
                            pturnFeat.GetEndPosition(out ex, out ey);
                            Console.Write("ex:" + minDia + ",ey:" + maxDia + "\n");
                            Console.Write("面长：" + atts[7] + "\n");

                            switch (pturnFeat.GetTurnFeatureType())
                            {
                                case (int)CWFeatureCatalog_e.CW_FEAT_SLOT_RECT:
                                    ICWSlotRectFeat srf = (CWSlotRectFeat)pturnFeat;
                                    Console.Write("根部直径：" + srf.IGetCornerRadius() + "\n");
                                    Console.Write("宽度：" + srf.IGetWidth() + "\n");
                                    Console.Write("深度：" + srf.IGetLength() + "\n");
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            Console.Write("其他特征:" + pThisFeat.FeatType + "\n");
                        }
                        #endregion
                        swList.Add(obj);

                    }

                }

                return swList;
            }
            catch (Exception ep)
            {
                Console.Write(ep.Message);
                return null;
            }

        }
        /// <summary>
        /// 获取车削所有特征
        /// </summary>
        /// <returns></returns>
        public List<SwCAM_Turn> GetFeatuer_Turn()
        {
            try
            {
                List<SwCAM_Turn> swList = new List<SwCAM_Turn>();//获取全部特征

                CWApp cwApp = new CWApp();
                CWPartDoc cwPd = (CWPartDoc)cwApp.IGetActiveDoc();
                CWDoc cwDoc = (CWDoc)cwApp.IGetActiveDoc();

                cwApp.ActiveDocEMF();//提取特征 

                CWPartDoc cwPartDoc = (CWPartDoc)cwDoc;
                CWMachine cwMach = (CWMachine)cwPartDoc.IGetMachine();

                CWDispatchCollection cwDispCol = (CWDispatchCollection)cwMach.IGetEnumSetups();

                if (cwDispCol.Count == 0)
                {
                    Console.Write("该图纸无法提取特征！");
                    return null;
                }

                for (int i = 0; i < cwDispCol.Count; i++)// 铣削零件设置组  如果count=0 则代表提取特征失败
                {
                    CWBaseSetup cwBaseSetup = (CWBaseSetup)cwDispCol.Item(i);//获取 铣削零件设置 

                    if (cwBaseSetup == null)
                        continue;

                    ICWDispatchCollection FeatList = (ICWDispatchCollection)cwBaseSetup.IGetEnumFeatures();//方法获取程序上的所有特性

                    for (int j = 0; j < FeatList.Count; j++)
                    {
                        ICWFeature pThisFeat = FeatList.Item(j);
                        double[] atts = pThisFeat.GetFeatureAttributes();//获得该特征的参数属性

                        SwCAM_Turn obj = new SwCAM_Turn();
                        obj.FeatureName = pThisFeat.FeatureName;
                        obj.FeatureType = pThisFeat.FeatType;
                        obj.Length = atts[7];
                        obj.Mindiameter = atts[9];
                        obj.Width = atts[8];

                        if (pThisFeat.FeatType == (int)CWFeatType_e.CW_FEAT_TYPE_TURN)
                        {
                            CWTurnFeature pMillFeat = (CWTurnFeature)pThisFeat;
                            double MinDiameter;
                            double MaxDiameter = pMillFeat.GetMinMaxDiameter(out MinDiameter);
                            obj.MaxDiameter = MaxDiameter;
                            obj.MinDiameter = MinDiameter;
                            //int xxx = pMillFeat.GetTurnFeatureType();
                            //Console.Write("是否主轴：" + pturnFeat.GetSpindleType().ToString() + "\n");

                            //double sx = 0, sy = 0;
                            //double ex = 0, ey = 0;
                            //pturnFeat.GetStartPosition(out sx, out sy);

                            //Console.Write("sx:" + minDia + ",sy:" + maxDia + "\n");
                            //pturnFeat.GetEndPosition(out ex, out ey);


                            //switch (pturnFeat.GetTurnFeatureType())
                            //{
                            //    case (int)CWFeatureCatalog_e.CW_FEAT_SLOT_RECT:
                            //        ICWSlotRectFeat srf = (CWSlotRectFeat)pturnFeat;
                            //        Console.Write("根部直径：" + srf.IGetCornerRadius() + "\n");
                            //        Console.Write("宽度：" + srf.IGetWidth() + "\n");
                            //        Console.Write("深度：" + srf.IGetLength() + "\n");
                            //        break;
                            //    default:
                            //        break;
                            //}
                        }
                        else
                        {
                            Console.Write("其他特征:" + pThisFeat.FeatType + "\n");
                        }

                        swList.Add(obj);

                    }

                }

                return swList;
            }
            catch (Exception ep)
            {
                Console.Write(ep.Message);
                return null;
            }

        }
        /// <summary>
        /// 获取组中子特征
        /// </summary>
        private void GetSubFeatuer(ICWPatternFeature pPatternFeat, SwCAM_Mill obj)
        {
            try
            {
                List<SwCAM_Mill> subList = new List<SwCAM_Mill>();
                ICWDispatchCollection pDispCollection = pPatternFeat.IGetEnumChildFeatures();
                obj.SubFeatureCount = pDispCollection.Count;
                foreach (ICWFeature item in pDispCollection)
                {
                    double[] atts = item.GetFeatureAttributes();

                    SwCAM_Mill subobj = new SwCAM_Mill();
                    subobj.FeatureName = item.FeatureName;
                    subobj.FeatureType = item.FeatType;
                    subobj.Suppressed = item.Suppressed;

                    ICWMillFeature pMillFeatitem = (CWMillFeature)item;

                    List<Island> subislands = new List<Island>();
                    ICWDispatchCollection subislandCollection = pMillFeatitem.IGetIslands();
                    foreach (CWIslandInfo subitem in subislandCollection)
                    {
                        Island island = new Island();
                        island.Depth = subitem.GetDepth();
                        subislands.Add(island);
                    }
                    subobj.Islands = subislands;
                    subobj.IslandCount = pMillFeatitem.IGetIslandCount();
                    subobj.VolumeType = pMillFeatitem.VolumeType;

                    ///======= 获取特征属性
                    SetVolumeType(pMillFeatitem.VolumeType, atts, pMillFeatitem, subobj);

                    subobj.TDBFeatID = pMillFeatitem.GetTDBFeatID();
                    subobj.TopFilletRadius = pMillFeatitem.IGetTopFilletRadius();
                    subobj.Taper = pMillFeatitem.Taper;
                    subobj.TaperAngle = pMillFeatitem.IGetTaperAngle();
                    subobj.CornerRadius = pMillFeatitem.IGetCornerRadius();

                    double[] substPos = { 0, 0, 0 };
                    pMillFeatitem.GetStartPosition(out substPos[0], out substPos[1], out substPos[2]);

                    string strAttribute = ""; int nIdAttribute = 0;
                    pMillFeatitem.GetAttribute(out strAttribute, out nIdAttribute);
                    subobj.Tactics = strAttribute;
                    subobj.TacticsID = nIdAttribute;
                    subobj.TdbIdForAttribute = pMillFeatitem.GetTdbIdForAttribute(strAttribute);

                    double subDistance = 0;
                    // pMillFeatitem.GetDistanceUptoStock(out subDistance);
                    subobj.DistanceUptoStock = subDistance;

                    double[] substBound = { 0, 0, 0 };
                    pMillFeatitem.GetBoundParams(out substBound[0], out substBound[1], out substBound[2]);

                    subobj.ThroughOrblind = pMillFeatitem.SubType;
                    subobj.Bound = substBound;
                    subobj.StartPosition = substPos;
                    subobj.Depth = pMillFeatitem.Depth;
                    subList.Add(subobj);
                }
                obj.SubFeatureList = subList;
            }
            catch (Exception ep)
            {
                Console.Write(ep.Message);
            }
        }
        /// <summary>
        /// 获取铣削特征独有属性
        /// </summary>
        private void SetVolumeType(int VolumeType, double[] atts, ICWMillFeature pMillFeat, SwCAM_Mill obj)
        {
            switch (pMillFeat.VolumeType)
            {
                case (int)CWVolumeType_e.CW_HOLE_VOLUME://孔或者孔组
                    if (pMillFeat is CWMSHoleFeat)
                    {
                        CWMSHoleFeat sf = (CWMSHoleFeat)pMillFeat;
                        obj.Sharp = sf.GetTipAngle();//尖角
                    }
                    obj.VolumeTypeString = "CW_HOLE_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_POCKET_VOLUME://不规则凹腔 、圆形凹腔、腰形凹腔、矩形凹腔
                    obj.VolumeTypeString = "CW_POCKET_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_BOSS_VOLUME://圆形凸台
                    obj.VolumeTypeString = "CW_BOSS_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_SLOT_VOLUME://不规则槽、矩形槽
                    obj.VolumeTypeString = "CW_SLOT_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_SLAB_VOLUME: //面特征
                    obj.VolumeTypeString = "CW_SLAB_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_OPENPOCKET_VOLUME: //开放式凹腔 、周界-非封闭凹腔
                    obj.VolumeTypeString = "CW_OPENPOCKET_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_WORKPIECE_VOLUME:
                    obj.VolumeTypeString = "CW_WORKPIECE_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_3AXIS_VOLUME:
                    obj.VolumeTypeString = "CW_3AXIS_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_HOLECTRSUNK_VOLUME: //埋头孔
                    if (pMillFeat is CWMSHoleCtrSunkFeat)
                    {
                        CWMSHoleCtrSunkFeat scs = (CWMSHoleCtrSunkFeat)pMillFeat;
                        obj.SinkAngle = scs.SinkAngle;
                        obj.SinkDiameter = scs.SinkDiameter;
                        obj.Sharp = scs.GetTipAngle();
                    }
                    obj.VolumeTypeString = "CW_HOLECTRSUNK_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_HOLECTRBORE_VOLUME: //沉镗孔
                    if (pMillFeat is ICWMSHoleCtrBoreFeat)
                    {
                        ICWMSHoleCtrBoreFeat sf = (ICWMSHoleCtrBoreFeat)pMillFeat;
                        obj.Sharp = sf.GetTipAngle();
                    }
                    obj.BoreDiameter = atts[11];
                    obj.BoreDepth = atts[13];
                    obj.VolumeTypeString = "CW_HOLECTRBORE_VOLUME";
                    break;
                case (int)CWVolumeType_e.CW_MULTISTEP_VOLUME:   //MS孔  (多阶)
                    obj.VolumeTypeString = "CW_MULTISTEP_VOLUME";
                    //判断是否为多阶类型，如果为组的话 会变成 CWPatternFeature 类型需要在子类中获取多阶属性   例：“MS孔组” 
                    #region 获取多阶
                    if (pMillFeat is CWMultiStepFeature)
                    {
                        CWMultiStepFeature sf = (CWMultiStepFeature)pMillFeat;
                        // double dd = sf.GetDepth();
                        // sf.GetStartPoint();
                        int steps = sf.GetNumOfSteps();

                        obj.SubMultiStep = new List<SwMultiStep>();
                        for (int si = 0; si < steps; si++)
                        {
                            double minvalue = 0;
                            double maxvalue = 0;
                            double oddept = 0;
                            double Conedepth = 0;
                            object ppstart = null;
                            object centor = null;
                            bool apex;
                            double angle;

                            SwMultiStep sms = new SwMultiStep();//多阶类

                            switch (sf.GetTypeAtStep(si))
                            {
                                case (int)CWMultiStepType.CW_MULTISTEP_TORUS_STEP: //圆弧过度（环形）
                                    sf.GetTorusParams(si, out minvalue, out maxvalue, out oddept, out ppstart, out centor);
                                    sms.MultiSetpType = (int)CWMultiStepType.CW_MULTISTEP_TORUS_STEP;
                                    sms.MultiSetpName = CWMultiStepType.CW_MULTISTEP_TORUS_STEP.ToString();
                                    sms.Diameter = minvalue;
                                    sms.Depth = oddept;
                                    sms.Distance = maxvalue / 2; //找SW的特征参数 就发现 距离参数 这里的数值 X2了！所以要除掉 （不清楚是否CAM的BUG）
                                    break;
                                case (int)CWMultiStepType.CW_MULTISTEP_CONE_STEP: //倒角（圆锥）
                                    sf.GetConeParams(si, out minvalue, out maxvalue, out Conedepth, out oddept, out ppstart, out apex, out angle);
                                    sms.MultiSetpType = (int)CWMultiStepType.CW_MULTISTEP_CONE_STEP;
                                    sms.MultiSetpName = CWMultiStepType.CW_MULTISTEP_CONE_STEP.ToString();
                                    sms.TopDiameter = maxvalue;
                                    sms.MinorDiameter = minvalue;
                                    sms.Depth = oddept;
                                    sms.Angle = angle;
                                    break;
                                case (int)CWMultiStepType.CW_MULTISTEP_CYLN_STEP: //圆柱
                                    sf.GetCylParams(si, out minvalue, out oddept, out ppstart);
                                    sms.MultiSetpType = (int)CWMultiStepType.CW_MULTISTEP_CYLN_STEP;
                                    sms.MultiSetpName = CWMultiStepType.CW_MULTISTEP_CYLN_STEP.ToString();
                                    sms.Diameter = minvalue;
                                    sms.Depth = oddept;
                                    break;
                            }
                            obj.SubMultiStep.Add(sms);//追加 多阶 MS孔
                        }
                    }
                    if (pMillFeat is ICWPatternFeature)
                    {
                        CWPatternFeature pf = (CWPatternFeature)pMillFeat;
                        ICWDispatchCollection dc = (ICWDispatchCollection) pf.IGetEnumChildFeatures();
                        for (int i = 0; i < dc.Count; i++)
                        {
                            ICWMultiStepFeature sf = dc.Item(i);
                            int steps = sf.GetNumOfSteps();

                            obj.SubMultiStep = new List<SwMultiStep>();
                            for (int si = 0; si < steps; si++)
                            {
                                double minvalue = 0;
                                double maxvalue = 0;
                                double oddept = 0;
                                double Conedepth = 0;
                                object ppstart = null;
                                object centor = null;
                                bool apex;
                                double angle;

                                SwMultiStep sms = new SwMultiStep();//多阶类

                                switch (sf.GetTypeAtStep(si))
                                {
                                    case (int)CWMultiStepType.CW_MULTISTEP_TORUS_STEP: //圆弧过度（环形）
                                        sf.GetTorusParams(si, out minvalue, out maxvalue, out oddept, out ppstart, out centor);
                                        sms.MultiSetpType = (int)CWMultiStepType.CW_MULTISTEP_TORUS_STEP;
                                        sms.MultiSetpName = CWMultiStepType.CW_MULTISTEP_TORUS_STEP.ToString();
                                        sms.Diameter = minvalue;
                                        sms.Depth = oddept;
                                        sms.Distance = maxvalue / 2; //找SW的特征参数 就发现 距离参数 这里的数值 X2了！所以要除掉 （不清楚是否CAM的BUG）
                                        break;
                                    case (int)CWMultiStepType.CW_MULTISTEP_CONE_STEP: //倒角（圆锥）
                                        sf.GetConeParams(si, out minvalue, out maxvalue, out Conedepth, out oddept, out ppstart, out apex, out angle);
                                        sms.MultiSetpType = (int)CWMultiStepType.CW_MULTISTEP_CONE_STEP;
                                        sms.MultiSetpName = CWMultiStepType.CW_MULTISTEP_CONE_STEP.ToString();
                                        sms.TopDiameter = maxvalue;
                                        sms.MinorDiameter = minvalue;
                                        sms.Depth = oddept;
                                        sms.Angle = angle;
                                        break;
                                    case (int)CWMultiStepType.CW_MULTISTEP_CYLN_STEP: //圆柱
                                        sf.GetCylParams(si, out minvalue, out oddept, out ppstart);
                                        sms.MultiSetpType = (int)CWMultiStepType.CW_MULTISTEP_CYLN_STEP;
                                        sms.MultiSetpName = CWMultiStepType.CW_MULTISTEP_CYLN_STEP.ToString();
                                        sms.Diameter = minvalue;
                                        sms.Depth = oddept;
                                        break;
                                }
                                obj.SubMultiStep.Add(sms);//追加 多阶 MS孔
                            }
                        }

                    }
                    #endregion
                    obj.Maxdiameter = atts[1];
                    break;
                case (int)CWVolumeType_e.CW_MULTIFACE_VOLUME:
                    // CWMultiSurfaceFeat sff = (CWMultiSurfaceFeat)pMillFeat;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 获取工序明细（获取时间以及刀具轨迹长度）
        /// </summary>
        /// <returns></returns>
        public List<ProcessDetail> GetProcessDetails()
        {
            List<ProcessDetail> list = new List<ProcessDetail>();

            CWApp cwApp = new CWApp();
            CWPartDoc cwPd = (CWPartDoc)cwApp.IGetActiveDoc();

            CWDoc cwDoc = (CWDoc)cwApp.IGetActiveDoc();
            cwApp.ActiveDocEMF();//提取特征
            cwApp.ActiveDocGOP(1);// （暂时不理解,但是不调用 生成操作计划会有问题）
            cwApp.ActiveDocGTP();//生成操作计划 + 生成道具轨迹
            CWPartDoc cwPartDoc = (CWPartDoc)cwDoc;
            CWMachine cwMach = (CWMachine)cwPartDoc.IGetMachine();
            CWDispatchCollection cwDispCol = (CWDispatchCollection)cwMach.IGetEnumSetups();

            for (int i = 0; i < cwDispCol.Count; i++)// 铣削零件设置组 
            {
                CWBaseSetup cwBaseSetup = (CWBaseSetup)cwDispCol.Item(i);

                if (cwBaseSetup == null)
                    continue;

                CWDispatchCollection geo = cwBaseSetup.IGetEnumOperations();
                for (int j = 0; j < geo.Count; j++)
                {
                    CWOperation operation = geo.Item(j);

                    ProcessDetail pd = new ProcessDetail();
                    pd.OperationName = operation.OperationName;
                    pd.ToolpathTotalTime = operation.ToolpathTotalTime;// 时间
                    pd.ToolpathTotalLength = operation.ToolpathTotalLength;//刀具总长度

                    CWDispatchCollection idc = (CWDispatchCollection)operation.IGetAllFeatures();
                    for (int k = 0; k < idc.Count; k++)
                    {
                        ICWFeature pThisFeat = idc.Item(k);
                        pd.FeatureName = pThisFeat.FeatureName;
                    }

                    list.Add(pd);
                }

            }
            return list;
        }
        #endregion

        #region ===== 特征时间计算 =====
        /// <summary>
        /// [孔]*
        /// </summary>
        private void GetFeature_Hole(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            //实现过程
            
             Axis3_Drilling p = new Axis3_Drilling(swCam.Maxdiameter, swCam.Depth, 1, GetMaterials());
             af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);
            af.TotalTime = af.TotalTime * 5;// 增加点孔的时间！！ 翻倍~
            double MachineMoney = GetMachineMoney();
              af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// [埋头孔]*
        /// </summary>
        private void GetFeature_HoleCtrsunk(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程
           
                Axis3_Drilling p = new Axis3_Drilling(swCam.Maxdiameter, swCam.Depth, 1, GetMaterials());
                af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);
                af.TotalTime = af.TotalTime * 3.5; // 因为埋头孔有 沉头可以把此部分的时间 增加50% 来处理  在增加点孔时间 共 3.5倍 （方案：Kevin.yang） 
                double MachineMoney = GetMachineMoney();
                af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// [沉镗孔]*
        /// </summary>
        private void GetFeature_HoleCtrbore(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程

                //镗孔 第一阶段 底孔
                Axis3_Drilling p = new Axis3_Drilling(swCam.Maxdiameter, swCam.Depth,1, GetMaterials());
                af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);
               af.TotalTime = af.TotalTime * 5;// 增加点孔的时间！！ 翻倍~

            //镗孔 第二阶段 镗孔
            Axis3_Drilling p2 = new Axis3_Drilling(swCam.Maxdiameter, swCam.BoreDepth, 1, GetMaterials());
                af.TotalTime += p2.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);
                af.TotalTime = af.TotalTime * 1.8;

            double MachineMoney = GetMachineMoney();
                af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

                TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// [MS孔]*
        /// </summary>
        private void GetFeature_MSHole(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            //实现过程

            af.TotalTime = 0;
            double minTime = 999999;//获取最短时间 来计算 点孔
            foreach (SwMultiStep item in swCam.SubMultiStep)
            {
                if (item.MultiSetpType == 1)//圆柱
                {
                    Axis3_Drilling p = new Axis3_Drilling(item.Diameter, item.Depth,1, GetMaterials());
                    af.TotalTime += p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);
                    if (p.TotalTime < minTime)
                        minTime = p.TotalTime;
                }
                else if (item.MultiSetpType == 2)//倒角
                {
                    Axis3_ChamferMilling p = new Axis3_ChamferMilling(Cutter_Drill.chamfer, item.TopDiameter * 3.14, item.Depth, 1,GetMaterials());
                    af.TotalTime += p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);
                    if (p.TotalTime < minTime)
                        minTime = p.TotalTime;
                }
            }
            if (minTime == 999999)
                minTime = 0;

            af.TotalTime += af.TotalTime + minTime / 2;//增加点孔时间。。 用时最短的工序/2

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 矩形槽*
        /// </summary>
        private void GetFeature_RectangleGroove(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程

            double[] bound = ConvertLWH(swCam.Bound,swCam.Depth);//因为有坐标系所以 要自动按大小 分配 长 宽 高

            double CutterTool = Cutter_Drill.GetPoked(bound[0], bound[1]);//刀具
            //槽铣
            Axis3_PocketMilling p = new Axis3_PocketMilling(CutterTool, bound[0], bound[1],bound[2], 1, GetMaterials());
            af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            int proCount = NumberOfWalkCut(bound[2], CutterTool) + 1;
            af.TotalTime = af.TotalTime * proCount;// 根据刀具与深度 判断要切割几次

            af.Test_SingleTime = Math.Round(p.TotalTime,0);
            af.Test_ProcessCount = proCount;
            af.Test_Dia = CutterTool;


            //精铣   EdgeRadius 棱角半径暂且给定 刀具半径
            // Axis3_SurfaceFinishMilling fm = new Axis3_SurfaceFinishMilling(Cutter_Drill.GetFinish(CutterTool), Cutter_Drill.GetFinish(CutterTool) / 2, (bound[0] * bound[1]), 1.6, 1, GetMaterials());
            // af.TotalTime += fm.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 腰形槽
        /// </summary>
        private void GetFeature_KidneySlot(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程
            double[] bound = ConvertLWH(swCam.Bound, swCam.Depth);//因为有坐标系所以 要自动按大小 分配 长 宽 高

            double CutterTool = Cutter_Drill.GetPoked(bound[0], bound[1]);//刀具
             

            Axis3_OpenSlotMilling p = new Axis3_OpenSlotMilling(CutterTool, bound[0], bound[1], bound[2], 1, GetMaterials());
            af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            int proCount = NumberOfWalkCut(bound[2], CutterTool) + 1;
            af.TotalTime = af.TotalTime * proCount;// 根据刀具与深度 判断要切割几次

            af.Test_SingleTime = Math.Round(p.TotalTime, 0);
            af.Test_ProcessCount = proCount;
            af.Test_Dia = CutterTool;

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额
            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 不规则槽*
        /// </summary>
        private void GetFeature_AnomalyGroove(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程

            double[] bound = ConvertLWH(swCam.Bound, swCam.Depth);//因为有坐标系所以 要自动按大小 分配 长 宽 高

            double CutterTool = Cutter_Drill.GetPoked(bound[0], bound[1]);//刀具

            //槽铣
            Axis3_PocketMilling p = new Axis3_PocketMilling(CutterTool, bound[0], bound[1], bound[2], 1, GetMaterials()); 
            af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            int proCount = NumberOfWalkCut(bound[2], CutterTool) + 1;
            af.TotalTime = af.TotalTime * proCount;// 根据刀具与深度 判断要切割几次

            af.Test_SingleTime = Math.Round(p.TotalTime, 0);
            af.Test_ProcessCount = proCount;
            af.Test_Dia = CutterTool;

            //精铣   EdgeRadius 棱角半径暂且给定 刀具半径
            //Axis3_SurfaceFinishMilling fm = new Axis3_SurfaceFinishMilling(Cutter_Drill.GetFinish(CutterTool), Cutter_Drill.GetFinish(CutterTool) / 2, (bound[0] * bound[1]), 1.6, 1, GetMaterials());
            //af.TotalTime += fm.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 矩形凹腔
        /// </summary>
        private void GetFeature_RectangleCavity(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程

            double[] bound = ConvertLWH(swCam.Bound, swCam.Depth);//因为有坐标系所以 要自动按大小 分配 长 宽 高

            double CutterTool = Cutter_Drill.GetPoked(bound[0], bound[1]);//刀具
            //槽铣（依旧用矩形槽的方式做矩形凹腔）
            Axis3_PocketMilling p = new Axis3_PocketMilling(CutterTool, bound[0], bound[1], bound[2], 1, GetMaterials());
            af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            int proCount = NumberOfWalkCut(bound[2], CutterTool) + 1;
            af.TotalTime = af.TotalTime * proCount;// 根据刀具与深度 判断要切割几次

            af.Test_SingleTime = Math.Round(p.TotalTime, 0);
            af.Test_ProcessCount = proCount;
            af.Test_Dia = CutterTool;

            //精铣   EdgeRadius 棱角半径暂且给定 刀具半径
            //Axis3_SurfaceFinishMilling fm = new Axis3_SurfaceFinishMilling(Cutter_Drill.GetFinish(CutterTool), Cutter_Drill.GetFinish(CutterTool) / 2, (bound[0] * bound[1] * bound[2]), 1.6, 1, GetMaterials());
            //af.TotalTime += fm.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 圆形凹腔
        /// </summary>
        private void GetFeature_CirclePock(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程

            //使用腰形槽的方式，长宽 使用 直径来代替
            Axis3_OpenSlotMilling p = new Axis3_OpenSlotMilling(Cutter_Drill.GetCirclePock(swCam.Maxdiameter), swCam.Maxdiameter, swCam.Maxdiameter, swCam.Depth, 1, GetMaterials());
            af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            int proCount = NumberOfWalkCut(swCam.Depth, Cutter_Drill.GetCirclePock(swCam.Maxdiameter)) + 1;
            af.TotalTime = af.TotalTime * proCount;// 根据刀具与深度 判断要切割几次

            af.Test_SingleTime = Math.Round(p.TotalTime, 0);
            af.Test_ProcessCount = proCount;
            af.Test_Dia = Cutter_Drill.GetCirclePock(swCam.Maxdiameter);

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额
            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 腰形凹腔*
        /// </summary>
        private void GetFeature_KidneyPock(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程
            double[] bound = ConvertLWH(swCam.Bound, swCam.Depth);//因为有坐标系所以 要自动按大小 分配 长 宽 高
            double CutterTool = Cutter_Drill.GetPoked(bound[0], bound[1]);//刀具

            Axis3_ClosedSlotMilling p = new Axis3_ClosedSlotMilling(CutterTool, bound[0], bound[1], bound[2], 1, GetMaterials());
            af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            int proCount = NumberOfWalkCut(bound[2], CutterTool) + 1;
            af.TotalTime = af.TotalTime * proCount;// 根据刀具与深度 判断要切割几次

            af.Test_SingleTime = Math.Round(p.TotalTime, 0);
            af.Test_ProcessCount = proCount;
            af.Test_Dia = CutterTool;

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额
            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 不规则凹腔
        /// </summary>
        private void GetFeature_AnomalyCavity(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程
            double[] bound = ConvertLWH(swCam.Bound, swCam.Depth);//因为有坐标系所以 要自动按大小 分配 长 宽 高
            double CutterTool = Cutter_Drill.GetPoked(bound[0], bound[1]);//刀具

            //槽铣
            Axis3_PocketMilling p = new Axis3_PocketMilling(CutterTool, bound[0], bound[1], bound[2], 1, GetMaterials());
            af.TotalTime = p.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

            int proCount = NumberOfWalkCut(bound[2], CutterTool) + 1;
            af.TotalTime = af.TotalTime * proCount;// 根据刀具与深度 判断要切割几次

            af.Test_SingleTime = Math.Round(p.TotalTime, 0);
            af.Test_ProcessCount = proCount;
            af.Test_Dia = CutterTool;

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 开放式凹腔
        /// </summary>
        /// <param name="swCam"></param>
        private void GetFeature_OpenCavity(SwCAM_Mill swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程
            double[] bound = ConvertLWH(swCam.Bound, swCam.Depth);//因为有坐标系所以 要自动按大小 分配 长 宽 高
            double CutterTool = Cutter_Drill.GetPoked(bound[0], bound[1]);//刀具

            //面铣
            Axis3_FaceMilling p = new Axis3_FaceMilling(CutterTool, bound[0], bound[1], bound[2], 1, GetMaterials());
            af.TotalTime = p.TotalTime;

            int proCount = NumberOfWalkCut(bound[2], CutterTool) + 1;
            af.TotalTime = af.TotalTime * proCount;// 根据刀具与深度 判断要切割几次

            double isLandTime = 0;
            if (swCam.IslandCount > 0)//如果有岛屿
            {
                double[] lengthWidth = GetIsLandArea(bound[0], bound[1], swCam.IslandCount);//获取每个岛屿的 预估尺寸 长、宽

                //槽铣(替代精铣)
                Axis3_PocketMilling pm = new Axis3_PocketMilling(Cutter_Drill.GetFinish(CutterTool), lengthWidth[0], lengthWidth[1], bound[2], 1, GetMaterials());
                isLandTime = pm.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);//防止出现 2个 开放式凹腔 出现组的情况。 正常下 只会是1
                isLandTime *= swCam.IslandCount;

                af.Test_IsLandCount = swCam.IslandCount;
                af.Test_IsLandSize = new double[] { lengthWidth[0], lengthWidth[1], bound[2] };
                af.Test_IsLandTime = Math.Round(isLandTime, 0);
            }
            af.TotalTime += isLandTime;

            af.Test_SingleTime = Math.Round(p.TotalTime, 0);
            af.Test_ProcessCount = proCount;
            af.Test_Dia = CutterTool; 

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 获取装夹时间*
        /// </summary>
        public void GetSetFixture()
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = "装夹时间";
            af._SwCAM = null;

            double MaxLenght = StockSize[0];//最大长度

            if (MaxLenght <= 300) //当最大长度 <300mm  每个面  * 30秒
                af.TotalTime = 30 * SetCount;
            else if (MaxLenght > 300 && MaxLenght <= 500)
                af.TotalTime = 60 * SetCount;
            else if (MaxLenght > 500 && MaxLenght <= 1000)
                af.TotalTime = 180 * SetCount;
            else
                af.TotalTime = 600 * SetCount;

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 面部粗铣*
        /// </summary>
        public void GetFaceMill()
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = "面部粗铣";
            af._SwCAM = null;

            Axis3_FaceMilling p = new Axis3_FaceMilling(50, StockSize[0], StockSize[1],1, 1, GetMaterials());
            af.TotalTime = p.TotalTime;

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 轮廓粗铣*
        /// </summary>
        /// <returns></returns>
        public void GetProfileMill()
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = "轮廓粗铣";
            af._SwCAM = null;

            Axis3_ProfileMilling p = new Axis3_ProfileMilling(16, StockSize[0], StockSize[1], StockSize[2], 1, GetMaterials());
            af.TotalTime = p.TotalTime;

            double MachineMoney = GetMachineMoney();
            af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额

            TotalFeatureMoney.Add(af);
        }
        #endregion

        #region ===== 获取特征与刀具轨迹合二为一 =====
        /// <summary>
        /// 获取铣削所有特征 and 刀具轨迹时间明细 (PS：把两部分合为一步是节省了一次提取特征的时间)
        /// </summary>
        /// <returns></returns>
        public MergeFeatrueDetail GetFeatuerMill_And_Process()
        {
            try
            {
                List<SwCAM_Mill> swList = new List<SwCAM_Mill>();//获取全部特征
                List<ProcessDetail> list = new List<ProcessDetail>();//刀具轨迹明细

                CWApp cwApp = new CWApp(); 
                CWDoc cwDoc = (CWDoc)cwApp.IGetActiveDoc();
                  
                cwApp.ActiveDocEMF();//提取特征 
                cwApp.ActiveDocGOP(1);// （暂时不理解,但是不调用 生成操作计划会有问题）
                cwApp.ActiveDocGTP();//生成操作计划 + 生成道具轨迹
                CWPartDoc cwPartDoc = (CWPartDoc)cwDoc;
                CWMachine cwMach = (CWMachine)cwPartDoc.IGetMachine();
                CWDispatchCollection cwDispCol = (CWDispatchCollection)cwMach.IGetEnumSetups();

                ICWDoc3 a1 = (ICWDoc3)cwApp.IGetActiveDoc();
                double d1, d2, d3, d4, d5, d6;
                a1.GetBoxRecordParams(out d1, out d2, out d3, out d4, out d5, out d6);
                StockSize = ConvertLWH(new double[] { d4, d5, d6 }, -1000);//获取毛坯尺寸

                if (cwDispCol.Count == 0)
                {
                    Console.Write("该图纸无法提取特征！");
                    return null;
                }

                for (int i = 0; i < cwDispCol.Count; i++)// 铣削零件设置组  如果count=0 则代表提取特征失败
                {
                    CWBaseSetup cwBaseSetup = (CWBaseSetup)cwDispCol.Item(i);//获取 铣削零件设置 

                    if (cwBaseSetup == null)
                        continue;

                    #region 处理刀具轨迹
                    CWDispatchCollection geo = cwBaseSetup.IGetEnumOperations();
                    for (int j = 0; j < geo.Count; j++)
                    {
                        CWOperation operation = geo.Item(j);

                       // ICWPartPerimeterFeat pf = geo.Item(j); 毛坯的接口。但是找不到创建实例的方法。。。

                        ProcessDetail pd = new ProcessDetail();
                        pd.OperationName = operation.OperationName;
                        pd.ToolpathTotalTime = operation.ToolpathTotalTime;// 时间
                        pd.ToolpathTotalLength = operation.ToolpathTotalLength;//刀具总长度

                        CWDispatchCollection idc = (CWDispatchCollection)operation.IGetAllFeatures();
                        for (int k = 0; k < idc.Count; k++)
                        {
                            ICWFeature pThisFeat = idc.Item(k);
                            pd.FeatureName = pThisFeat.FeatureName;
                        }

                        list.Add(pd);
                    }
                    #endregion

                    ICWDispatchCollection FeatList = (ICWDispatchCollection)cwBaseSetup.IGetEnumFeatures();//方法获取程序上的所有特性

                    for (int j = 0; j < FeatList.Count; j++)
                    {
                        ICWFeature pThisFeat = FeatList.Item(j); 

                        SwCAM_Mill obj = new SwCAM_Mill();
                        obj.FeatureName = pThisFeat.FeatureName;
                        obj.FeatureType = pThisFeat.FeatType;
                        obj.Axis = pThisFeat.Axis;
                        obj.Suppressed = pThisFeat.Suppressed;

                        double[] atts = pThisFeat.GetFeatureAttributes();//获得该特征的参数属性

                        if (pThisFeat.FeatType == (int)CWFeatType_e.CW_FEAT_TYPE_MILL)
                        {
                            double[] stPos = { 0, 0, 0 };
                            ICWMillFeature pMillFeat = (CWMillFeature)pThisFeat;
                            pMillFeat.GetStartPosition(out stPos[0], out stPos[1], out stPos[2]);
                            obj.StartPosition = stPos;
                            obj.Shape = pMillFeat.Shape; //CWShpeType_e 类型
                            obj.Depth = pMillFeat.Depth;
                            string strAttribute = ""; int nIdAttribute = 0;
                            pMillFeat.GetAttribute(out strAttribute, out nIdAttribute);
                            obj.Tactics = strAttribute;
                            obj.TacticsID = nIdAttribute;
                            obj.TdbIdForAttribute = pMillFeat.GetTdbIdForAttribute(strAttribute);
                            obj.TopFilletRadius = pMillFeat.IGetTopFilletRadius();
                            obj.Chamfer = atts[16];//倒角
                            obj.Maxdiameter = atts[1];//直径
                            obj.BottomRadius = atts[20];//底部半径
                            obj.Mindiameter = atts[9];//精加工半径 
                            obj.Bound = new double[] { atts[7], atts[8], atts[9] };//长、宽、高

                            //double[] stBound = { 0, 0, 0 };
                            //pMillFeat.GetBoundParams(out stBound[0], out stBound[1], out stBound[2]);
                            //obj.Bound = stBound;

                            if (pMillFeat.VolumeType == 4)
                            {

                                //ICWTaperTool ssd = (ICWTaperTool)cwMach.IGetToolcrib();
                                //ICWDispatchCollection ddd = (ICWDispatchCollection)cwMach.IGetToolcrib();
                                //for (int u = 0; u < ddd.Count; u++)
                                //{
                                //    object fsfs = ddd.Item(u);

                                //}

                                //ICWOperation op = (ICWOperation)pThisFeat.GetOperationList();
                                //ICWTool col = (ICWTool)op.IGetTool();
                                //ICWTaperTool tap = (ICWTaperTool)op.IGetTool();


                            }

                            double Distance = 0;
                            //pMillFeat.GetDistanceUptoStock(out Distance);
                            obj.DistanceUptoStock = Distance;

                            obj.VolumeType = pMillFeat.VolumeType;

                            ///======= 获取特征独有属性
                            SetVolumeType(pMillFeat.VolumeType, atts, pMillFeat, obj);

                            List<Island> islands = new List<Island>();//获取岛屿
                            ICWDispatchCollection islandCollection = pMillFeat.IGetIslands();
                            object jjjj = pMillFeat.IGetIslands();
                            if (pThisFeat.FeatureName == "开放式凹腔1")
                            {
                                int landCount = pMillFeat.IGetIslandCount();
                                object objjjj = pMillFeat.IGetIslands();

                                foreach (CWIslandInfo item in islandCollection)
                                {
                                    Island island = new Island();
                                    island.Depth = item.GetDepth();
                                    islands.Add(island);
                                }

                                for (int m = 0; m < islandCollection.Count; m++)
                                {
                                    object cc = islandCollection.Item(m);
                                }

                            }


                            foreach (CWIslandInfo item in islandCollection)
                            {
                                Island island = new Island();
                                island.Depth = item.GetDepth();
                                islands.Add(island);
                            }

                            obj.Islands = islands;
                            obj.IslandCount = pMillFeat.IGetIslandCount();
                            obj.ThroughOrblind = pMillFeat.SubType;
                            obj.CornerRadius = pMillFeat.IGetCornerRadius();
                            obj.TDBFeatID = pMillFeat.GetTDBFeatID();
                            obj.Taper = pMillFeat.Taper;

                            if (obj.Taper)
                            {

                                //TaperInfo ti = new TaperInfo();
                                //ti.TaperAngle = pMillFeat.IGetTaperAngle();
                                ////ti.TaperHeight = obj.BottomRadius == 0 ? obj.Depth : (obj.Depth - obj.BottomRadius - obj.Mindiameter);//底部半径==0？深度：（深度-底部半径-精加工半径）
                                //ti.BottomArcFaceRadius = obj.BottomRadius;
                                ////ti.BottomArcFaceHeight = obj.BottomRadius == 0 ? 0 : (obj.Depth - ti.TaperHeight - obj.Mindiameter);//（深度-锥度高-精加工半径）= 底部高度

                            }



                            if (pMillFeat.IsPatternFeature()) //获取组信息， 子列表
                            {
                                //获取子特征
                                GetSubFeatuer((ICWPatternFeature)pMillFeat, obj);
                            }
                        }
                        #region 车削
                        else if (pThisFeat.FeatType == (int)CWFeatType_e.CW_FEAT_TYPE_TURN)
                        {
                            Console.Write("车削特征:" + pThisFeat.FeatureName + "\n");
                            ICWTurnFeature pturnFeat = (CWTurnFeature)pThisFeat;

                            Console.Write(pturnFeat.GetAttributeName() + "\n");

                            double minDia = 0;
                            double maxDia = pturnFeat.GetMinMaxDiameter(out minDia);

                            Console.Write("minDia:" + minDia + ",maxDia:" + maxDia + "\n");
                            Console.Write("FeatureType:" + pturnFeat.GetTurnFeatureType() + "\n");
                            Console.Write("是否主轴：" + pturnFeat.GetSpindleType().ToString() + "\n");
                            Console.Write("类型" + pturnFeat.GetTurnFeatureType() + "\n");

                            double sx = 0, sy = 0;
                            double ex = 0, ey = 0;
                            pturnFeat.GetStartPosition(out sx, out sy);

                            Console.Write("sx:" + minDia + ",sy:" + maxDia + "\n");
                            pturnFeat.GetEndPosition(out ex, out ey);
                            Console.Write("ex:" + minDia + ",ey:" + maxDia + "\n");
                            Console.Write("面长：" + atts[7] + "\n");

                            switch (pturnFeat.GetTurnFeatureType())
                            {
                                case (int)CWFeatureCatalog_e.CW_FEAT_SLOT_RECT:
                                    ICWSlotRectFeat srf = (CWSlotRectFeat)pturnFeat;
                                    Console.Write("根部直径：" + srf.IGetCornerRadius() + "\n");
                                    Console.Write("宽度：" + srf.IGetWidth() + "\n");
                                    Console.Write("深度：" + srf.IGetLength() + "\n");
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            Console.Write("其他特征:" + pThisFeat.FeatType + "\n");
                        }
                        #endregion
                        swList.Add(obj);

                    }

                }

                MergeFeatrueDetail mfd = new MergeFeatrueDetail();
                mfd.swCAM_Mill = swList;
                mfd.processDetail = list;

                return mfd;
            }
            catch (Exception ep)
            {
                Console.Write(ep.Message);
                return null;
            }

        }
        /// <summary>
        /// 计算铣削总特征(包含 刀具轨迹参数)
        /// </summary>
        /// <param name="swCAM"></param>
        public void ComputeFeature_Mill_Process(MergeFeatrueDetail mfd)
        {
            List<SwCAM_Mill> swCAMs = mfd.swCAM_Mill;
            foreach (SwCAM_Mill item in swCAMs)
            {
                switch (item.VolumeType)
                {
                    case (int)CWVolumeType_e.CW_HOLE_VOLUME://孔或者孔组
                        GetFeature_Hole(item);
                        break;
                    case (int)CWVolumeType_e.CW_HOLECTRSUNK_VOLUME: //埋头孔
                        GetFeature_HoleCtrsunk(item);
                        break;
                    case (int)CWVolumeType_e.CW_HOLECTRBORE_VOLUME: //沉镗孔
                        GetFeature_HoleCtrbore(item);
                        break;
                    case (int)CWVolumeType_e.CW_MULTISTEP_VOLUME:   //MS孔  (多阶)
                        GetFeature_MSHole(item);
                        return;
                    case (int)CWVolumeType_e.CW_POCKET_VOLUME://不规则凹腔 、圆形凹腔、腰形凹腔、矩形凹腔
                        if (item.FeatureName.IndexOf("矩形") >= 0)
                            GetFeature_RectangleGroove(item);
                        else if (item.FeatureName.IndexOf("腰形") >= 0)
                            GetFeature_KidneyPock(item);
                        else if (item.FeatureName.IndexOf("圆形凹腔") >= 0)
                            GetFeature_CirclePock(item);
                        else
                            GetFeature_AnomalyCavity(item);//不规则凹腔
                        break;
                    case (int)CWVolumeType_e.CW_SLOT_VOLUME://不规则槽、矩形槽、腰形槽
                        if (item.FeatureName.IndexOf("矩形") >= 0)
                            GetFeature_RectangleGroove(item);
                        else if (item.FeatureName.IndexOf("腰形") >= 0)
                            GetFeature_KidneySlot(item);
                        else
                            GetFeature_AnomalyGroove(item);//不规则槽
                        break;
                    case (int)CWVolumeType_e.CW_OPENPOCKET_VOLUME: //开放式凹腔 、周界-非封闭凹腔
                          GetFeature_OpenCavity(item, mfd.processDetail);
                        break;
                    case (int)CWVolumeType_e.CW_BOSS_VOLUME://圆形凸台 
                        break;
                    case (int)CWVolumeType_e.CW_SLAB_VOLUME: //面特征
                        break;
                    case (int)CWVolumeType_e.CW_MULTIFACE_VOLUME:
                        break;
                    case (int)CWVolumeType_e.CW_WORKPIECE_VOLUME:
                        break;
                    case (int)CWVolumeType_e.CW_3AXIS_VOLUME:
                        break;
                    default:
                        break;

                }
            }

        }
        /// <summary>
        /// 获取[开放式凹腔、周界非封闭凹腔]计算金额
        /// </summary>
        private void GetFeature_OpenCavity(SwCAM_Mill swCam, List<ProcessDetail> pDetail)
        { 
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            //实现过程

            foreach (ProcessDetail item in pDetail)
            {
                if (item.FeatureName == af.FeatureName)
                {
                    af.TotalTime += item.ToolpathTotalTime * 60;
                }
            }

            TotalFeatureMoney.Add(af);
        }
        #endregion

    }
}
