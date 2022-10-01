
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

namespace DS2S_META
{
    public class SavedPos : IComparable<SavedPos>
    {
        public override string? ToString() => Name;

        [XmlAnyElement("NameXmlComment")]
        public XmlComment? NameXmlComment { get => GetType().GetXmlComment(); set { } }

        [XmlComment("string")]
        public string? Name { get; set; }


        [XmlAnyElement("XXmlComment")]
        public XmlComment? XXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlComment("decimal")]
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }


        [XmlAnyElement("PlayerStateXmlComment")]
        public XmlComment? PlayerStateXmlComment { get { return GetType().GetXmlComment(); } set { } }

        [XmlComment("HP & Stam = int. FollowCam is a byte array as Base64")]
        public State.PlayerState PlayerState { get; set; }

        public int CompareTo(SavedPos? other)
        {
            if (Name == null)
                return -1;
            return Name.CompareTo(other?.Name);
        }

        private static XmlSerializer XML = new XmlSerializer(typeof(List<SavedPos>));

        private static string SavedPositions = $"{GetTxtResourceClass.ExeDir}/Resources/SavedPositions.xml";

        public static List<SavedPos>? GetSavedPositions()
        {
            var positions = new List<SavedPos>();
            if (File.Exists(SavedPositions))
            {
                using var stream = new FileStream(SavedPositions, FileMode.Open);
                positions = (List<SavedPos>?)XML.Deserialize(stream);
            }

            return positions;
        }

        public static void Save(List<SavedPos> positions)
        {
            positions.Sort();
            using (FileStream stream = new FileStream(SavedPositions, FileMode.Create))
            {
                XML.Serialize(stream, positions);
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(SavedPositions);
            XmlComment info = doc.CreateComment("Comments denote following value types. FollowCam is a byte array for camera data.");
            XmlElement? root = doc.DocumentElement;
            doc.InsertBefore(info, root);
            doc.Save(SavedPositions);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlCommentAttribute : Attribute
    {
        public XmlCommentAttribute(string value)
        {
            this.Value = value;
        }

        public string Value { get; set; }
    }

    public static class XmlCommentExtensions
    {
        const string XmlCommentPropertyPostfix = "XmlComment";

        static XmlCommentAttribute? GetXmlCommentAttribute(this Type type, string memberName)
        {
            var member = type.GetProperty(memberName);
            if (member == null)
                return null;
            var attr = member.GetCustomAttribute<XmlCommentAttribute>();
            return attr;
        }

        public static XmlComment? GetXmlComment(this Type type, [CallerMemberName] string memberName = "")
        {
            var attr = GetXmlCommentAttribute(type, memberName);
            if (attr == null)
            {
                if (memberName.EndsWith(XmlCommentPropertyPostfix))
                    attr = GetXmlCommentAttribute(type, memberName.Substring(0, memberName.Length - XmlCommentPropertyPostfix.Length));
            }
            if (attr == null || string.IsNullOrEmpty(attr.Value))
                return null;
            return new XmlDocument().CreateComment(attr.Value);
        }
    }
}
