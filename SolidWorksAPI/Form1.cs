using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SolidWorks.Interop.sldcostingapi; 
using SolidWorks.Interop.swconst;
using System.IO;
using CAMWORKSLib;
using System.Runtime.InteropServices;
using System.Threading; 
using System.Diagnostics;

namespace SolidWorksAPI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 头部
        /// <summary>
        /// Solidworks程序对象
        /// </summary>
        public SldWorks m_SwApp;
        ModelDoc2 m_ModelDoc;
        /// <summary>
        ///   Costing管理器对象
        /// </summary>
        CostManager swCosting = default(CostManager);
        /// <summary>
        /// Cost Part
        /// </summary>
        CostPart swCostingPart = default(CostPart);
        /// <summary>
        /// 切换默认模块
        /// </summary>
        private CostingDefaults swcCostingDefaults = default(CostingDefaults);

        /// <summary>
        ///  Costing Body
        /// </summary>
        CostBody swCostingBody = default(CostBody);
        /// <summary>
        /// 成本核算模块
        /// </summary>
        public CostAnalysis m_swCostingAnalysis = default(CostAnalysis);

        private HZ_Tools m_Tools = HZ_Tools.initInstance();

        #endregion

        string partfilepath = Application.StartupPath + "\\unitTest.SLDPRT";
        string templatePath = Application.StartupPath;

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime dt1 = DateTime.Now;

            HZ_EnumType type = HZ_EnumType.hz_SimpleTurning; //工艺

            int lotSize = 100; //批量大小
            int totalNum = 100;//零件总个数
            string materialClass = "";//材料类型
            string materialName = "";//材料名称
            HZ_StockType stocktype = HZ_StockType.Block;//配料类型


            //Solidworks程序对象
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
            
            //打开文件
            m_ModelDoc = openModle(partfilepath);

            //生成缩略图
            // ModelView LoMyModelView; //生成缩略图
            //LoMyModelView = m_ModelDoc.ActiveView;
            //LoMyModelView.FrameState = (int)swWindowState_e.swWindowMaximized;
            ////m_ModelDoc.ShowNamedView2("*Isometric", 7);
            //m_ModelDoc.ViewZoomtofit2();
            //m_ModelDoc.SaveAs(Application.StartupPath + "\\test.jpg");
            //m_ModelDoc.SaveAs(Application.StartupPath + "\\test.stl");


            //仅支持零部件
            if (m_ModelDoc.GetType() != (int)swDocumentTypes_e.swDocPART)
            {
                MessageBox.Show("仅支持零部件！");
                return;
            }
            bool isSheetMetal = false;
            bool isSolidBody = false;
            bool isBlank = false;
            string strError = string.Empty;

            //仅支持单实体零部件
            if (isMutiBodyOrSheetMetal(ref isBlank, ref isSheetMetal, ref isSolidBody, ref strError))
            {
                MessageBox.Show("仅支持单体零部件！");
                return;
            }

            //空白图纸
            if (isBlank)
            {
                MessageBox.Show("空白图纸！");
                return;
            }

            //不支持钣金件
            if (isSheetMetal)
            {
                MessageBox.Show("不支持钣金件！");
                return;
            }
            //是否为实体零件
            if (!isSolidBody)
            {
                MessageBox.Show("非实体零部件！");
                return;
            }

            //获取SolidWorks扩展器
            ModelDocExtension swModelDocExt = m_ModelDoc.Extension;

            // 初始化成本输出对象
            HZ_CostingOutput CostingOutput = new HZ_CostingOutput();

            //详细输出对象
            HZ_MassProperty massOutput = new HZ_MassProperty();

            // 得到质量属性 Part Mass
            getMass("", ref massOutput);

            // 得到外形尺寸属性
            getBox(ref massOutput);

            // 得到 CostingManager
            swCosting = (CostManager)swModelDocExt.GetCostingManager();
            swCosting.WaitForUIUpdate();

            // 得到 Costing part
            object swCostingModel = (object)swCosting.CostingModel;
            swCostingPart = (CostPart)swCostingModel;

            //设置默认模块参数
            //模板，批量大小，零件总个数，材料类型，材料名称，计算方式，配料类型
            swcCostingDefaults = (CostingDefaults)swCosting.CostingDefaults;
            swcCostingDefaults.SetTemplateName((int)swcCostingType_e.swcCostingType_Machining, selectCostingTemp(type));

            //单体零件
            swcCostingDefaults.LotSizeForSingleBody = lotSize;
            swcCostingDefaults.TotalNumberOfPartsForSingleBody = totalNum;

            //多实体文件
            swcCostingDefaults.LotSizeForMultibody = lotSize;
            swcCostingDefaults.TotalNumberOfPartsForMultibody = totalNum;

            //设置材料以及材料处理方式
            swcCostingDefaults.SetMaterialClass((int)swcMethodType_e.swcMethodType_Machining, materialClass);
            swcCostingDefaults.SetMaterialName((int)swcMethodType_e.swcMethodType_Machining, materialName);
            swcCostingDefaults.SetManufacturingMethod((int)swcBodyType_e.swcBodyType_Machined, (int)swcBodyType_e.swcBodyType_Machined);
            swcCostingDefaults.MachiningStockBodyType = (int)stocktype;

            //获取Cost Body
            swCostingBody = swCostingPart.SetCostingMethod("", (int)swcMethodType_e.swcMethodType_Machining);
            if (swCostingBody == null)
            {
                 
            }

            // 创建 common Costing analysis
            m_swCostingAnalysis = swCostingBody.CreateCostAnalysis(selectCostingTemp(type));
            m_swCostingAnalysis = swCostingBody.GetCostAnalysis();
            m_swCostingAnalysis.TotalQuantity = totalNum;
            m_swCostingAnalysis.LotSize = lotSize;

            // 得到 Costing bodies
            int nbrCostingBodies = swCostingPart.GetBodyCount();
            if (hasBody(nbrCostingBodies, swCostingPart, type))
            {
                // 得到 machining Costing Analysis 数据
                getMachiningCostingAnalysisData(CostingOutput, ref massOutput, stocktype, materialClass, materialName);

                //得到返回的质量属性
                CostingOutput.massProperty = massOutput;

                // 得到 common Costing analysis 数据
                getCommonCostingAnalysisData(CostingOutput, totalNum, lotSize);

                //孔超标数据，标准 > 5 直接剔除，并+2 元，处理倒角导致成本超高的问题
                getCostingFeatures(ref CostingOutput);
            }


            DateTime dt2 = DateTime.Now;

            TimeSpan dt3 = dt2 - dt1;

            int goTime = dt3.Seconds;
          //  MessageBox.Show(moneys.ToString() + "元");
        }
        #region 扩展
        /// <summary>
        /// 有无待核算体
        /// </summary>
        /// <param name="bodies">待核算体</param>
        /// <param name="swcostingpart">CostPart参数</param>
        /// <param name="type">零件类型</param>
        /// <returns>是否有</returns>
        private bool hasBody(int bodies, CostPart swcostingpart, HZ_EnumType type)
        {
            bool isBody = false;
            if ((bodies > 0))
            {
                var costingBodies = (object[])swcostingpart.GetBodies();
                CostBody swCostingBody = (CostBody)costingBodies[0];
                string costingBodyName = swCostingBody.GetName();
                // 确保是机加工零件
                if ((swCostingBody.GetBodyType() == (int)swcBodyType_e.swcBodyType_Machined))
                {
                    isBody = true;
                    switch ((int)swCostingBody.BodyStatus)
                    {
                        case (int)swcBodyStatus_e.swcBodyStatus_Analysed:
                            // 得到模板
                            m_swCostingAnalysis = swCostingBody.CreateCostAnalysis((selectCostingTemp(type)));
                            m_swCostingAnalysis = swCostingBody.GetCostAnalysis();
                            break;
                        default:
                            isBody = false;
                            break;
                    }
                }
            }
            return isBody;
        }

        /// <summary>
        /// 选择Costing模板
        /// </summary>
        /// <param name="type">零件类型（6种）</param>
        /// <returns>模板字符串</returns>
        private string selectCostingTemp(HZ_EnumType type)
        {
            string temp = templatePath;
            switch (type)
            {
                case HZ_EnumType.hz_SimpleTurning:
                    temp += "SimpleTurning.sldctm";
                    break;
                case HZ_EnumType.hz_ComplexTurning:
                    temp += "ComplexTurning.sldctm";
                    break;
                case HZ_EnumType.hz_TurnMill:
                    temp += "TurnMill.sldctm";
                    break;
                case HZ_EnumType.hz_3AxisMilling:
                    temp += "3AxisMilling.sldctm";
                    break;
                case HZ_EnumType.hz_4AxisMilling:
                    temp += "4AxisMilling.sldctm";
                    break;
                case HZ_EnumType.hz_5AxisMilling:
                    temp += "5AxisMilling.sldctm";
                    break;
            }
            return temp;
        }

        //判断是否为多实体零件或钣金件
        private bool isMutiBodyOrSheetMetal(ref bool isBlank, ref bool isSM, ref bool isSolidBody, ref string strError)
        {
            if (m_ModelDoc != null)
            {
                PartDoc swPartDoc = (PartDoc)m_ModelDoc;
                if (swPartDoc != null)
                {
                    object[] vBodies = (object[])swPartDoc.GetBodies2((int)swBodyType_e.swAllBodies, false);
                    if (vBodies == null)
                    {
                        strError = "空白图纸";
                        isBlank = true;
                        return false;
                    }
                    if (vBodies.Length > 1)
                    {
                        return true;
                    }
                    else if (vBodies.Length == 1)
                    {
                        Body2 body = (Body2)vBodies[0];
                        isSM = body.IsSheetMetal();

                        //识别文件类型体
                        isSolidBody = false;
                        switch (body.GetType())
                        {
                            case (int)swBodyType_e.swSolidBody:
                                strError = "实体文件";
                                isSolidBody = true;
                                break;
                            case (int)swBodyType_e.swSheetBody:
                                strError = "板体文件";
                                break;
                            case (int)swBodyType_e.swEmptyBody:
                                strError = "空白体文件";
                                break;
                            case (int)swBodyType_e.swGeneralBody:
                                strError = "一般，非流形体文件";
                                break;
                            case (int)swBodyType_e.swMinimumBody:
                                strError = "点体文件";
                                break;
                            case (int)swBodyType_e.swWireBody:
                                strError = "线体文件";
                                break;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 打开零部件文件
        /// </summary>
        /// <param name="partfilepath">零部件文件路径</param>
        /// <param name="err">错误文本</param>
        private ModelDoc2 openModle(String partfilepath)
        {
            closeAllSWDoc();

            ModelDoc2 doc = null;
            int doctype = getDocTyoe(partfilepath);

            if (1 == doctype)
            {
                doc = m_SwApp.OpenDoc6(partfilepath, (int)swDocumentTypes_e.swDocPART, (int)swOpenDocOptions_e.swOpenDocOptions_Silent, "", 0, 0);
            }
            else if (doctype > 1)
            {
                doc = loadFile(partfilepath, doctype);
            }
            return doc;
        }
        /// <summary>
        /// 关闭所有打开的SW文档
        /// </summary>
        private void closeAllSWDoc()
        {
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
            if (m_SwApp != null)
            {
                m_SwApp.CloseAllDocuments(true);
            }
        }
        /// <summary>
        /// 载入文件
        /// </summary>
        /// <param name="partfilepath">文件路径</param>
        /// <param name="doctype">ModelDoc2模型</param>
        /// <returns>ModelDoc2对象</returns>
        private ModelDoc2 loadFile(String partfilepath, int doctype)
        {
            ModelDoc2 doc = null;
            switch (doctype)
            {
                case 2:
                case 3:
                    doc = loadStepFile(partfilepath);
                    break;
                case 4:
                case 5:
                    doc = loadIgesFile(partfilepath);
                    break;
            }
            doc?.SaveAs(System.IO.Path.GetDirectoryName(partfilepath) + "//" + System.IO.Path.GetFileNameWithoutExtension(partfilepath) + ".sldprt");
            return doc;
        }
        /// <summary>
        /// 得到特征分析数据
        /// </summary>
        /// <param name="costingoutput">成本输出对象</param>
        private double getCostingFeatures(ref HZ_CostingOutput costingoutput)
        {
            double superCost = 0;

            List<HZ_FeatCost> FeatCostList = new List<HZ_FeatCost>();
            CostFeature swCostingFeat = (CostFeature)m_swCostingAnalysis.GetFirstCostFeature();

            // 得到特征数据
            while ((swCostingFeat != null))
            {
                HZ_FeatCost FeatCost = new HZ_FeatCost
                {
                    feature = swCostingFeat.Name,
                    type = swCostingFeat.GetType(),
                    description = swCostingFeat.Description,
                    combinedCost = m_Tools.specNum(swCostingFeat.CombinedCost),
                    combinedTime = m_Tools.getTimeCost(swCostingFeat.CombinedTime)
                };
                // 得到子特征数据
                CostFeature swCostingSubFeat = swCostingFeat.GetFirstSubFeature();
                while ((swCostingSubFeat != null))
                {
                    //孔超标数据，标准 > 5 直接剔除，并+2 元
                    if (swCostingFeat.GetType() == (int)swcCostFeatureType_e.swcMachiningHoleOperationsFolderType && m_Tools.specNum(swCostingSubFeat.CombinedCost) > 5)
                    {
                        costingoutput.totalManufacturingCost -= swCostingSubFeat.CombinedCost;
                        costingoutput.totalCostrueToManufacture -= swCostingSubFeat.CombinedCost;
                        costingoutput.totalCostrueToCharge -= swCostingSubFeat.CombinedCost;

                        costingoutput.totalManufacturingCost += 2;
                        costingoutput.totalCostrueToManufacture += 2;
                        costingoutput.totalCostrueToCharge += 2;
                    }

                    HZ_SubFeatCost SubFeatCost = new HZ_SubFeatCost
                    {
                        SubFeature = swCostingSubFeat.Name,
                        type = swCostingSubFeat.GetType(),
                        description = swCostingSubFeat.Description,
                        combinedCost = m_Tools.specNum(swCostingSubFeat.CombinedCost),
                        combinedTime = m_Tools.getTimeCost(swCostingSubFeat.CombinedTime)
                    };

                    FeatCost.subFeatCostList.Add(SubFeatCost);
                    CostFeature swCostingNextSubFeat = (CostFeature)swCostingSubFeat.GetNextFeature();
                    swCostingSubFeat = null;
                    swCostingSubFeat = (CostFeature)swCostingNextSubFeat;
                    swCostingNextSubFeat = null;
                }
                CostFeature swCostingNextFeat = swCostingFeat.GetNextFeature();
                swCostingFeat = null;
                swCostingFeat = (CostFeature)swCostingNextFeat;
                swCostingNextFeat = null;
                FeatCostList.Add(FeatCost);
            }

            // 成本输出文件赋值
            costingoutput.featCostList = FeatCostList;
            return superCost;
        }

        /// <summary>
        /// 得到通用成本分析数据
        /// </summary>
        /// <param name="costingoutput">成本输出对象</param>
        private void getCommonCostingAnalysisData(HZ_CostingOutput costingoutput, int totalNum, int lotSize)
        {
            //设置零件总个数和批量大小
            //m_swCostingAnalysis.TotalQuantity = totalNum;
            //m_swCostingAnalysis.LotSize = lotSize;

            // 得到 common Costing analysis 数据
            // 成本输出对象赋值
            costingoutput.templateName = m_swCostingAnalysis.CostingTemplateName;
            costingoutput.currencyCode = m_swCostingAnalysis.CurrencyCode;
            costingoutput.currencyName = m_swCostingAnalysis.CurrencyName;
            costingoutput.currencySeparator = m_swCostingAnalysis.CurrencySeparator;
            costingoutput.totalManufacturingCost = m_swCostingAnalysis.GetManufacturingCost();
            costingoutput.materialCosts = m_swCostingAnalysis.GetMaterialCost();
            costingoutput.totalNum = m_swCostingAnalysis.TotalQuantity;
            costingoutput.lotSize = m_swCostingAnalysis.LotSize;
            costingoutput.totalCostrueToCharge = m_swCostingAnalysis.GetTotalCostToCharge();
            costingoutput.totalCostrueToManufacture = m_swCostingAnalysis.GetTotalCostToManufacture();


        }

        /// <summary>
        /// 得到机加工属性数据数据
        /// </summary>
        /// <param name="costingoutput">成本输出对象</param>
        private void getMachiningCostingAnalysisData(HZ_CostingOutput costingoutput, ref HZ_MassProperty massOutput, HZ_StockType stockType, string materialClass, string materialName)
        {
            // 得到机加工属性
            CostAnalysisMachining swCostingMachining = (CostAnalysisMachining)m_swCostingAnalysis.GetSpecificAnalysis();
            if (swCostingMachining != null)
            {
                swCostingMachining.CurrentMaterialClass = materialClass;
                swCostingMachining.CurrentMaterial = materialName;
                swCostingMachining.CurrentStockType = (int)stockType;


                costingoutput.stockType = swCostingMachining.CurrentStockType;
                costingoutput.currentMaterial = swCostingMachining.CurrentMaterial;
                costingoutput.currentMaterialClass = swCostingMachining.CurrentMaterialClass;
                //costingoutput.currentPlateThickness = swCostingMachining.CurrentPlateThickness;
            }
        }

        /// <summary>
        /// 得到尺寸属性
        /// </summary>
        /// <param name="costingoutput">成本输出对象</param>
        private void getBox(ref HZ_MassProperty costingoutput)
        {
            if (m_ModelDoc != null)
            {
                object boxobj = new object();
                PartDoc swPartDoc = (PartDoc)m_ModelDoc;
                if (swPartDoc != null)
                {
                    boxobj = swPartDoc.GetPartBox(true);
                    if (boxobj != null)
                    {
                        double[] BoxFaceDblArray = new double[7];
                        BoxFaceDblArray = (double[])boxobj;
                        // 成本输出对象赋值
                        costingoutput.x = (BoxFaceDblArray[3] - BoxFaceDblArray[0]).ToString();
                        costingoutput.y = (BoxFaceDblArray[4] - BoxFaceDblArray[1]).ToString();
                        costingoutput.z = (BoxFaceDblArray[5] - BoxFaceDblArray[2]).ToString();
                        costingoutput.box = (BoxFaceDblArray[5] - BoxFaceDblArray[2]).ToString();
                    }
                }
            }
        }

        /// <summary>
        /// 得到质量属性
        /// <param name="trademark">材料牌号</param>
        /// <param name="costingoutput">成本输出对象</param>
        /// </summary>
        private void getMass(string trademark, ref HZ_MassProperty costingoutput)
        {
            if (m_ModelDoc != null)
            {
                MassProperty mp = m_ModelDoc.Extension.CreateMassProperty();
                mp.UseSystemUnits = true;

                // 获取名称
                costingoutput.name = m_ModelDoc.GetTitle();
                // 获取材料牌号-没什么用
                costingoutput.material = trademark;
                // 获取重量
                costingoutput.mass = mp.Mass.ToString();
                // 获取密度
                costingoutput.density = mp.Density.ToString();
                // 获取表面积
                costingoutput.surface = mp.SurfaceArea.ToString();
                // 获取体积
                costingoutput.volume = mp.Volume.ToString();
            }
        }

        /// <summary>
        /// 载入STP、STEP文件
        /// </summary>
        /// <param name="partfilepath">文件路径</param>
        /// <returns>ModelDoc2模型</returns>
        private ModelDoc2 loadStepFile(String partfilepath)
        {
            ModelDoc2 doc = null;
            ImportStepData importData = (ImportStepData)m_SwApp.GetImportFileData(partfilepath);
            if ((importData != null))
            {
                var err = 0;
                doc = (ModelDoc2)m_SwApp.LoadFile4(partfilepath, "r", importData, ref err);
            }
            return doc;
        }
        /// <summary>
        /// 载入IGS、IGES格式文件
        /// </summary>
        /// <param name="partfilepath">文件路径</param>
        /// <returns>ModelDoc2模型</returns>
        private ModelDoc2 loadIgesFile(String partfilepath)
        {
            ModelDoc2 doc = null;
            ImportIgesData importData = (ImportIgesData)m_SwApp.GetImportFileData(partfilepath);
            if ((importData != null))
            {
                var err = 0;
                doc = (ModelDoc2)m_SwApp.LoadFile4(partfilepath, "r", importData, ref err);
            }
            return doc;
        }
        /// <summary>
        /// 得到SW文本类型(STEP（STP）， x_t，stl ，prt, sldprt，ipt，IGS(iges)等通用格式)
        /// </summary>
        /// <param name="path">文本路径</param>
        /// <returns>文本类型</returns>
        private int getDocTyoe(string path)
        {
            string suffix = Path.GetExtension(path).ToUpper();

            int type = 0;
            switch (suffix)
            {
                case HZ_Extension.PRT:
                case HZ_Extension.SLDPRT:
                    type = 1;
                    break;
                case HZ_Extension.STEP:
                    type = 2;
                    break;
                case HZ_Extension.STP:
                    type = 3;
                    break;
                case HZ_Extension.IGS:
                    type = 4;
                    break;
                case HZ_Extension.IGES:
                    type = 5;
                    break;
            }
            return type;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_SwApp != null)
            {
                m_SwApp.CloseAllDocuments(true);
            }
        }

        #endregion

        private void button3_Click(object sender, EventArgs e)
        {
            HZ_EnumType type = HZ_EnumType.hz_SimpleTurning; //工艺

            int lotSize = 100; //批量大小
            int totalNum = 100;//零件总个数
            string materialClass = "";//材料类型
            string materialName = "";//材料名称
            HZ_StockType stocktype = HZ_StockType.Block;//配料类型


            //Solidworks程序对象
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));

            //打开文件
            m_ModelDoc = openModle(partfilepath);

            //生成缩略图
            // ModelView LoMyModelView; //生成缩略图
            //LoMyModelView = m_ModelDoc.ActiveView;
            //LoMyModelView.FrameState = (int)swWindowState_e.swWindowMaximized;
            ////m_ModelDoc.ShowNamedView2("*Isometric", 7);
            //m_ModelDoc.ViewZoomtofit2();
            //m_ModelDoc.SaveAs(Application.StartupPath + "\\test.jpg");
            //m_ModelDoc.SaveAs(Application.StartupPath + "\\test.stl");


            //仅支持零部件
            if (m_ModelDoc.GetType() != (int)swDocumentTypes_e.swDocPART)
            {
                MessageBox.Show("仅支持零部件！");
                return;
            }
            bool isSheetMetal = false;
            bool isSolidBody = false;
            bool isBlank = false;
            string strError = string.Empty;

            //仅支持单实体零部件
            if (isMutiBodyOrSheetMetal(ref isBlank, ref isSheetMetal, ref isSolidBody, ref strError))
            {
                MessageBox.Show("仅支持单体零部件！");
                return;
            }

            //空白图纸
            if (isBlank)
            {
                MessageBox.Show("空白图纸！");
                return;
            }

            //不支持钣金件
            if (isSheetMetal)
            {
                MessageBox.Show("不支持钣金件！");
                return;
            }
            //是否为实体零件
            if (!isSolidBody)
            {
                MessageBox.Show("非实体零部件！");
                return;
            }

            //获取SolidWorks扩展器
            ModelDocExtension swModelDocExt = m_ModelDoc.Extension;

            // 初始化成本输出对象
            HZ_CostingOutput CostingOutput = new HZ_CostingOutput();

            //详细输出对象
            HZ_MassProperty massOutput = new HZ_MassProperty();

            // 得到质量属性 Part Mass
            getMass("", ref massOutput);

            // 得到外形尺寸属性
            getBox(ref massOutput);

            swCosting = (CostManager)swModelDocExt.GetCostingManager();
            swCosting.WaitForUIUpdate();

            // 得到 Costing part
            object swCostingModel = (object)swCosting.CostingModel;
            swCostingPart = (CostPart)swCostingModel;

            //设置默认模块参数
            //模板，批量大小，零件总个数，材料类型，材料名称，计算方式，配料类型
            swcCostingDefaults = (CostingDefaults)swCosting.CostingDefaults;
            swcCostingDefaults.SetTemplateName((int)swcCostingType_e.swcCostingType_Machining, selectCostingTemp(type));

            //单体零件
            swcCostingDefaults.LotSizeForSingleBody = lotSize;
            swcCostingDefaults.TotalNumberOfPartsForSingleBody = totalNum;

            //多实体文件
            swcCostingDefaults.LotSizeForMultibody = lotSize;
            swcCostingDefaults.TotalNumberOfPartsForMultibody = totalNum;

            //设置材料以及材料处理方式
            swcCostingDefaults.SetMaterialClass((int)swcMethodType_e.swcMethodType_Machining, materialClass);
            swcCostingDefaults.SetMaterialName((int)swcMethodType_e.swcMethodType_Machining, materialName);
            swcCostingDefaults.SetManufacturingMethod((int)swcBodyType_e.swcBodyType_Machined, (int)swcBodyType_e.swcBodyType_Machined);
            swcCostingDefaults.MachiningStockBodyType = (int)stocktype;

            //获取Cost Body
            swCostingBody = swCostingPart.SetCostingMethod("", (int)swcMethodType_e.swcMethodType_Machining);
            if (swCostingBody == null)
            {
                MessageBox.Show("制造成本计算失败");
                return;
            }


            // 创建 common Costing analysis
            m_swCostingAnalysis = swCostingBody.CreateCostAnalysis(selectCostingTemp(type));
            m_swCostingAnalysis = swCostingBody.GetCostAnalysis();
            m_swCostingAnalysis.TotalQuantity = totalNum;
            m_swCostingAnalysis.LotSize = lotSize;

            CostFeature swCostingFeat = default(CostFeature);
            CostFeature swCostingNextFeat = default(CostFeature);
            CostFeature swCostingSubFeat = default(CostFeature);
            CostFeature swCostingNextSubFeat = default(CostFeature);

            swCostingFeat = (CostFeature)m_swCostingAnalysis.GetFirstCostFeature();
            while ((swCostingFeat != null))
            {
                //  swcCostFeatureType_e.

                Debug.Print("    Feature: " + swCostingFeat.Name);
                Debug.Print("      Type: " + swCostingFeat.GetType());
                Debug.Print("        Setup related: " + swCostingFeat.IsSetup);
                Debug.Print("        Overridden: " + swCostingFeat.IsOverridden);
                Debug.Print("        Combined cost: " + swCostingFeat.CombinedCost);
                Debug.Print("        Combined time: " + swCostingFeat.CombinedTime);

                swCostingSubFeat = swCostingFeat.GetFirstSubFeature();
                while ((swCostingSubFeat != null))
                {
                    Debug.Print("      Subfeature: " + swCostingSubFeat.Name);
                    Debug.Print("        Type: " + swCostingSubFeat.GetType());
                    Debug.Print("          Setup related: " + swCostingSubFeat.IsSetup);
                    Debug.Print("          Overridden: " + swCostingSubFeat.IsOverridden);
                    Debug.Print("          Combined cost: " + swCostingSubFeat.CombinedCost);
                    Debug.Print("          Combined time: " + swCostingSubFeat.CombinedTime);

                    swCostingNextSubFeat = (CostFeature)swCostingSubFeat.GetNextFeature();
                    swCostingSubFeat = null;
                    swCostingSubFeat = (CostFeature)swCostingNextSubFeat;
                    swCostingNextSubFeat = null;

                }
                swCostingNextFeat = swCostingFeat.GetNextFeature();
                swCostingFeat = null;
                swCostingFeat = (CostFeature)swCostingNextFeat;
                swCostingNextFeat = null;

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            CWApp cwApp = new CWApp();
            CWPartDoc cwPd = (CWPartDoc)cwApp.IGetActiveDoc();
            CWMillMachine cwMiillMach = (CWMillMachine)cwPd.IGetMachine();
            string name = cwMiillMach.Name;
            string duty = cwMiillMach.MachDuty;
            double feed = cwMiillMach.FeedRateLimit;
            string ID = cwMiillMach.MachId;
            double power = cwMiillMach.Horsepower;
            double spindle = cwMiillMach.SpindleSpeedLimit;

            CWDoc cwDoc = (CWDoc)cwApp.IGetActiveDoc();

            cwApp.ActiveDocEMF();//提取特征
            //cwApp.ActiveDocGOP(1);// （暂时不理解,但是不调用 生成操作计划会有问题）
            //cwApp.ActiveDocGTP();//生成操作计划

            CWPartDoc cwPartDoc = (CWPartDoc)cwDoc;
            CWMachine cwMach = (CWMachine)cwPartDoc.IGetMachine();

            CWDispatchCollection cwDispCol = (CWDispatchCollection)cwMach.IGetEnumSetups();

            for (int i = 0; i < cwDispCol.Count; i++)// 铣削零件设置组 
            {
                CWBaseSetup cwBaseSetup = (CWBaseSetup)cwDispCol.Item(i);

                if (cwBaseSetup == null)
                    continue;

                Console.Write("铣削设置" + i.ToString());

                CWDispatchCollection FeatList = (CWDispatchCollection)cwBaseSetup.IGetEnumFeatures();//方法获取安装程序上的所有特性

                for (int j = 0; j < FeatList.Count; j++)
                {


                    CWMillFeature cwFeat = (CWMillFeature)FeatList.Item(j);
                    int attr = cwFeat.Attribute;
                    string strAttr = "";
                    int intAttr = 0;

                    cwFeat.GetAttribute(out strAttr, out intAttr);//获取


                    int subType = cwFeat.SubType;
                    int volume = cwFeat.VolumeType;
                    double depth = cwFeat.Depth;

                    double taperAngle = cwFeat.IGetTaperAngle();
                    double topFillRadius = cwFeat.IGetTopFilletRadius();

                    int island = cwFeat.IGetIslandCount();

                    double dLeng = 0;
                    double dwidth = 0;
                    double dDepth = 0;
                    cwFeat.GetBoundParams(out dLeng, out dwidth, out dDepth);


                    //string [] attrs = cwFeat.IGetAllAttributes();

                    int tbdid = cwFeat.GetTdbIdForAttribute(strAttr);



                    // CWTurnFeature cwTurn = (CWTurnFeature)
                    // string strName = cwTurn.GetAttributeName();



                }

            }


        }
        /// <summary>
        /// 获取CAM加工时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            DateTime dt1 = DateTime.Now;

            CAM_Feature cf = new CAM_Feature();
            List<ProcessDetail> list = cf.GetProcessDetails();
            double sums =  list.Sum(p => p.ToolpathTotalTime);

            int temp = Convert.ToInt32(Math.Round(sums, 2));
            double temp2 = Math.Round(sums * 60, 2);
            string sumStr = "【获取CAM加工时间】\n";

            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            sumStr += "程序用时:" + ts.Seconds + "秒\n";

            sumStr += "共(sec) : " + temp2.ToString() + "秒 \n共(min) :" + temp + "分钟 \n";
            txtMsg.Text = sumStr;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            CWApp cwApp = new CWApp();
            CWPartDoc cwPd = (CWPartDoc)cwApp.IGetActiveDoc();
            CWDoc cwDoc = (CWDoc)cwApp.IGetActiveDoc();

           int GX = (int)CWMachineType.CW_MACHINE_TYPE_TURN;
 
            CAMWORKSLib.CWApp cwapp = new CAMWORKSLib.CWApp();
            CAMWORKSLib.CWDoc doc = cwapp.IGetActiveDoc(); 
 
            ICWDispatchCollection Machines = cwapp.GetMachines(GX);

            foreach (CWTurnMachine item in Machines)
            {
                if ("Turn Dual Turret - Inch" == item.Name)
                {
                    doc.SetMachine2(item);
                    break;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            CAM_Feature cf = new CAM_Feature();
            List<SwCAM_Turn> list = cf.GetFeatuer_Turn();

            //cf.ComputeFeature(list); //计算总特征
            // decimal moneys = cf.GetTotalMoney();//得出最后的成本核算价
            // double time = cf.GetTotalTime();//加工总用时
            //AllFeature f = new AllFeature();
            //f.list = list;
            //f.ShowDialog();

            //精铣   EdgeRadius 棱角半径暂且给定 刀具半径
            //Axis3_SurfaceFinishMilling fm = new Axis3_SurfaceFinishMilling(Cutter_Drill.GetFinish(CutterTool), Cutter_Drill.GetFinish(CutterTool) / 2, (bound[0] * bound[1] * bound[2]), 1.6, 1, GetMaterials());
            //af.TotalTime += fm.TotalTime * (swCam.SubFeatureCount == 0 ? 1 : swCam.SubFeatureCount);

        }
        //特征刀轨二合一
        private void button2_Click_1(object sender, EventArgs e)
        {
            DateTime dt1 = DateTime.Now;
            CAM_Feature cf = new CAM_Feature();
            MergeFeatrueDetail list = cf.GetFeatuerMill_And_Process();
            cf.ComputeFeature_Mill_Process(list); //计算总特征 

            double time = cf.GetTotalTime();//加工总用时
            int temp = Convert.ToInt32(Math.Round(time, 0));
            double temp2 = Math.Round(time / 60, 0);
            string sumStr = "【特征刀轨二合一】\n";
            foreach (FeatureAmount item in cf.TotalFeatureMoney)
            {
                sumStr += (item.FeatureName + "   :   " + Convert.ToInt32(Math.Round(item.TotalTime, 0)) + "秒\n");
            }
            sumStr += "======================\n";

            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            sumStr += "程序用时:" + ts.Seconds + "秒";

            sumStr += "\n共(sec) : " + temp.ToString() + "秒 \n共(min) :" + temp2 + "分钟 \n";
            txtMsg.Text = sumStr;
        }

        //获取铣削特征
        private void button5_Click(object sender, EventArgs e)
        {
            txtMsg.Text = "";
            Application.DoEvents();

            DateTime dt1 = DateTime.Now;

            CAM_Feature cf = new CAM_Feature();
            List<SwCAM_Mill> list = cf.GetFeatuer_Mill();
            cf.ComputeFeature_Mill(list); //计算总特征
             // decimal moneys = cf.GetTotalMoney();//得出最后的成本核算价
            double time = cf.GetTotalTime();//加工总用时
            int temp = Convert.ToInt32(Math.Round(time, 0));
            double temp2 = Math.Round(time / 60, 0);
            string sumStr = "";
            foreach (FeatureAmount item in cf.TotalFeatureMoney)
            {
                if (item.FeatureName.IndexOf("装夹") >= 0) sumStr += "\n";
                if (item.FeatureName.IndexOf("矩形槽") >= 0 || item.FeatureName.IndexOf("不规则槽") >= 0 || item.FeatureName.IndexOf("不规则凹腔") >= 0 ||
                    item.FeatureName.IndexOf("矩形凹腔") >= 0 || item.FeatureName.IndexOf("腰型") >= 0 || item.FeatureName.IndexOf("圆形凹腔") >= 0)
                {
                    sumStr += "┏┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┓\n";
                    string isCg = item._SwCAM.ThroughOrblind == 0 ? "否" : "是";
                    sumStr += ("┣  【" + item.FeatureName + "】: " + Convert.ToInt32(Math.Round(item.TotalTime, 0)) + "秒 "+ GetMin(Convert.ToInt32(Math.Round(item.TotalTime, 0))) + "\n" +
                          "┣  单次时间：" + item.Test_SingleTime + "秒\n" +
                          "┣  走刀次数:" + item.Test_ProcessCount + "次\n" +
                          "┣  裁剪单长度:" + Math.Round(item.Test_CuttingLength/ item.Test_ProcessCount,1) + "mm\n" +
                          "┣  裁剪总长度:" + Math.Round(item.Test_CuttingLength, 1) + "mm\n" +
                          "┣  刀具直径：" + item.Test_Dia + "mm\n"+
                          "┣  下刀深度：" + item.Test_CutteDepth + "mm\n" +
                          "┣  进给率：" + item.Test_FeedRate + "(mm/min)\n" +
                          "┣  材料切割速率：" + item.Test_CuttingSpeed + "倍\n" +
                          "┣  穿过:" + isCg + " \n" +
                          "┣  组:X" + item._SwCAM.SubFeatureCount + " \n" +
                          "┣  尺寸(mm):[" + Math.Round(item._SwCAM.Bound[0], 2) + " * " + Math.Round(item._SwCAM.Bound[1], 2) + " * " + Math.Round(item._SwCAM.Depth, 2) + "]\n" +
                          "┣  " + item.Test_MethodName + " \n");
                    sumStr += "┗┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┛\n";
                }
                else if (item.FeatureName.IndexOf("开放式凹腔") >= 0)
                {
                    sumStr += "┏┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┓\n";
                    sumStr += ("┣  【" + item.FeatureName + "】: " + Convert.ToInt32(Math.Round(item.TotalTime, 0)) + "秒 " + GetMin(Convert.ToInt32(Math.Round(item.TotalTime, 0))) + "\n" +
                           "┣  粗铣单次时间：" + item.Test_SingleTime + "秒\n"+
                           "┣  走刀次数:" + item.Test_ProcessCount + "次\n"+
                           "┣  刀具直径：" + item.Test_Dia + " \n"+
                           "┣  岛屿数量:" + item.Test_IsLandCount + "\n"+
                           "┣  岛屿共耗时:" + item.Test_IsLandTime + "秒\n"+
                           "┣  岛屿尺寸(mm):[" + Math.Round(item.Test_IsLandSize[0], 2) + " * " + Math.Round(item.Test_IsLandSize[1], 2) + " * " + Math.Round(item.Test_IsLandSize[2], 2) + "]  \n"+
                           "┣  尺寸(mm):[" + Math.Round(item._SwCAM.Bound[0], 2) + " * " + Math.Round(item._SwCAM.Bound[1], 2) + " * " + Math.Round(item._SwCAM.Depth, 2) + "]\n" +
                           "┣  组:X" + item._SwCAM.SubFeatureCount + " \n");
                    sumStr += "┗┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┛\n";
                }
                else if (item.FeatureName.IndexOf("MS") >= 0)
                {
                    sumStr += "┏┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┓\n";
                    sumStr += "┣  【" + item.FeatureName + "(X" + item._SwCAM.SubFeatureCount + ")】: " + Convert.ToInt32(Math.Round(item.TotalTime, 0)) + "秒 " + GetMin(Convert.ToInt32(Math.Round(item.TotalTime, 0))) + "\n";
                    sumStr += "┣   阶梯数目:" + item._SwCAM.SubMultiStep.Count + "\n";
                    sumStr += "┣   深度:" + Math.Round(item._SwCAM.Depth, 2) + "\n";
                    sumStr += "┣   组:X" + item._SwCAM.SubFeatureCount + "\n";
                    sumStr += "┗┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┛\n";
                }
                else if (item.FeatureName.IndexOf("沉镗孔") >= 0)
                {
                    sumStr += "┏┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┓\n";
                    sumStr += "┣  【" + item.FeatureName + "(X" + item._SwCAM.SubFeatureCount + ")】: " + Convert.ToInt32(Math.Round(item.TotalTime, 0)) + "秒 " + GetMin(Convert.ToInt32(Math.Round(item.TotalTime, 0))) + "\n";

                    sumStr += "┣   ========= 镗孔 =========\n";
                    if (item._SwCAM.BoreDiameter > 20)
                    {
                        sumStr += "┣   头部时间:" + Math.Round(item.Test_BoreTopTime, 2) + "秒\n" +
                          "┣   直径:" + Math.Round(item._SwCAM.BoreDiameter, 2) + "\n"+
                          "┣   深度:" + Math.Round(item._SwCAM.BoreDepth, 2) + "\n"+
                          "┣   单次时间：" + item.Test_SingleTime + "秒\n" +
                          "┣   走刀次数:" + item.Test_ProcessCount + "次\n" +
                          "┣   整体时间:" + item.Test_ProcessCount * item.Test_SingleTime + "秒\n" +
                          "┣   裁剪单长度:" + Math.Round(item.Test_CuttingLength / item.Test_ProcessCount, 1) + "mm\n" +
                          "┣   裁剪总长度:" + Math.Round(item.Test_CuttingLength,1) + "mm\n" +
                          "┣   刀具直径：" + item.Test_Dia + "mm\n" +
                          "┣   下刀深度：" + item.Test_CutteDepth + "mm\n" +
                          "┣   进给率：" + item.Test_FeedRate + "(mm/min)\n" +
                          "┣   材料切割速率：" + item.Test_CuttingSpeed + "倍\n" +
                          "┣   尺寸(mm):[" + Math.Round(item._SwCAM.Bound[0], 2) + " * " + Math.Round(item._SwCAM.Bound[1], 2) + " * " + Math.Round(item._SwCAM.Depth, 2) + "]\n" +
                          "┣   " + item.Test_BoreMethod_1 + " \n";
                    }
                    else
                    {
                        sumStr += "┣   直径:" + Math.Round(item._SwCAM.BoreDiameter, 2) + "\n";
                        sumStr += "┣   深度:" + Math.Round(item._SwCAM.BoreDepth, 2) + "\n";
                        sumStr += "┣   单次钻孔:" + Math.Round(item.Test_BoreSingleHole, 0) + "秒\n";
                        sumStr += "┣   扩孔时间:" + Math.Round(item.Test_BoreExpandHole,0) + "秒\n";
                        sumStr += "┣   点孔:" + item.Test_BoreDotHole + "秒\n";
                        sumStr += "┣   换刀:" + item.Test_ToolChange + "秒\n";
                        sumStr += "┣   进给率:" + item.Test_FeedRate + "(mm/min)\n";
                        sumStr += "┣   固定减少:-11秒\n";
                        sumStr += "┣   " + item.Test_BoreMethod_1 + "\n";
                    }
                    sumStr += "┣   ========= 孔 =========\n";
                    sumStr += "┣   底部时间:" + Math.Round(item.Test_BoreBottomTime, 2) + "秒\n";
                    sumStr += "┣   直径:" + Math.Round(item._SwCAM.Maxdiameter, 2) + "\n";
                    sumStr += "┣   深度:" + Math.Round(item._SwCAM.Depth - item._SwCAM.BoreDepth, 2) + "\n";
                    sumStr += "┣   单次钻孔:" + Math.Round(item.Test_SingleHole,0) + "秒\n";
                    sumStr += "┣   扩孔时间:" + Math.Round(item.Test_ExpandHole,0) + "秒\n";
                    sumStr += "┣   点孔:" + item.Test_DotHolel + "秒\n";
                    sumStr += "┣   换刀:" + item.Test_ToolChange + "秒\n";
                    sumStr += "┣   进给率:" + item.Test_FeedRate + "(mm/min)\n";
                    sumStr += "┣   " + item.Test_BoreMethod_2 + "\n";

                    sumStr += "┣   组:X" + item._SwCAM.SubFeatureCount + "\n";
                    sumStr += "┗┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┛\n";
                }
                else if (item.FeatureName.IndexOf("埋头孔") >= 0)
                {
                    sumStr += "┏┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┓\n";
                    sumStr += "┣  【" + item.FeatureName + "(X" + item._SwCAM.SubFeatureCount + ")】: " + Convert.ToInt32(Math.Round(item.TotalTime, 0)) + "秒 " + GetMin(Convert.ToInt32(Math.Round(item.TotalTime, 0))) + "\n";
                    sumStr += "┣   直径:" + Math.Round(item._SwCAM.Maxdiameter, 2) + "\n";
                    sumStr += "┣   深度:" + Math.Round(item._SwCAM.Depth, 2) + "\n";
                    sumStr += "┣   单次钻孔:" + Math.Round(item.Test_SingleHole, 0) + "秒\n";
                    sumStr += "┣   扩孔时间:" + Math.Round(item.Test_ExpandHole, 0) + "秒\n";
                    sumStr += "┣   沉头:" + item.Test_CounterBore + "秒\n";
                    sumStr += "┣   点孔:" + item.Test_DotHolel + "秒\n";
                    sumStr += "┣   换刀:" + item.Test_ToolChange + "秒\n";
                    sumStr += "┣   进给率:" + item.Test_FeedRate + "(mm/min)\n";
                    sumStr += "┣   组:X" + item._SwCAM.SubFeatureCount + "\n";
                    sumStr += "┣   " + item.Test_MethodName + "\n";
                    sumStr += "┗┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┛\n";
                }
                else if (item.FeatureName.IndexOf("孔") >= 0)
                {
                    sumStr += "┏┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┓\n";
                    sumStr += "┣  【" + item.FeatureName + "(X" + item._SwCAM.SubFeatureCount + ")】: " + Convert.ToInt32(Math.Round(item.TotalTime, 0)) + "秒 " + GetMin(Convert.ToInt32(Math.Round(item.TotalTime, 0))) + "\n";
                    sumStr += "┣   直径:" + Math.Round(item._SwCAM.Maxdiameter,2) + "\n";
                    sumStr += "┣   深度:" + Math.Round(item._SwCAM.Depth, 2) + "\n";

                    if (item._SwCAM.Maxdiameter > 20)
                    {
                        sumStr += "┣   单次时间：" + item.Test_SingleTime + "秒\n" +
                          "┣   走刀次数:" + item.Test_ProcessCount + "次\n" +
                          "┣   裁剪单长度:" + Math.Round(item.Test_CuttingLength / item.Test_ProcessCount, 1) + "mm\n" +
                          "┣   裁剪总长度:" + item.Test_CuttingLength + "mm\n" +
                          "┣   刀具直径：" + item.Test_Dia + "mm\n" +
                          "┣   下刀深度：" + item.Test_CutteDepth + "mm\n" +
                          "┣   进给率：" + item.Test_FeedRate + "(mm/min)\n" +
                          "┣   材料切割速率：" + item.Test_CuttingSpeed + "倍\n" +
                          "┣   尺寸(mm):[" + Math.Round(item._SwCAM.Bound[0], 2) + " * " + Math.Round(item._SwCAM.Bound[1], 2) + " * " + Math.Round(item._SwCAM.Depth, 2) + "]\n";
                    }
                    else
                    {

                        sumStr += "┣   单次钻孔:" + Math.Round(item.Test_SingleHole, 0) + "秒\n";
                        sumStr += "┣   扩孔时间:" + Math.Round(item.Test_ExpandHole, 0) + "秒\n";
                        sumStr += "┣   点孔:" + item.Test_DotHolel + "秒\n";
                        sumStr += "┣   换刀:" + item.Test_ToolChange + "秒\n";
                        sumStr += "┣   进给率:" + item.Test_FeedRate + "(mm/min)\n";
                    }
                    sumStr += "┣   组:X" + item._SwCAM.SubFeatureCount + "\n";
                    sumStr += "┣   " + item.Test_MethodName + "\n";
                    sumStr += "┗┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┅┛\n";
                }
                else
                {
                    sumStr += item.FeatureName + ":   " + Convert.ToInt32(Math.Round(item.TotalTime, 0)) + "秒\n";
                }
            }

            DateTime dt2 = DateTime.Now;
            TimeSpan ts = dt2 - dt1;
            string HeadStr = "【计算铣削特征时间】\n程序用时:" + ts.Seconds + "秒\n共(sec) : " + temp.ToString() + "秒 \n共(min) :" + temp2 + "分钟 \n";
            if (txtTime.Text != "" && txtTime.Text != "0")
            {
                double wc1 = 0;
                if (Convert.ToDouble(txtTime.Text) > temp2)
                     wc1 = temp / (Convert.ToDouble(txtTime.Text) * 60) * 100;
                else
                    wc1 = (Convert.ToDouble(txtTime.Text) * 60) / temp * 100;
                HeadStr += "准确率:" + Math.Round(wc1, 2) + "%\n\n";
            }
            HeadStr += "毛坯尺寸(mm):[" + Math.Round(cf.StockSize[0], 2) + " * " + Math.Round(cf.StockSize[1], 2) + " * " + Math.Round(cf.StockSize[2], 2) + "] \n";
            sumStr = HeadStr + sumStr;
            txtMsg.Text = sumStr;

        }

        public string GetMin(int sec)
        {
            if (sec.ToString().Length <= 2)
                return "";
            else
            {
                double min = Math.Round((Convert.ToDouble(sec) / 60), 1);
                return "("+ min + "分钟)";
            }
        }

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]

        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        IntPtr ProcessHwnd;
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);//设定焦点

        //[DllImport("user32.dll", EntryPoint = "PostMessage")]
        //public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        //消息发送API
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
            IntPtr hWnd,        // 信息发往的窗口的句柄
           int Msg,            // 消息ID
            int wParam,         // 参数1
            int lParam            // 参数2
        );

        private void button9_Click(object sender, EventArgs e)
        {

            //IntPtr ieMinZheng = new IntPtr(0);

            //ieMinZheng = FindWindow("#32770", "CAMWorks 消息");

            //ProcessHwnd = ieMinZheng;

            //if (!ieMinZheng.Equals(IntPtr.Zero))
            //{
            //    MessageBox.Show("True");
            //    const int BM_CLICK = 0xF5;
            //    PostMessage1(ieMinZheng, BM_CLICK, 13, 13);
            //    //const int WM_CHAR = 0x0102;
            //    // SendMessage(ProcessHwnd, 258, 13, 0);     //发送点击按钮的消息
            //    //const int WM_KEYDOWN = 0x0100;
            //    ////释放一个键   
            //    //const int WM_KEYUP = 0x0101;
            //    //SendMessage(ieMinZheng, WM_KEYDOWN, 13, 13);
            //    //SendMessage(ieMinZheng, WM_KEYUP, 13, 13);
            //    //const int BM_CLICK = 0xF5;
            //    //SendMessage(ieMinZheng, BM_CLICK, 0, 0);
            //}
            //else
            //    MessageBox.Show("False");

           // Thread thread = new Thread(new ThreadStart(CheckCAM_MSG_Thread));
           // thread.Start();
        }

        public void CheckCAM_MSG_Thread()
        {
            while (true)
            {
                IntPtr IntPtrOne = new IntPtr(0);
                IntPtrOne = FindWindow("#32770", "CAMWorks 消息");
                if (!IntPtrOne.Equals(IntPtr.Zero))
                {
                    SendKeys.SendWait("{Enter}");
                }

                IntPtr IntPtrTwo = new IntPtr(0);
                IntPtrTwo = FindWindow("#32770", "CAMWorks 警告");
                if (!IntPtrTwo.Equals(IntPtr.Zero))
                {
                    SendKeys.SendWait("{Enter}");
                }

                IntPtr IntPtrThree = new IntPtr(0);
                IntPtrThree = FindWindow("#32770", "SOLIDWORKS");
                if (!IntPtrThree.Equals(IntPtr.Zero))
                {
                    SendKeys.SendWait("{Enter}");
                }
                Thread.Sleep(1000);
            }
                
        }
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);


        private void button10_Click(object sender, EventArgs e)
        {
            IntPtr ieMinZheng = new IntPtr(0);

            ieMinZheng = FindWindow("#32770", "CAMWorks 消息");

            ProcessHwnd = ieMinZheng;

            if (!ieMinZheng.Equals(IntPtr.Zero))
            {
                MessageBox.Show("True");

                IntPtr childHwnd = FindWindowEx(ieMinZheng, IntPtr.Zero, "Button", "确定"); //通过按钮名获得按钮的句柄

                const int BM_CLICK = 0xF5;

                if (!childHwnd.Equals(IntPtr.Zero))
                {
                    MessageBox.Show("Child True");
                    SendMessage(childHwnd, BM_CLICK, 0, 0);
                    SendMessage(childHwnd, BM_CLICK, 0, 0);
                }
                else
                {
                    MessageBox.Show("Child False");
                }

                }
            else
                MessageBox.Show("False");
        }

        IntPtr IntPtrOne = new IntPtr(0);
        IntPtr IntPtrTwo = new IntPtr(0);
        IntPtr IntPtrThree = new IntPtr(0);

        IntPtr IntPtrOneChild = new IntPtr(0);
        IntPtr IntPtrTwoChild = new IntPtr(0);
        IntPtr IntPtrThreeChild = new IntPtr(0);

        const int BM_CLICK = 0xF5;

        private void button11_Click(object sender, EventArgs e)
        {
            IntPtrTwo = FindWindow("#32770", "提示信息");
            if (!IntPtrTwo.Equals(IntPtr.Zero))
            {
                MessageBox.Show("True");
                IntPtrTwoChild = FindWindowEx(IntPtrTwo, IntPtr.Zero, "Button", "确定"); //通过按钮名获得按钮的句柄
                if (!IntPtrTwoChild.Equals(IntPtr.Zero))
                {
                    MessageBox.Show("Child True");
                    SendMessage(IntPtrTwoChild, BM_CLICK, 0, 0);
                    SendMessage(IntPtrTwoChild, BM_CLICK, 0, 0);
                }
                else
                {
                    MessageBox.Show("Child False");
                }
            }
            else
            {
                MessageBox.Show("False");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            SldWorks swApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
            ModelDoc2 Part = null;

            ModelDocExtension ModelDocExtension = default(ModelDocExtension);


            Part = (ModelDoc2)swApp.ActiveDoc;

            ModelDocExtension = Part.Extension;
           




           // MessageBox.Show(docUserUnit.SpecificUnitType.ToString()); //获取 长度单位 0 毫米 1厘米 2米 3英寸
 

           // ModelDocExtension.SetUserPreferenceInteger((int)swUserPreferenceIntegerValue_e.swUnitsLinear,(int)swUserPreferenceOption_e.swDetailingNoOptionSpecified,0);

            UserUnit docUserUnit = (UserUnit)Part.GetUserUnit((int)swUserUnitsType_e.swLengthUnit);

            MessageBox.Show(docUserUnit.SpecificUnitType.ToString()); //获取 长度单位 0 毫米 1厘米 2米 3英寸

          //  Application.Exit();

        }

        private void button13_Click(object sender, EventArgs e)
        {

            //Solidworks程序对象
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));

            //打开文件
            m_ModelDoc = openModle(partfilepath);

            ModelDoc2 Part = null;
            ModelDocExtension ModelDocExtension = default(ModelDocExtension);
            Part = (ModelDoc2)m_SwApp.ActiveDoc;
            ModelDocExtension = Part.Extension;
            //设置单位为 0=毫米
            ModelDocExtension.SetUserPreferenceInteger((int)swUserPreferenceIntegerValue_e.swUnitsLinear, (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified, 0);

            //UserUnit docUserUnit = (UserUnit)Part.GetUserUnit((int)swUserUnitsType_e.swLengthUnit);
            //MessageBox.Show(docUserUnit.SpecificUnitType.ToString()); //获取 长度单位 0 毫米 1厘米 2米 3英寸

            //仅支持零部件
            if (m_ModelDoc.GetType() != (int)swDocumentTypes_e.swDocPART)
            {
                MessageBox.Show("仅支持零部件！");
                return;
            }
            bool isSheetMetal = false;
            bool isSolidBody = false;
            bool isBlank = false;
            string strError = string.Empty;

            //仅支持单实体零部件
            if (isMutiBodyOrSheetMetal(ref isBlank, ref isSheetMetal, ref isSolidBody, ref strError))
            {
                MessageBox.Show("仅支持单体零部件！");
                return;
            }

            //空白图纸
            if (isBlank)
            {
                MessageBox.Show("空白图纸！");
                return;
            }

            //不支持钣金件
            if (isSheetMetal)
            {
                MessageBox.Show("不支持钣金件！");
                return;
            }
            //是否为实体零件
            if (!isSolidBody)
            {
                MessageBox.Show("非实体零部件！");
                return;
            }


            MessageBox.Show("Over");

        }

        private void button14_Click(object sender, EventArgs e)
        {
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));

            //打开文件
            //ModelDoc2 swModel  = openModle(partfilepath);
            //swModel.ViewDisplayHiddenremoved();// 变成二位画面


            ModelDoc2 SwModel = (ModelDoc2)m_SwApp.ActiveDoc;
            ModelDocExtension swModelDocExt = default(ModelDocExtension);
            ModelView swModView = default(ModelView);
            object[] modViews = null;
            long Count = 0;

            SwModel = (ModelDoc2)m_SwApp.ActiveDoc;
            swModelDocExt = (ModelDocExtension)SwModel.Extension;

            // Get model views
            modViews = (object[])swModelDocExt.GetModelViews();

            // Get number of model views
            Count = swModelDocExt.GetModelViewCount();
            Debug.Print("Number of model views: " + Count);

            // Get the scale factor of each model view
            for (long i = 0; i < Count; i++)
            {
                swModView = (ModelView)modViews[i];
                Debug.Print("Scale factor of this model view is: " + swModView.Scale2);
                //swModView.RotateAboutCenter(0,10);
                // swModView.RotateAboutAxis(90, 0, 0, 0, 90, 90, 90);
                //swModView.TranslateBy(90,90);
                //swModView.RollBy(120);
                swModView.RotateAboutPoint(90, 180, 0, 0, 0);

            }

        }
 

        private void button15_Click(object sender, EventArgs e)
        {
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));


            ModelDoc2 swModel = (ModelDoc2)m_SwApp.ActiveDoc;


            if (cboHM.SelectedIndex == 0)
            {
                swModel.ViewDisplayHiddenremoved();// 变成二位画面 
            }
            else if (cboHM.SelectedIndex == 1)
            {
                swModel.ViewDisplayHiddengreyed();//二维画面 透视  （透视部分为虚线）
            }
            else if (cboHM.SelectedIndex == 2)
            {
                swModel.ViewDisplayWireframe();//只有边线 透视
            }
            else if (cboHM.SelectedIndex == 3)
            {
                swModel.ViewDisplayShaded();//只有边线 透视
            }
            else
            {
                // 还原 默认状态
                ModelView LoMyModelView = swModel.ActiveView;
                LoMyModelView.DisplayMode = (int)swViewDisplayMode_e.swViewDisplayMode_ShadedWithEdges;
            }

        }
        //图像另存为
        private void button16_Click(object sender, EventArgs e)
        {
            //Solidworks程序对象
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));

            m_ModelDoc = (ModelDoc2)m_SwApp.ActiveDoc;
 
            ModelView LoMyModelView; //生成缩略图
            LoMyModelView = m_ModelDoc.ActiveView;
            LoMyModelView.FrameState = (int)swWindowState_e.swWindowMaximized;
            m_ModelDoc.SaveAs("D:\\Images\\"+ txtTime.Text + ".jpg");

            txtMsg.Text += "保存完成\n";
        }

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        private void button17_Click(object sender, EventArgs e)
        {
            //Solidworks程序对象
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
            m_ModelDoc = (ModelDoc2)m_SwApp.ActiveDoc;
            m_ModelDoc.SaveAs("D:\\Images\\TEST.STL");
            MessageBox.Show("FINISH");
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string Center = ((Button)sender).Tag.ToString();
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));

            m_ModelDoc = (ModelDoc2)m_SwApp.ActiveDoc;

            if (Center == "X")
            {
                //m_ModelDoc.ViewRotateminusx();
                //m_ModelDoc.ViewRotateminusx();
                //m_ModelDoc.ViewRotateminusx();
                //m_ModelDoc.ViewRotateminusx();
                //m_ModelDoc.ViewRotateminusx();
                //m_ModelDoc.ViewRotateminusx();

                m_ModelDoc.ViewDisplayHiddenremoved();// 变成二位画面
                m_ModelDoc.ShowNamedView2("*Isometric", 1);
                m_ModelDoc.ViewZoomtofit2();
            }
            else if (Center == "Y")
            {
                //m_ModelDoc.ViewRotateminusy();
                //m_ModelDoc.ViewRotateminusy();
                //m_ModelDoc.ViewRotateminusy();
                //m_ModelDoc.ViewRotateminusy();
                //m_ModelDoc.ViewRotateminusy();
                //m_ModelDoc.ViewRotateminusy();
                m_ModelDoc.ViewDisplayHiddenremoved();// 变成二位画面
                m_ModelDoc.ShowNamedView2("*Isometric", 3);
                m_ModelDoc.ViewZoomtofit2();
            }
            else
            {
                //m_ModelDoc.ViewRotateminusz();
                //m_ModelDoc.ViewRotateminusz();
                //m_ModelDoc.ViewRotateminusz();
                //m_ModelDoc.ViewRotateminusz();
                //m_ModelDoc.ViewRotateminusz();
                //m_ModelDoc.ViewRotateminusz();
                m_ModelDoc.ViewDisplayHiddenremoved();// 变成二位画面
                m_ModelDoc.ShowNamedView2("*Isometric", 6);
                m_ModelDoc.ViewZoomtofit2();
            }

        }
        private void button21_Click(object sender, EventArgs e)
        {
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
            m_ModelDoc = (ModelDoc2)m_SwApp.ActiveDoc;
            m_ModelDoc.SelectByID("Point1@原点", "EXTSKETCHPOINT", 0, 0, 0);
            m_ModelDoc.BlankSketch();//隐藏原点
            m_ModelDoc.InsertBkgImage("\\scenes\\01 basic scenes\\11 white kitchen.p2s");
            ModelView LoMyModelView = m_ModelDoc.ActiveView;
            LoMyModelView.FrameState = (int)swWindowState_e.swWindowMaximized;
            LoMyModelView.InitializeShading();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cboHM.SelectedIndex = 0;
        }

        private void btnssxx_Click(object sender, EventArgs e)
        {
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
            m_ModelDoc = (ModelDoc2)m_SwApp.ActiveDoc;
            m_ModelDoc.ViewZoomtofit2(); 
        }

        private void button22_Click(object sender, EventArgs e)
        {
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
            m_ModelDoc = (ModelDoc2)m_SwApp.ActiveDoc; 
            m_ModelDoc.ViewRotateminusx();//向下转动
        }

        private void button23_Click(object sender, EventArgs e)
        {
            closeAllSWDoc();
        }

        private void button24_Click(object sender, EventArgs e)
        {
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
            ModelDoc2 swModel = (ModelDoc2)m_SwApp.ActiveDoc;
            swModel.Extension.InsertScene("\\scenes\\01 basic scenes\\11 white kitchen.p2s"); // 修改背景色为纯白色
            //swModel.ViewDisplayHiddenremoved();// 变成二位画面 

            //swModel.ShowNamedView2("*Isometric", 1);
            //swModel.ViewZoomtofit2();

            //swModel.ShowNamedView2("*Isometric", 3);
            //swModel.ViewZoomtofit2();

            //swModel.ShowNamedView2("*Isometric", 6);
            //swModel.ViewZoomtofit2();

         
        }
        //获得尺寸
        private string[] getBox(ModelDoc2 m_ModelDoc)
        {
            if (m_ModelDoc != null)
            {
                object boxobj = new object();
                PartDoc swPartDoc = (PartDoc)m_ModelDoc;
                if (swPartDoc != null)
                {
                    boxobj = swPartDoc.GetPartBox(true);
                    if (boxobj != null)
                    {
                        double[] BoxFaceDblArray = new double[7];
                        BoxFaceDblArray = (double[])boxobj;
                        // 成本输出对象赋值
                        string x = getLong(BoxFaceDblArray[3] - BoxFaceDblArray[0]);
                        string y = getLong(BoxFaceDblArray[4] - BoxFaceDblArray[1]);
                        string z = getLong(BoxFaceDblArray[5] - BoxFaceDblArray[2]);
                        return new string[] { x, y, z };
                    }
                }
            }
            return new string[] { "", "", "" };
        }
        public string getLong(double value)
        {
            string longstr = "";
            int factor = 1000;
            longstr = (Math.Round(Math.Abs(value * factor), 5)).ToString();
            return longstr;
        }
        private void button25_Click(object sender, EventArgs e)
        {
            m_SwApp = (SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));
            ModelDoc2 swModel = (ModelDoc2)m_SwApp.ActiveDoc;
            string[] str = getBox(swModel);
            MessageBox.Show(str[0]+","+ str[1] + ","+ str[2]);
        }
    }

}
