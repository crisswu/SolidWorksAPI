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

        public List<FeatureAmount> TotalFeatureMoney = new List<FeatureAmount>();//全部 特征金额

        public CAM_Feature()
        {
          
        }

        #region 获取特征相关方法
        /// <summary>
        /// 获取所有特征
        /// </summary>
        /// <returns></returns>
        public List<SwCAM> GetFeatuer()
        {
            try
            {
                List<SwCAM> swList = new List<SwCAM>();//获取全部特征

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

                        SwCAM obj = new SwCAM();
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
        /// 获取组中子特征
        /// </summary>
        private void GetSubFeatuer(ICWPatternFeature pPatternFeat, SwCAM obj)
        {
            try
            {
                List<SwCAM> subList = new List<SwCAM>();
                ICWDispatchCollection pDispCollection = pPatternFeat.IGetEnumChildFeatures();
                obj.SubFeatureCount = pDispCollection.Count;
                foreach (ICWFeature item in pDispCollection)
                {
                    double[] atts = item.GetFeatureAttributes();

                    SwCAM subobj = new SwCAM();
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
        /// 获取特征独有属性
        /// </summary>
        private void SetVolumeType(int VolumeType, double[] atts, ICWMillFeature pMillFeat, SwCAM obj)
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
        #endregion

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
        /// 计算总特征
        /// </summary>
        /// <param name="swCAM"></param>
        public void ComputeFeature(List<SwCAM> swCAMs)
        {
            foreach (SwCAM item in swCAMs)
            {
                switch (item.VolumeType)
                {
                    case (int)CWVolumeType_e.CW_HOLE_VOLUME://孔或者孔组
                        GetFeature_Hole(item);
                        break;
                    case (int)CWVolumeType_e.CW_POCKET_VOLUME://不规则凹腔 、圆形凹腔、腰形凹腔、矩形凹腔
                        break;
                    case (int)CWVolumeType_e.CW_BOSS_VOLUME://圆形凸台
                       
                        break;
                    case (int)CWVolumeType_e.CW_SLOT_VOLUME://不规则槽、矩形槽
                        
                        break;
                    case (int)CWVolumeType_e.CW_SLAB_VOLUME: //面特征
                         
                        break;
                    case (int)CWVolumeType_e.CW_OPENPOCKET_VOLUME: //开放式凹腔 、周界-非封闭凹腔
                        
                        break;
                    case (int)CWVolumeType_e.CW_WORKPIECE_VOLUME:
                        
                        break;
                    case (int)CWVolumeType_e.CW_3AXIS_VOLUME:
                        
                        break;
                    case (int)CWVolumeType_e.CW_HOLECTRSUNK_VOLUME: //埋头孔
                        GetFeature_HoleCtrsunk(item);
                        break;
                    case (int)CWVolumeType_e.CW_HOLECTRBORE_VOLUME: //沉镗孔
                        GetFeature_HoleCtrbore(item);
                        break;
                    case (int)CWVolumeType_e.CW_MULTISTEP_VOLUME:   //MS孔  (多阶)
                        return;
                    case (int)CWVolumeType_e.CW_MULTIFACE_VOLUME:
                        
                        break;
                    default:
                        break;

                }
            }

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
        /// 获取材料
        /// </summary>
        /// <returns></returns>
        public Materials GetMaterials()
        {
            return Materials.Carbon;
        }

        #region 特征金额计算
        /// <summary>
        /// 获取[孔]计算金额
        /// </summary>
        private void GetFeature_Hole(SwCAM swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            //实现过程
            
            if (swCam.SubFeatureCount == 0) //单孔
            {
                Simple_Drilling p = new Simple_Drilling(swCam.Maxdiameter, swCam.Depth, 1, GetMaterials());
                af.TotalTime = p.TotalTime;
                double MachineMoney = GetMachineMoney();
                af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额
            }
            else { //孔组
                Simple_Drilling p = new Simple_Drilling(swCam.Maxdiameter, swCam.Depth, swCam.SubFeatureCount, GetMaterials());
                af.TotalTime = p.TotalTime;
                double MachineMoney = GetMachineMoney();
                af.Money = Convert.ToDecimal(MachineMoney / 60 / 60 * af.TotalTime); //小时换算秒 * 加工时间 = 加工金额
            }

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 获取[埋头孔]计算金额
        /// </summary>
        private void GetFeature_HoleCtrsunk(SwCAM swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程
           

            TotalFeatureMoney.Add(af);
        }
        /// <summary>
        /// 获取[沉镗孔]计算金额
        /// </summary>
        private void GetFeature_HoleCtrbore(SwCAM swCam)
        {
            FeatureAmount af = new FeatureAmount();
            af.FeatureName = swCam.FeatureName;
            af._SwCAM = swCam;
            ///实现过程

            TotalFeatureMoney.Add(af);
        }
        #endregion

        /// <summary>
        /// 获取工序明细（获取时间以及刀具轨迹长度）
        /// </summary>
        /// <returns></returns>
        public List<ProcessDetail> GetProcessDetails()
        {
            List<ProcessDetail> list = new List<ProcessDetail>();

            CWApp cwApp = new CWApp();
            CWPartDoc cwPd = (CWPartDoc)cwApp.IGetActiveDoc();
            CWMillMachine cwMiillMach = (CWMillMachine)cwPd.IGetMachine();
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
                    pd.ToolpathTotalTime= operation.ToolpathTotalTime;// 时间
                    pd.ToolpathTotalLength = operation.ToolpathTotalLength;//刀具总长度
                    list.Add(pd);
                }

            }
            return list;
        }

    }
}
