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
    /// 材料
    /// </summary>
    public enum Materials
    {
        /// <summary>
        /// 铝
        /// </summary>
        Aluminum =0,
        /// <summary>
        /// 铜
        /// </summary>
        Copper=1,
        /// <summary>
        /// 合金钢
        /// </summary>
        Alloy=2,
        /// <summary>
        /// 碳钢
        /// </summary>
        Carbon=3,
        /// <summary>
        /// 不锈钢
        /// </summary>
        Stainless=4,
        /// <summary>
        /// 塑料
        /// </summary>
        Plastic=5
    }








}
