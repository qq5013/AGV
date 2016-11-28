﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AGVCenterLib.Data;

namespace AGVCenterLib.Service
{
    public class PositionService : ServiceBase
    {

        public PositionService(string dbString) : base(dbString)
        {

        }

        /// <summary>
        /// 是否有可用库位
        /// </summary>
        /// <param name="dispatchedPositions">已分配的库位</param>
        /// <returns></returns>
        public bool HasAvaliablePosition(List<string> dispatchedPositions=null)
        {
            return true;
        }

        /// <summary>
        /// 查询可用的位置，可以传递前一个库位做动态平衡
        /// </summary>
        /// <param name="prevFloor"></param>
        /// <param name="prevColumn"></param>
        /// <param name="prevRow"></param>
        /// <returns></returns>
        public Position FindInStockPosition(int? roadMachineIndex)
        {
            return new Position()
            {
                Floor = new Random().Next(10),
                Column = new Random().Next(20),
                Row = new Random().Next(30)
            };
        }
    }
}