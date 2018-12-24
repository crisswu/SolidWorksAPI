using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 孔
    /// </summary>
    public class Simple_Drilling : SimpleTurning
    {
        public double HoleDepth { get; set; }

        /// <summary>
        /// 初始化构造
        /// </summary>
        /// <param name="Dia">直径</param>
        /// <param name="HoleDepth">深度</param>
        /// <param name="NoOfPlaces">数量</param>
        public Simple_Drilling(double Dia, double HoleDepth, int NoOfPlaces,Materials _Materials)
        {
            this.Dia = Dia;
            this.HoleDepth = HoleDepth;
            this.NoOfPlaces = NoOfPlaces;
            this.No = 2;
            this.CuttingSpeed = GetCuttingSpeed();
            this.FeedPer = 0.07;
            this.ReserveLength = 5;
            this._Materials = _Materials;
            Calculate_SpindleSpeed();
            Calculate_FeedRate();
            Calculate_CuttingTime();
            Calculate_TotalTime();
        }
        /// <summary>
        /// 获取材料切割速度
        /// </summary>
        /// <returns></returns>
        protected override int GetCuttingSpeed()
        {
            ///碳钢40 不锈钢30 铝合金80
            switch (this._Materials)
            {
                case Materials.Carbon:
                    return 40;
                case Materials.Stainless:
                    return 30;
                default:
                    return 80;
            }
        }
        /// <summary>
        /// 计算 主轴转速
        /// </summary>
        protected override void Calculate_SpindleSpeed()
        {
            if ((this.CuttingSpeed * 1000 / (this.Dia * 3.14) - 3500) >= 0)
            {
                this.SpindleSpeed = 3500;
            }
            else
            {
                this.SpindleSpeed = this.CuttingSpeed * 1000 / (this.Dia * 3.14);
            }
        }
        /// <summary>
        /// 计算 进给速率
        /// </summary>
        protected override void Calculate_FeedRate()
        {
            this.FeedRate = this.No * this.FeedPer * this.SpindleSpeed;
        }
        /// <summary>
        /// 裁剪时间
        /// </summary>
        protected override void Calculate_CuttingTime()
        {
            this.CuttingTime = (this.HoleDepth + this.ReserveLength) * this.NoOfPlaces * 60 / this.FeedRate;
        }
        /// <summary>
        /// 加工时间合计
        /// </summary>
        protected override void Calculate_TotalTime()
        {
            this.TotalTime = this.AtcTime + this.OtherTime + this.CuttingTime;
        }
    }
}
