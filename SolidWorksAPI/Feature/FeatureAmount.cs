using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
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
        /// 加工用时 （秒）
        /// </summary>
        public double TotalTime { get; set; }
        /// <summary>
        /// 特征金额
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 铣削特征明细
        /// </summary>
        public SwCAM_Mill _SwCAM;
        /// <summary>
        /// 铣削特征明细
        /// </summary>
        public SwCAM_Turn _SwCAM_Turn;

        public int Test_ProcessCount;//切削次数
        public double Test_SingleTime;//单次时间
        public double Test_Dia;//刀具直径
        public double[] Test_IsLandSize;//岛屿尺寸
        public double Test_IsLandTime;//岛屿耗时
        public double Test_IsLandCount;//岛屿数量
        public string Test_MethodName = "";//执行方法名
        public double Test_CuttingLength;//裁剪长度
        public double Test_FeedRate;//进给率
        public double Test_CutteDepth;//下刀深度
        public double Test_CuttingSpeed;//材料切割速率
        public double Test_DotHolel;//点孔时间
        public double Test_SingleHole;//单次的钻孔时间
        public double Test_ExpandHole;//扩孔时间
    }
}
