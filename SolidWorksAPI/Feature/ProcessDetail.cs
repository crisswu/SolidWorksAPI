using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 工序明细
    /// </summary>
    public class ProcessDetail
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string OperationName { get; set; }
        /// <summary>
        /// 特征名称
        /// </summary>
        public string FeatureName { get; set; }
        /// <summary>
        /// 工序所用时间(min)
        /// </summary>
        public double ToolpathTotalTime { get; set; }
        /// <summary>
        /// 刀具路线长度(mm)
        /// </summary>
        public double ToolpathTotalLength { get; set; }
    }
}
