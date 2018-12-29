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
        /// 加工用时
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
    }
}
