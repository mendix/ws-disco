//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Services.Description;
    using System.Web.Services.Discovery;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Runtime.CompilerServices;

    public sealed class WebServicesSection {

        static object ClassSyncObject {
            get {
                if (classSyncObject == null) {
                    object o = new object();
                    Interlocked.CompareExchange(ref classSyncObject, o, null);
                }
                return classSyncObject;
            }
        }

        public static WebServicesSection Current {
            get {
                if (current == null) {
                    current = new WebServicesSection();
                }
                return current;
            }
        }

        internal XmlSerializer DiscoveryDocumentSerializer {
            get {
                if (this.discoveryDocumentSerializer == null) {
                    lock (WebServicesSection.ClassSyncObject) {
                        if (this.discoveryDocumentSerializer == null) {
                            XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();
                            XmlAttributes attrs = new XmlAttributes();
                            foreach (Type discoveryReferenceType in this.DiscoveryReferenceTypes) {
                                object[] xmlElementAttribs = discoveryReferenceType.GetCustomAttributes(typeof(XmlRootAttribute), false);
                                if (xmlElementAttribs.Length == 0) {
                                    throw new InvalidOperationException(Res.GetString(Res.WebMissingCustomAttribute, discoveryReferenceType.FullName, "XmlRoot"));
                                }
                                string name = ((XmlRootAttribute)xmlElementAttribs[0]).ElementName;
                                string ns = ((XmlRootAttribute)xmlElementAttribs[0]).Namespace;
                                XmlElementAttribute attr = new XmlElementAttribute(name, discoveryReferenceType);
                                attr.Namespace = ns;
                                attrs.XmlElements.Add(attr);
                            }
                            attrOverrides.Add(typeof(DiscoveryDocument), "References", attrs);
                            this.discoveryDocumentSerializer = new DiscoveryDocumentSerializer();
                        }
                    }
                }
                return discoveryDocumentSerializer;
            }
        }

        internal Type[] DiscoveryReferenceTypes {
            get { return this.discoveryReferenceTypes; }
        }

        // Object for synchronizing access to the entire class( avoiding lock( typeof( ... )) )
        static object classSyncObject = null;

        static WebServicesSection current = null;

        Type[] discoveryReferenceTypes = new Type[] { typeof(DiscoveryDocumentReference), typeof(ContractReference), typeof(SchemaReference), typeof(System.Web.Services.Discovery.SoapBinding) };
        XmlSerializer discoveryDocumentSerializer = null;
    }
}
