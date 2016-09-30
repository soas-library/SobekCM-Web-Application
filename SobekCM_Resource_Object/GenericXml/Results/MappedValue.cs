﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SobekCM.Resource_Object.GenericXml.Reader;

namespace SobekCM.Resource_Object.GenericXml.Results
{
    public class MappedValue
    {
        public GenericXmlPath Path { get; set; }

        public string Value { get; set; }

        public string Mapping { get; set; }
    }
}
