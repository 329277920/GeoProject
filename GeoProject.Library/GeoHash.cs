using System;
using System.Text;

namespace GeoProject.Library
{
    /// <summary>
    /// 提供对GeoHash的编码与解码操作
    /// </summary>
    public sealed class GeoHash
    {       
        private static string Base32Charset;

        private static byte[] Bits;

        private static string[][] Neighbors;

        private static string[][] Borders;

        /// <summary>
        /// 初始化相关参数
        /// </summary>
        static GeoHash()
        {            
            Base32Charset = "0123456789bcdefghjkmnpqrstuvwxyz";
            Bits = new byte[] { 1, 2, 4, 8, 16 };
            Neighbors = new string[][] 
            {
                new[]{
                          "p0r21436x8zb9dcf5h7kjnmqesgutwvy", // Top
                          "bc01fg45238967deuvhjyznpkmstqrwx", // Right
                          "14365h7k9dcfesgujnmqp0r2twvyx8zb", // Bottom
                          "238967debc01fg45kmstqrwxuvhjyznp", // Left
                     },
                new[]
                     {
                          "bc01fg45238967deuvhjyznpkmstqrwx", // Top
                          "p0r21436x8zb9dcf5h7kjnmqesgutwvy", // Right
                          "238967debc01fg45kmstqrwxuvhjyznp", // Bottom
                          "14365h7k9dcfesgujnmqp0r2twvyx8zb", // Left
                     }
            };
            Borders = new string[][]
            {
                new[] { "prxz", "bcfguvyz", "028b", "0145hjnp" },
                new[] { "bcfguvyz", "prxz", "0145hjnp", "028b" }
            };
        }

        /// <summary>
        /// 将一个经纬度值编码成GeoHash值
        /// </summary>
        /// <param name="longitude">经度值</param>
        /// <param name="latitude">纬度值</param>
        /// <param name="codingLen">编码长度，可取值为1-11</param>
        /// <returns>返回GeoHash值</returns>
        public string Encode(double longitude, double latitude, int codingLen = 11)
        {
            byte tempBits = 0;
            StringBuilder tempHash = new StringBuilder(codingLen);
            var longRange = new double[] { -180, 0, 180 };
            var latRange = new double[] { -90, 0, 90 };
            bool isLat = false;

            // GeoHash以5个位为一组进行编码，下面的一个循环，将有5次拆分操作 
            while (codingLen-- > 0)
            {
                tempBits = 0;
                for (var i = 4; i >= 0; i--)
                {
                    var tempRange = isLat ? latRange : longRange;
                    var tempValue = isLat ? latitude : longitude;

                    // 位于第一个区间，保持默认值
                    if (tempValue >= tempRange[0] && tempValue <= tempRange[1])
                    {
                        tempRange[2] = tempRange[1];                        
                    }
                    // 位于第二个区间，设置位1，设置区间值
                    else
                    {
                        tempRange[0] = tempRange[1];                       
                        tempBits |= Bits[i];
                    }
                    // 设置区间值
                    tempRange[1] = tempRange[0] + (Math.Abs(Math.Abs(tempRange[2]) - Math.Abs(tempRange[0]))) / 2;

                    isLat = !isLat;
                }
                tempHash.Append(Base32Charset[tempBits]);
            }            
           
            return tempHash.ToString();
        }

        /// <summary>
        /// 将一个GeoHash值解码成经纬度
        /// </summary>
        /// <param name="geoHash">GeoHash串</param>
        /// <returns></returns>
        public unsafe double[] Decode(string geoHash)
        {
            bool isLat = false;
            var longRange = new double[] { -180, 0, 180 };
            var latRange = new double[] { -90, 0, 90 };
            double latitude = 0.0;
            double longitude = 0.0;
            foreach (var c in geoHash)
            {
                // 获得Hash中某个字符对应的数值
                var temp = Base32Charset.IndexOf(c);
                for (var i = Bits.Length - 1; i >= 0; i--)
                {
                    var tempRange = isLat ? latRange : longRange;
                    var tempValue = isLat ? &latitude : &longitude;
                    // 位于第一个区间，取最小值
                    if ((temp & Bits[i]) == 0)
                    {
                        *tempValue = tempRange[0];
                        tempRange[2] = tempRange[1];
                    }
                    // 位于第二个区间，取中间值
                    else
                    {
                        *tempValue = tempRange[1];
                        tempRange[0] = tempRange[1];                     
                    }

                    // 设置区间值
                    tempRange[1] = tempRange[0] + (Math.Abs(Math.Abs(tempRange[2]) - Math.Abs(tempRange[0]))) / 2;

                    isLat = !isLat;
                }
            }
            return new double[] { longitude,latitude };
        }

        /// <summary>
        /// 获取某个geoHash相邻的顶部区块
        /// </summary>
        /// <param name="geoHash">当前geoHash值</param>
        /// <returns>返回顶部区块的geoHash值</returns>
        public string Top(string geoHash)
        {
            return this.CalculateAdjacent(geoHash, Direction.Top);
        }

        /// <summary>
        /// 获取某个geoHash相邻的左边区块
        /// </summary>
        /// <param name="geoHash">当前geoHash值</param>
        /// <returns>返回左边区块的geoHash值</returns>
        public string Left(string geoHash)
        {
            return this.CalculateAdjacent(geoHash, Direction.Left);
        }

        /// <summary>
        /// 获取某个geoHash相邻的底部区块
        /// </summary>
        /// <param name="geoHash">当前geoHash值</param>
        /// <returns>返回底部区块的geoHash值</returns>
        public string Bottom(string geoHash)
        {
            return this.CalculateAdjacent(geoHash, Direction.Bottom);
        }

        /// <summary>
        /// 获取某个geoHash相邻的右边区块
        /// </summary>
        /// <param name="geoHash">当前geoHash值</param>
        /// <returns>返回右边区块的geoHash值</returns>
        public string Right(string geoHash)
        {
            return this.CalculateAdjacent(geoHash, Direction.Right);
        }


        /// <summary>
        /// 参考网络解决方案        
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private string CalculateAdjacent(string hash, Direction direction)
        {
            hash = hash.ToLower();

            char lastChr = hash[hash.Length - 1];
            int type = hash.Length % 2;
            var dir = (int)direction;
            string nHash = hash.Substring(0, hash.Length - 1);

            if (Borders[type][dir].IndexOf(lastChr) != -1)
            {
                nHash = CalculateAdjacent(nHash, (Direction)dir);
            }
            return nHash + Base32Charset[Neighbors[type][dir].IndexOf(lastChr)];
        }

        private enum Direction
        {
            Top = 0,
            Right = 1,
            Bottom = 2,
            Left = 3
        }
    }

}
