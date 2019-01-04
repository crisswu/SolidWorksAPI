using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SolidWorksAPI
{
    /// <summary>
    /// 3轴 粗铣
    /// </summary>
    public class Axis3_SurfaceRoughMilling : Axis3Milling
    {
        /// <summary>
        /// 棱角半径
        /// </summary>
        public double EdgRadius { get; set; }
        /// <summary>
        /// 删除体积
        /// </summary>
        public double RemoveVolume { get; set; }
        /// <summary>
        /// 切削深度
        /// </summary>
        public double CuttingDepth { get; set; }
        /// <summary>
        /// 重叠率
        /// </summary>
        public double overlapRate { get; set; }
        /// <summary>
        /// 材料清除率
        /// </summary>
        public double MaterialRemoveRate { get; set; }

        public Axis3_SurfaceRoughMilling(double Dia,double RemoveVolume,double CuttingDepth,double overlapRate,int Number, Materials _Materials)
        {
            this.Dia = Dia; 
            this.No = 4;  
            this.FeedPer = 0.1;
            this.ReserveLength = 5;
            this._Materials = _Materials; 
            this.NoOfPlaces = Number;
            this.RemoveVolume = RemoveVolume;
            this.CuttingDepth = CuttingDepth;
            this.overlapRate = overlapRate;
            this.CuttingSpeed = GetCuttingSpeed();
            Calculate_SpindleSpeed();
            Calculate_FeedRate();
            Calculate_CuttingTime();
            Calculate_TotalTime();
            Calculate_MaterialRemoveRate();
        }
        /// <summary>
        /// 计算材料清除率
        /// </summary>
        private void Calculate_MaterialRemoveRate()
        {
            // 计算公式 =( L4^2*2*ACOS((L4-N4)/L4)/2-(L4-N4)*L4*SIN(ACOS((L4-N4)/L4)))*K4*O4/100
            this.MaterialRemoveRate = (Math.Pow(this.EdgRadius, 2) * 2 * Math.Acos((this.EdgRadius - this.CuttingDepth) / this.EdgRadius) / 2 - (this.EdgRadius - CuttingDepth) * this.EdgRadius * Math.Sin(Math.Cos((this.EdgRadius - this.CuttingDepth) / this.EdgRadius))) * this.FeedRate * this.overlapRate / 100;
        }
        /// <summary>
        /// 裁剪时间
        /// </summary>
        protected override void Calculate_CuttingTime()
        {
            this.CuttingTime = this.RemoveVolume / this.MaterialRemoveRate * 60;
        }
        /// <summary>
        /// 计算 进给速率
        /// </summary>
        protected override void Calculate_FeedRate()
        {
            this.FeedRate = this.No * this.FeedPer * this.SpindleSpeed;
        }
        /// <summary>
        /// 计算 主轴转速
        /// </summary>
        protected override void Calculate_SpindleSpeed()
        {
            if ((this.CuttingSpeed * 1000 / (this.Dia * 3.14) - 6500) >= 0)
            {
                this.SpindleSpeed = 6500;
            }
            else
            {
                this.SpindleSpeed = this.CuttingSpeed * 1000 / (this.Dia * 3.14);
            }
        }
        /// <summary>
        /// 加工时间合计
        /// </summary>
        protected override void Calculate_TotalTime()
        {
            this.TotalTime = this.AtcTime + this.OtherTime + this.CuttingTime;
        }

        protected override int GetCuttingSpeed()
        { 
            switch (this._Materials)
            {
                case Materials.Carbon:
                    return 120;
                case Materials.Alloy:
                    return 120;
                case Materials.Stainless:
                    return 80;
                case Materials.Aluminum:
                    return 200;
                default:
                    return 200;
            }
        }
    }
}
