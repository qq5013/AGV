﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AGVCenterLib.Model.SearchModel
{
    [DataContract]
    public class DeliveryItemSearchModel
    {
        [DataMember]
        public string DeliveryNr { get; set; }


        [DataMember]
        public string TrayNr { get; set; }
    }
}
