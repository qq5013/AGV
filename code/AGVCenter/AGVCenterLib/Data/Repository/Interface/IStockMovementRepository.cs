﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGVCenterLib.Data.Repository.Interface
{
    public interface IStockMovementRepository
    {
        void Create(StockMovement entity);
    }
}
