using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{

    public class HZ_Extension
    {
        public const string SLDPRT = ".SLDPRT";
        public const string PRT = ".PRT";
        public const string STEP = ".STEP";
        public const string STP = ".STP";
        public const string IGS = ".IGS";
        public const string IGES = ".IGES";

        //目前调研项
        public const string STL = ".STL";
        public const string IPT = ".IPT";
        public const string X_T = ".X_T";
    }
    public class HZ_CostingOutput
    {

        /// <summary>
        /// 模板名称
        /// </summary>
        public string templateName { get; set; }
        /// <summary>
        /// 币种代码
        /// </summary>
        public string currencyCode { get; set; }
        /// <summary>
        /// 币种名称
        /// </summary>
        public string currencyName { get; set; }
        /// <summary>
        /// 货币分隔符
        /// </summary>
        public string currencySeparator { get; set; }
        /// <summary>
        /// 总制造成本
        /// </summary>
        public double totalManufacturingCost { get; set; }
        /// <summary>
        /// 材料成本
        /// </summary>
        public double materialCosts { get; set; }
        /// <summary>
        /// 总费用
        /// </summary>
        public double totalCostrueToCharge { get; set; }
        /// <summary>
        /// 生产总成本
        /// </summary>
        public double totalCostrueToManufacture { get; set; }

        /// <summary>
        /// 总耗时
        /// </summary>
        public string totalCostingTime { get; set; }

        // 机加工成本属性
        /// <summary>
        /// 当前材料
        /// </summary>
        public string currentMaterial { get; set; }
        /// <summary>
        /// 当前材料库
        /// </summary>
        public string currentMaterialClass { get; set; }

        /// <summary>
        /// 零件总大小
        /// </summary>
        public int totalNum { get; set; }

        /// <summary>
        /// 批量大小
        /// </summary>
        public int lotSize { get; set; }

        /// <summary>
        /// 记录仓库类型
        /// </summary>
        public int stockType { get; set; }

        /// <summary>
        /// 热处理
        /// </summary>
        public string heatTreatment { get; set; }

        /// <summary>
        /// 序列号ID
        /// </summary>
        public string seriesId { get; set; }

        /// <summary>
        /// 表面处理
        /// </summary>
        public string surfaceAreaTreatment { get; set; }

        /// <summary>
        /// 质量属性
        /// </summary>
        public HZ_MassProperty massProperty { get; set; }

        /// <summary>
        /// 特征信息
        /// </summary>
        public List<HZ_FeatCost> featCostList { get; set; }
    }

    /// <summary>
    /// 零件类型（6种）
    /// </summary>
    public enum HZ_EnumType
    {
        /// <summary>
        /// 简单车削
        /// </summary>
        hz_SimpleTurning,
        /// <summary>
        /// 复杂车削
        /// </summary>
        hz_ComplexTurning,
        /// <summary>
        /// Turn Mill
        /// </summary>
        hz_TurnMill,
        /// <summary>
        /// 三轴
        /// </summary>
        hz_3AxisMilling,
        /// <summary>
        /// 四轴
        /// </summary>
        hz_4AxisMilling,
        /// <summary>
        /// 五轴
        /// </summary>
        hz_5AxisMilling
    }
    /// <summary>
    /// 单位类型
    /// </summary>
    public enum HZ_Unit
    {
        /// <summary>
        /// 毫米
        /// </summary>
        hz_mm,
        /// <summary>
        /// 米
        /// </summary>
        hz_m
    }

    public enum HZ_StockType
    {
        Unknown = 0,    //未知
        Block = 1,          //块
        Plate = 2,          //板块
        Cylinder = 3,     //圆柱
        Custom = 4      //自定义
    }

    public enum HZ_BizCode
    {
        Success = 200,
        Failed = 201
    }
    /// <summary>
    /// CAMWork 特征类
    /// </summary>
    public class SwCAM
    {
        /// <summary>
        /// 特征名称
        /// </summary>
        public string FeatureName { get; set; } 
        /// <summary>
        /// 特征类型
        /// </summary>
        public int FeatureType { get; set; }
        /// <summary>
        /// 轴
        /// </summary>
        public double[] Axis { get; set; }
        /// <summary>
        /// 是否抑制
        /// </summary>
        public bool Suppressed { get; set; }
        /// <summary>
        /// 开始位置 x,y,z
        /// </summary>
        public double[] StartPosition {get;set;}
        /// <summary>
        /// 形状(CWShpeType_e)
        /// </summary>
        public int Shape { get; set; }
        /// <summary>
        /// 角
        /// </summary>
        public double Sharp { get; set; }
        /// <summary>
        /// 深度
        /// </summary>
        public double Depth { get; set; }
        /// <summary>
        /// 策略名称
        /// </summary>
        public string Tactics { get; internal set; }
        /// <summary>
        /// 策略ID
        /// </summary>
        public int TacticsID { get; internal set; }
        /// <summary>
        /// 同 策略ID  tacticsID
        /// </summary>
        public int TdbIdForAttribute { get; internal set; }
        /// <summary>
        /// 顶部圆角半径
        /// </summary>
        public double TopFilletRadius { get; internal set; }
        /// <summary>
        /// 直径
        /// </summary>
        public double Maxdiameter { get; internal set; }
        /// <summary>
        /// 特征类型字符串
        /// </summary>
        public string VolumeTypeString { get; internal set; }
        /// <summary>
        /// 精加工半径 mm
        /// </summary>
        public double Mindiameter { get; internal set; }
        /// <summary>
        /// 底部半径 mm
        /// </summary>
        public double BottomRadius { get; set; }
        /// <summary>
        /// 倒角
        /// </summary>
        public double Chamfer { get; set; }
        /// <summary>
        /// 边界框，长,宽,高
        /// </summary>
        public double[] Bound { get; internal set; }
        /// <summary>
        /// 子特征个数
        /// </summary>
        public int SubFeatureCount { get; internal set; }
        /// <summary>
        /// 圆角半径
        /// </summary>
        public double CornerRadius { get; internal set; }
        /// <summary>
        /// 是否为锥形
        /// </summary>
        public bool Taper { get; internal set; }
        /// <summary>
        /// 锥角度
        /// </summary>
        public double TaperAngle { get; internal set; }
        /// <summary>
        /// 特征类型，（枚举CWVolumeType_e）
        /// </summary>
        public int VolumeType { get; internal set; }
        /// <summary>
        /// 子特征
        /// </summary>
        public List<SwCAM> SubFeatureList { get; internal set; }
        /// <summary>
        /// 多阶 MS孔
        /// </summary>
        public List<SwMultiStep> SubMultiStep { get; set; }

        public double DistanceUptoStock { get; internal set; }
        /// <summary>
        /// 岛屿
        /// </summary>
        public List<Island> Islands { get; internal set; }
        /// <summary>
        /// 岛屿数量
        /// </summary>
        public int IslandCount { get; internal set; }
        /// <summary>
        /// 是否穿过 
        /// </summary>
        public int ThroughOrblind { get; internal set; }
        /// <summary>
        /// 镗削直径
        /// </summary>
        public double BoreDiameter { get; set; }
        /// <summary>
        /// 镗削深度
        /// </summary>
        public double BoreDepth { get; set; }
        /// <summary>
        /// 沉孔直径
        /// </summary>
        public double SinkDiameter { get; set; }
        /// <summary>
        /// 沉孔角度
        /// </summary>
        public double SinkAngle { get; set; }
        /// <summary>
        /// 锥度信息
        /// </summary>
        public TaperInfo Tapers { get; set; }
        public int TDBFeatID { get; internal set; }
    }

    public class Island
    {
        public double Depth { get; internal set; }
    }

    /// <summary>
    /// 阶梯参数
    /// </summary>
    public class SwMultiStep
    {
        /// <summary>
        /// 阶梯类型  1圆柱，2倒角，3圆弧过度
        /// </summary>
        public int MultiSetpType { get; set; }
        /// <summary>
        /// 枚举字符串
        /// </summary>
        public string MultiSetpName { get; set; }
        /// <summary>
        /// 顶部直径
        /// </summary>
        public double TopDiameter { get; set; }
        /// <summary>
        /// 螺纹内径
        /// </summary>
        public double MinorDiameter { get; set; }
        /// <summary>
        /// 直径
        /// </summary>
        public double Diameter { get; set; }
        /// <summary>
        /// 距离
        /// </summary>
        public double Distance { get; set; }
        /// <summary>
        /// 深度
        /// </summary>
        public double Depth { get; set; }
        /// <summary>
        /// 角
        /// </summary>
        public double Angle { get; set; }
    }
    /// <summary>
    /// 锥度信息
    /// </summary>
    public class TaperInfo
    {
        /// <summary>
        /// 锥度-角度
        /// </summary>
        public double TaperAngle { get; set; }
        /// <summary>
        /// 锥度-高
        /// </summary>
        public double TaperHeight { get; set; }
        /// <summary>
        /// 顶部倒角-角度
        /// </summary>
        public double TopChamferAngle { get; set; }
        /// <summary>
        /// 顶部倒角-高
        /// </summary>
        public double TopChamferHeight { get; set; }
        /// <summary>
        /// 底部圆弧过度面-半径
        /// </summary>
        public double BottomArcFaceRadius { get; set; }
        /// <summary>
        /// 底部圆弧过度面-高
        /// </summary>
        public double BottomArcFaceHeight { get; set; }
    }

    /// <summary>
    /// 特征计算金额
    /// </summary>
    public class FeatureAmount
    {
        /// <summary>
        /// 特征名称
        /// </summary>
        public string FeatureName { get; set; }
        /// <summary>
        /// 特征金额
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 特征明细
        /// </summary>
        public SwCAM _SwCAM;
    }


}
