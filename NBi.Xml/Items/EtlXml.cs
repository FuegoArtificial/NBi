﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using NBi.Core.Etl;
using System.ComponentModel;

namespace NBi.Xml.Items
{
    public class EtlXml: ExecutableXml, IEtl 
    {
        [XmlAttribute("version")]
        public string Version { get; set; }
        
        [XmlAttribute("server")]
        public string Server { get; set; }

        [XmlAttribute("path")]
        public string Path { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("username")]
        public string UserName { get; set; }

        [XmlAttribute("password")]
        public string Password { get; set; }

        [XmlAttribute("catalog")]
        public string Catalog { get; set; }

        [XmlAttribute("folder")]
        public string Folder { get; set; }

        [XmlAttribute("project")]
        public string Project { get; set; }

        [DefaultValue(false)]
        [XmlAttribute("bits-32")]
        public bool Is32Bits { get; set; }

        [DefaultValue(30)]
        [XmlAttribute("timeout")]
        public int Timeout { get; set; }

        [XmlIgnore]
        public List<EtlParameter> Parameters
        {
            get
            {
                return InternalParameters.ToList<EtlParameter>();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        [XmlElement("parameter")]
        public List<EtlParameterXml> InternalParameters { get; set; }

        public EtlXml()
        {
            InternalParameters = new List<EtlParameterXml>();
            Version = "SqlServer2014";
        }
    }
}
