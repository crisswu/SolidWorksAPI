using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 简单工艺
    /// </summary>
    public abstract class SimpleTurning
    {
        /// <summary>
        /// 直径
        /// </summary>
        public double Dia { get; set; }
        /// <summary>
        /// 工序数量
        /// </summary>
        public int No { get; set; }
        /// <summary>
        /// 裁剪速度
        /// </summary>
        public int CuttingSpeed { get; set; }
        /// <summary>
        ///  每分钟走刀量
        /// </summary>
        public double FeedPer { get; set; }
        /// <summary>
        /// 主轴转速
        /// </summary>
        public double SpindleSpeed { get; set; }
        /// <summary>
        /// 进给速率
        /// </summary>
        public double FeedRate { get; set; }
        /// <summary>
        /// 储备的长度
        /// </summary>
        public int ReserveLength { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int NoOfPlaces { get; set; }
        /// <summary>
        /// 裁剪时间
        /// </summary>
        public double CuttingTime { get; set; }
        /// <summary>
        /// ATC时间
        /// </summary>
        public double AtcTime { get; set; } = 3;
        /// <summary>
        /// 其它时间
        /// </summary>
        public double OtherTime { get; set; } = 3;
        /// <summary>
        /// 总计用时
        /// </summary>
        public double TotalTime { get; set; }
        /// <summary>
        /// 材料
        /// </summary>
        public Materials _Materials { get; set; }
        /// <summary>
        /// 获取材料切割速度
        /// </summary>
        /// <returns></returns>
        protected abstract int GetCuttingSpeed();
        /// <summary>
        /// 加工时间合计
        /// </summary>
        protected abstract void Calculate_TotalTime();
        /// <summary>
        /// 计算 主轴转速
        /// </summary>
        protected abstract void Calculate_SpindleSpeed();
        /// <summary>
        /// 计算 进给速率
        /// </summary>
        protected abstract void Calculate_FeedRate();
        /// <summary>
        /// 裁剪时间
        /// </summary>
        protected abstract void Calculate_CuttingTime();

    }
}
