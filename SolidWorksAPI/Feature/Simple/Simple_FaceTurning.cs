using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidWorksAPI
{
    /// <summary>
    /// 端面铣
    /// </summary>
    public class Simple_FaceTurning:SimpleTurning
    {
        /// <summary>
        /// 裁剪长度
        /// </summary>
        public double CuttingLength { get; set; }

        public Simple_FaceTurning(double Dia, double CuttingLength, int NoOfPlaces, Materials _Materials)
        {
            this.Dia = Dia;
            this.CuttingLength = CuttingLength;
            this.NoOfPlaces = NoOfPlaces;
            this.No = 1;
            this.CuttingSpeed = GetCuttingSpeed();
            this.FeedPer = 0.16;
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
            ///碳钢200 合金钢200 不锈钢150 铝合金300
            switch (this._Materials)
            {
                case Materials.Alloy:
                    return 200;
                case Materials.Stainless:
                    return 150;
                default:
                    return 300;
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
            this.CuttingTime = (this.CuttingLength + this.ReserveLength) * this.NoOfPlaces * 60 / this.FeedRate;
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
